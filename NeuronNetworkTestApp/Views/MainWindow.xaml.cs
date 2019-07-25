using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using NeuralNetworks;
using NeuralNetworks.NeuralNetworks;
using NeuronNetworkTestApp.Models;

namespace NeuronNetworkTestApp.Views
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool TrainMode { get; set; }
        List<double[]> TrainData { get; set; }
        NeuralNetwork NeuralNetwork { get; set; }
        List<MapItem> Map { get; set; }
        List<MapItem> LastMapBackup { get; set; }
        private int MapSize = 10;
        MapItem Player { get; set; }
        MapItem Finish { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            var topology = new Topology(8, 2, 0.2, 4);
            NeuralNetwork = new NeuralNetwork(topology);
            InitializeData();
        }

        private void InitializeData()
        {
            TrainMode = false;
            Map = MapCreator.CreateDefaultMap(MapSize);
            var playerX = MapSize / 2;
            var playerY = MapSize / 2;
            Map = MapCreator.ChangeItemType(Map, playerX, playerY, MapItemType.Player);//Add player
            Map = MapCreator.ChangeItemType(Map, 5, 6, MapItemType.Wall); //Add Wall
            Map = MapCreator.ChangeItemType(Map, 4, 5, MapItemType.Wall); //Add Wall
            Map = MapCreator.CreateRandomWall(Map, 3, MapSize);
            Map = MapCreator.CreateRandomFinish(Map, MapSize);
            CreateMap();
            Player = Moving.GetPlayer(Map);
            Finish = Moving.GetFinish(Map);
        }

        private void Move()
        {

        }


        private void ButtonStart_OnClick(object sender, RoutedEventArgs e)
        {
            Move();
        }
        private void TextBlockLog_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBlockLog.ScrollToEnd();
        }
        private void CreateMap()
        {
            this.Height = MapSize * 50 + 50 + 200;
            this.Width = MapSize * 50 + 50;
            GridAll.ShowGridLines = true;
            GridAll.HorizontalAlignment = HorizontalAlignment.Center;
            GridAll.VerticalAlignment = VerticalAlignment.Center;
            for (int i = 0; i < MapSize; i++)
            {
                ColumnDefinition columnDefinition = new ColumnDefinition();
                columnDefinition.Width = new GridLength(50);
                GridAll.ColumnDefinitions.Add(columnDefinition);
                RowDefinition rd = new RowDefinition();
                rd.Height = new GridLength(50);
                GridAll.RowDefinitions.Add(rd);
            }
            for (int x = 0; x < MapSize; x++)
            {
                for (int y = 0; y < MapSize; y++)
                {
                    TextBlock textBlock = new TextBlock();
                    textBlock.Text = "0";
                    textBlock.Name = $"x{x}y{y}";
                    textBlock.TextAlignment = TextAlignment.Center;
                    textBlock.VerticalAlignment = VerticalAlignment.Center;
                    textBlock.HorizontalAlignment = HorizontalAlignment.Center;
                    textBlock.Height = 50;
                    textBlock.Width = 50;
                    Grid.SetRow(textBlock, x);
                    Grid.SetColumn(textBlock, y);
                    GridAll.Children.Add(textBlock);
                }
            }
            UpdateMap();
        }
        private void UpdateMap()
        {
            foreach (var mapItem in Map)
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    TextBlock userTextBlock =
                        UiHelper.FindChild<TextBlock>(GridAll, $"x{mapItem.X}y{mapItem.Y}");
                    if (userTextBlock != null)
                    {
                        userTextBlock.Text = mapItem.Icon;
                    }
                }));
            }
        }
        private void Log(string text)
        {
            Dispatcher.Invoke((Action)(() => { TextBlockLog.Text += $"\n {DateTime.Now.ToString("T")} : {text}"; }));
        }

        private void ButtonTrain_OnClick(object sender, RoutedEventArgs e)
        {
            if (TrainMode==false)
            {
                TrainMode = true;
                TrainData = new List<double[]>();
                LastMapBackup = new List<MapItem>();
                LastMapBackup.AddRange(Map);
                ButtonMove.Visibility = Visibility.Collapsed;
                ButtonForward.Visibility = Visibility.Visible;
                ButtonBack.Visibility = Visibility.Visible;
                ButtonLeft.Visibility = Visibility.Visible;
                ButtonRight.Visibility = Visibility.Visible;
                ButtonTrain.Content = "CloseTrain";
            }
            else
            {
               EndTrain();
            }
        }

        private bool CheckGameEnd()
        {
            if (Player.Y == Finish.Y && Player.X == Finish.X)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void EndTrain()
        {
            TrainHelper.SaveTrainData(TrainData);
            TrainMode = false;
            ButtonMove.Visibility = Visibility.Visible;
            ButtonForward.Visibility = Visibility.Collapsed;
            ButtonBack.Visibility = Visibility.Collapsed;
            ButtonLeft.Visibility = Visibility.Collapsed;
            ButtonRight.Visibility = Visibility.Collapsed;
            ButtonTrain.Content = "Train";
            Map = LastMapBackup;
            UpdateMap();
        }

        private void ButtonForward_OnClick(object sender, RoutedEventArgs e)
        {
            var sensorData = Moving.GetSensorData(Map, Player);
            if (Moving.CanMove(Player, MoveType.Forward, Map))
            {
                Map = Moving.Move(Player, MoveType.Forward, Map);
                UpdateMap();
                Player = Moving.GetPlayer(Map);
                var trainRow = new double[]
                {
                    sensorData[0],
                    sensorData[1],
                    sensorData[2],
                    sensorData[3],
                    Player.X,
                    Player.Y,
                    Finish.X,
                    Finish.Y,
                    0
                };
                TrainData.Add(trainRow);
                if (CheckGameEnd())
                {
                    EndTrain();
                }
            }
            else
            {
                Log("Препятствие мешает переместится");
            }
           
        }
        private void ButtonBack_OnClick(object sender, RoutedEventArgs e)
        {
            var sensorData = Moving.GetSensorData(Map, Player);
            if (Moving.CanMove(Player, MoveType.Back, Map))
            {
                Map = Moving.Move(Player, MoveType.Back, Map);
                UpdateMap();
                Player = Moving.GetPlayer(Map);
                var trainRow = new double[]
                {
                    sensorData[0],
                    sensorData[1],
                    sensorData[2],
                    sensorData[3],
                    Player.X,
                    Player.Y,
                    Finish.X,
                    Finish.Y,
                    1
                };
                TrainData.Add(trainRow);
                if (CheckGameEnd())
                {
                    EndTrain();
                }
            }
            else
            {
                Log("Препятствие мешает переместится");
            }
        }
        private void ButtonLeft_OnClick(object sender, RoutedEventArgs e)
        {
            var sensorData = Moving.GetSensorData(Map, Player);
            if (Moving.CanMove(Player, MoveType.Left, Map))
            {
                Map = Moving.Move(Player, MoveType.Left, Map);
                UpdateMap();
                Player = Moving.GetPlayer(Map);
                var trainRow = new double[]
                {
                    sensorData[0],
                    sensorData[1],
                    sensorData[2],
                    sensorData[3],
                    Player.X,
                    Player.Y,
                    Finish.X,
                    Finish.Y,
                    2
                };
                TrainData.Add(trainRow);
                if (CheckGameEnd())
                {
                    EndTrain();
                }
            }
            else
            {
                Log("Препятствие мешает переместится");
            }
        }
        private void ButtonRight_OnClick(object sender, RoutedEventArgs e)
        {
            var sensorData = Moving.GetSensorData(Map, Player);
            if (Moving.CanMove(Player, MoveType.Right, Map))
            {
                Map = Moving.Move(Player, MoveType.Right, Map);
                UpdateMap();
                Player = Moving.GetPlayer(Map);
                var trainRow = new double[]
                {
                    sensorData[0],
                    sensorData[1],
                    sensorData[2],
                    sensorData[3],
                    Player.X,
                    Player.Y,
                    Finish.X,
                    Finish.Y,
                    3
                };
                TrainData.Add(trainRow);
                if (CheckGameEnd())
                {
                    EndTrain();
                }
            }
            else
            {
                Log("Препятствие мешает переместится");
            }
        }
    }
}
