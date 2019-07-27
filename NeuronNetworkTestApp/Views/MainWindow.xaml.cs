using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
        bool Life { get; set; }
        List<double[]> TrainData { get; set; }
        NeuralNetwork NeuralNetwork { get; set; }
        List<MapItem> Map { get; set; }
        List<MapItem> LastMapBackup { get; set; }
        private int MapSize = 10;
        MapItem Player { get; set; }
        MapItem Finish { get; set; }
        int Errors { get; set; }
        int MoveCount { get; set; }
        TaskFactory TaskFactory { get; set; }
        Logging Logger { get; set; }
        private bool GameOver = false;
        Random Random { get; set; }
        int WallCount { get; set; }
        double DistanceToFinish { get; set; }
        private bool AutoTrain = true;

        public MainWindow()
        {
            InitializeComponent();
            InitializeData();
        }
        private void InitializeData()
        {
            Random = new Random();
            var topology = new Topology(9, 1, 0.2, 6,6,4,4,2);
            NeuralNetwork = new NeuralNetwork(topology);
            Errors = 0;
            Life = false;
            WallCount = 8;
            Logger = new Logging();
            TaskFactory = new TaskFactory();
            TrainData = new List<double[]>();
            TrainMode = false;
            CreateMap();
            Player = Moving.GetPlayer(Map);
            Finish = Moving.GetFinish(Map);
            TrainingFromDataset();
        }
        private void Dolife()
        {
            while (Life)
            {
                Move();
                MoveCount += 1;
                Thread.Sleep(1000);
            }
        }
        private void Move()
        {
            DistanceToFinish = Moving.GetDistance(Player.X,Player.Y, Finish);
            var sensorData = Moving.GetSensorData(Map, Player);
            double[] inputsignal = new double[]
            {
                sensorData[0],
                sensorData[1],
                sensorData[2],
                sensorData[3],
                Player.X/MapSize,
                Player.Y/MapSize,
                Finish.X/MapSize,
                Finish.Y/MapSize,
                1/Moving.GetDistance(Player.X,Player.Y,Finish)
            };
            var res = NeuralNetwork.Predict(inputsignal).Output;
            Log($"Результат вычислений {res}");
            var action = Moving.ConvertBotResultToMove(res);
            Log($"Бот решает {action}");
            if (Moving.CanMove(Player,action,Map))
            {
                if (AutoTrain)
                {
                    var newCoord = Moving.ConvertMovingTypeToCoord(action, Player);
                    AddTryToDataSet(newCoord, res);
                }
                Map = Moving.Move(Player, action, Map);
                UpdateMap();
                Player = Moving.GetPlayer(Map);
                Log($"Удачно выполнено {action}");
                EndMove();
            }
            else
            {
                Log($"Невозможно выполнить {action}");
                Errors += 1;
                if (AutoTrain)
                {
                    AddErorToDataSet();
                }
            }
        }
        private void EndMove()
        {
            if (CheckGameEnd())
            {
                GameOver = true;
                Life = false;
                Log($"Бот успешно добрался до финиша, ходов потребовалось - {MoveCount}, Ошибок совершено - {Errors}");
                Dispatcher.Invoke((Action)(() => { ButtonMove.Content = "Start"; }));
            }
        }
        private void Mutate()
        {
            Random random = new Random();
            var learnrate = random.NextDouble();
            var hidenLayCount = random.Next(1, 16);
            List<int> hiddeList = new List<int>();
            Log($"Сеть мутирует и приобретает следующие характеристики: \n LearnRate={learnrate} \n HiddenLayerCount={hidenLayCount}");
            for (int i = 0; i < hidenLayCount; i++)
            {
                var hiddenneuron = random.Next(1, 8);
                Log($"HiddenLayer{i} neuronCount = {hiddenneuron}");
                hiddeList.Add(hiddenneuron);
            }
            var topology = new Topology(8, 4, learnrate, hiddeList.ToArray());
            NeuralNetwork = new NeuralNetwork(topology);
            Errors = 0;
            TrainingFromDataset();
        }
        private void ButtonStart_OnClick(object sender, RoutedEventArgs e)
        {
            if (Life)
            {
                Life = false;
                ButtonMove.Content = "Start";
                ButtonTrain.Visibility = Visibility.Visible;
            }
            else
            {
                if (GameOver)
                {
                    GameOver = false;
                    CreateNewMap();
                    Player = Moving.GetPlayer(Map);
                    Finish = Moving.GetFinish(Map);
                    Life = true;
                    MoveCount = 0;
                    ButtonMove.Content = "Stop";
                    ButtonTrain.Visibility = Visibility.Collapsed;
                    TaskFactory.StartNew(Dolife);
                }
                else
                {
                    GameOver = false;
                    Life = true;
                    MoveCount = 0;
                    ButtonMove.Content = "Stop";
                    ButtonTrain.Visibility = Visibility.Collapsed;
                    TaskFactory.StartNew(Dolife);
                }
            }
        }
        private void TextBlockLog_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBlockLog.ScrollToEnd();
        }
        private void CreateMap()
        {
            Map = new List<MapItem>();
            Map = MapCreator.CreateDefaultMap(MapSize);
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
            var playerX = MapSize / 2;
            var playerY = MapSize / 2;
            Map = MapCreator.CreateRandomWall(Map, WallCount, MapSize);
            Map = MapCreator.ChangeItemType(Map, playerX, playerY, MapItemType.Player);//Add player
            Map = MapCreator.CreateRandomFinish(Map, MapSize);
            UpdateMap();
        }
        private void CreateNewMap()
        {
            WallCount ++;
            Map = new List<MapItem>();
            Map = MapCreator.CreateDefaultMap(MapSize);
            var playerX = MapSize / 2;
            var playerY = MapSize / 2;
            Map = MapCreator.CreateRandomWall(Map, WallCount, MapSize);
            Map = MapCreator.ChangeItemType(Map, playerX, playerY, MapItemType.Player);//Add player
            Map = MapCreator.CreateRandomFinish(Map, MapSize);
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
            var logtext = $"\n {DateTime.Now.ToString("T")} : {text}";
            Logger.LogString(logtext);
            Dispatcher.Invoke((Action)(() => { TextBlockLog.Text += $"{logtext}"; }));
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
            Player = Moving.GetPlayer(Map);
            Finish = Moving.GetFinish(Map);
            TrainingFromDataset();
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
                    0.01
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
                    0.5
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
                    0.4
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
                    0.25
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
        private void TrainingFromDataset()
        {
            var dataset = TrainHelper.GetTrainData().ToArray();
            List<double> truevaluelist = new List<double>();
            if (dataset.Length>0)
            {
                for (int i = 0; i < dataset.Length; i++)
                {
                    truevaluelist.Add(dataset[i].Last());
                    Array.Resize(ref dataset[i],8);
                }
                double[] trueDoublesarr = truevaluelist.ToArray();
                if (trueDoublesarr.Length!= dataset.Length)
                {
                    Log("Не удалось загрузить датасет, датасет имеет неверный формат");
                    return;
                }
                Train(TrainHelper.CreateMatrixToArray(dataset),trueDoublesarr);
            }
            else
            {
                Log("Не удалось загрузить датасет так как он пуст");
            }
        }
        private void Train(double[,] dataset, double[] truevalues)
        {
            // dataset = NeuralNetwork.Normalization(dataset);
            var difference = NeuralNetwork.Learn(truevalues, dataset, 10000);
            Log($"Бот успешно обучен. difference={difference}");
        }
        private void TrainToOneRow(double[] trainrow, double truevalue)
        {
            double[] trueVal = new double[]
            {
                truevalue
            };
            double[][] inputs = new double[][]
           {
               trainrow
           };
            Train(TrainHelper.CreateMatrixToArray(inputs),trueVal);
        }
        private void AddErorToDataSet()
        {
            var sensorData = Moving.GetSensorData(Map, Player);
            bool trueresult = false;
            int counter = 0;
            double newres = 0;
            while (!trueresult)
            {
                newres = GetFirstNotErrorMove(sensorData);
                var newcoord = Moving.ConvertMovingTypeToCoord(Moving.ConvertBotResultToMove(newres), Player);
                if (Moving.GetDistance(newcoord.Item1,newcoord.Item2,Finish)<DistanceToFinish)
                {
                    trueresult = true;
                }
                else if (counter>20)
                {
                    trueresult = true;
                }
                counter++;
            }
            Log($"Обучение говорит боту делать {Moving.ConvertBotResultToMove(newres)}");
            var trainRow = new double[]
            {
                sensorData[0],
                sensorData[1],
                sensorData[2],
                sensorData[3],
                Player.X/10,
                Player.Y/10,
                Finish.X/10,
                Finish.Y/10,
                1/Moving.GetDistance(Player.X,Player.Y,Finish)
            };
            TrainData.Add(trainRow);
            TrainHelper.SaveTrainData(TrainData);
            TrainToOneRow(trainRow, newres);
        }
        private void AddTryToDataSet(Tuple<double,double> newcoord, double result)
        {
            var newDistance = Moving.GetDistance(newcoord.Item1, newcoord.Item2, Finish);
            if (newDistance<DistanceToFinish)
            {
                var sensorData = Moving.GetSensorData(Map, Player);
                var trainRow = new double[]
                {
                    sensorData[0],
                    sensorData[1],
                    sensorData[2],
                    sensorData[3],
                    Player.X/10,
                    Player.Y/10,
                    Finish.X/10,
                    Finish.Y/10,
                    1/Moving.GetDistance(Player.X,Player.Y,Finish)
                };
                TrainData.Add(trainRow);
                TrainHelper.SaveTrainData(TrainData);
                TrainToOneRow(trainRow, result);
            }
            else
            {
                AddErorToDataSet();
            }

        }
        private double GetFirstNotErrorMove(double[] sensorData)
        {
            var x = Player.X - Finish.X;
            var y = Player.Y - Finish.Y;
            bool canMove = false;
            while (!canMove)
            {
               
                if (sensorData[0] == 1 && x>0)
                {
                    return 0.1;
                }
                else if (sensorData[1] == 1 && x<0)
                {
                    return 0.4;
                }
                else if (sensorData[2] == 1 && y>0)
                {
                    return 0.6;
                }
                else if (sensorData[3] == 1 && y<0)
                {
                    return 0.9;
                }
                else
                {
                    var rseed = Random.Next(0, 1000);
                    if (sensorData[3] == 1 && rseed<250)
                    {
                        return 0.9;
                    }
                    else if (sensorData[2] == 1 && rseed < 500)
                    {
                        return 0.6;
                    }
                    else if (sensorData[1] == 1 && rseed < 750)
                    {
                        return 0.4;
                    }
                    else if (sensorData[0] == 1 && rseed < 1000)
                    {
                        return 0.1;
                    }
                }
            }
            return 0;
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
           
        }
    }
}
