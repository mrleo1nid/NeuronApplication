using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NeuralNetworks;
using NeuronNetworkTestApp.Models;

namespace NeuronNetworkTestApp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        NeuralNetwork NeuralNetwork { get; set; }
        List<MapItem> Map { get; set; }
        private int MapSize = 10;
        private bool Cancel = false;
        MapItem Player { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            InitializeData();
        }

        private void Life()
        {
            Player = Moving.GetPlayer(Map);
            while (!Cancel)
            {
                var sensorData = Moving.GetSensorData(Map, Player);
                PrepareToMoving(sensorData);
                Thread.Sleep(2000);
            }

        }
        private void PrepareToMoving(double[] sensorData)
        {
            var moveRes = NeuralNetwork.Predict(sensorData).Output;
            var action = ConvertBotResponceToMoveType(moveRes);
            if (Moving.CanMove(Player,action,Map))
            {
                Log("Бот может это сделать");
                Train(sensorData,moveRes);
                Map = Moving.Move(Player, action, Map);
                UpdateMap();
                Player = Moving.GetPlayer(Map);
                Log("Бот успешно переместился");
                return;

            }
            else
            {
                Log("Бот не может это сделать");
                Train(sensorData);
                return;
            }

        }

        private MoveType ConvertBotResponceToMoveType(double botResponce)
        {
            if (botResponce>0 && botResponce<=1)
            {
                Log("Бот решает идти вверх");
                return MoveType.Forward;
            }
            if (botResponce > 1 && botResponce <= 2)
            {
                Log("Бот решает идти вниз");
                return MoveType.Back;
            }
            if (botResponce > 2 && botResponce <= 3)
            {
                Log("Бот решает идти влево");
                return MoveType.Left;
            }
            if (botResponce > 3 && botResponce <= 4)
            {
                Log("Бот решает идти вправо");
                return MoveType.Right;
            }
            Log("Бот решает идти вверх");
            return MoveType.Forward;
        }

       

        private void InitializeData()
        {
            var topology = new Topology(4, 1, 0.1, 2);
            NeuralNetwork = new NeuralNetwork(topology);
            Map = MapCreator.CreateDefaultMap(MapSize);
            var playerX = MapSize / 2;
            var playerY = MapSize / 2;
            Map = MapCreator.ChangeItemType(Map, playerX, playerY, MapItemType.Player);//Add player
            Map = MapCreator.ChangeItemType(Map, 5, 6, MapItemType.Wall); //Add Wall
            Map = MapCreator.ChangeItemType(Map, 3, 3, MapItemType.Wall); //Add Wall
            Map = MapCreator.ChangeItemType(Map, 4, 5, MapItemType.Wall); //Add Wall
            CreateMap();
            Player = Moving.GetPlayer(Map);
        }

        private void Train(double[] sensorData, double trueResult)
        {
            Log("Пытаюсь обучить  бота");
            NeuralNetwork.Learn(new []{trueResult}, sensorData, 100);
            Log("Бот чему то научился(наверное)");
        }
        private void Train(double[] sensorData)
        {
            Log("Пытаюсь обучить  бота");
            NeuralNetwork.Learn(ConvertSensorDataToTrueResult(sensorData), sensorData, 100);
            Log("Бот чему то научился(наверное)");
        }

        private double[] ConvertSensorDataToTrueResult(double[] sensorData)
        {
            List<double> doubles = new List<double>();
            if (sensorData[0]==1)
            {
                doubles.Add(0.5);
            }
            if (sensorData[1] ==1)
            {
              doubles.Add(1.5);   
            }
            if (sensorData[2]==1)
            {
                doubles.Add(2.5);
            }
            if (sensorData[3] == 1)
            {
                doubles.Add(3.5);
            }
            double[] result = doubles.ToArray();
            return result;
        }
        private void CreateMap()
        {
            this.Height = MapSize * 50 + 50 + 200;
            this.Width = MapSize * 50 + 50;
            GridAll.ShowGridLines=true;
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
                Dispatcher.Invoke((Action) (() =>
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

        private void ButtonStart_OnClick(object sender, RoutedEventArgs e)
        {
            Cancel = false;
            Log("Бот начинает работу");
            TaskFactory taskFactory = new TaskFactory();
            taskFactory.StartNew(Life);
        }

        private void ButtonStop_OnClick(object sender, RoutedEventArgs e)
        {
            Cancel = true;
            Log("Бот завершает работу");
        }

        private void TextBlockLog_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBlockLog.ScrollToEnd();
        }
    }
}
