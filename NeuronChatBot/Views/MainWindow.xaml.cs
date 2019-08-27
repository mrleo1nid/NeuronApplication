using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Forms;
using NeuralBotBase.Storage;

namespace NeuronChatBot.Views
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private StorageDBRepository dbRepository = new StorageDBRepository();
        private LearnDBRepository learnDbRepository = new LearnDBRepository();
        private string[] lineStrings;
        private string[] lineStringsLearn;
        private ProgressView progressView;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonAddDB_OnClick(object sender, RoutedEventArgs e)
        {
            var text = TextBoxInput.Text;
            if (!String.IsNullOrEmpty(text))
            {
                var textArr = text.Split(' ');
                foreach (var slovo in textArr)
                {
                    dbRepository.AddStorageRow(slovo);
                }
            }
        }

        private void ButtonLoadFromFile_OnClick(object sender, RoutedEventArgs e)
        {
           OpenFileDialog dialog = new OpenFileDialog();
           dialog.Filter = "Text files(*.txt)|*.txt|All files(*.*)|*.*";
           var result = dialog.ShowDialog();
           if (result == System.Windows.Forms.DialogResult.OK)
           {
               var file = new FileInfo(dialog.FileName);
               if (file.Exists)
               {
                   Encoding encoding = Encoding.UTF8;
                   Stream fs = new FileStream(dialog.FileName, FileMode.Open);
                   using (StreamReader sr = new StreamReader(fs, true))
                   {
                       encoding = sr.CurrentEncoding;
                   }
                   fs.Close();
                   var lines = File.ReadAllLines(dialog.FileName,encoding);
                   lineStrings = lines;
                   progressView = new ProgressView("Загрузка в БД из файла", lines.Length);
                   progressView.Owner = this;
                   progressView.Show();
                   TaskFactory taskFactory = new TaskFactory();
                   taskFactory.StartNew(LoadToDB);
               }
           }
        }

        private void LoadToDB()
        {
            if (lineStrings.Length>0)
            {
                double counter = 0;
                foreach (var line in lineStrings)
                {
                    var dopline = line.Split(' ')[0];
                    dbRepository.AddStorageRow(dopline.ToLower());
                    counter++;
                    if (System.Windows.Application.Current==null) return;
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(new MethodInvoker(delegate
                    {
                        progressView.SetProgress(counter);
                    }));
                }
            }
        }

        private void ButtonLoadFromFileLearn_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Text files(*.txt)|*.txt|All files(*.*)|*.*";
            var result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                var file = new FileInfo(dialog.FileName);
                if (file.Exists)
                {
                    Encoding encoding = Encoding.UTF8;
                    Stream fs = new FileStream(dialog.FileName, FileMode.Open);
                    using (StreamReader sr = new StreamReader(fs, true))
                    {
                        encoding = sr.CurrentEncoding;
                    }
                    fs.Close();
                    var lines = File.ReadAllLines(dialog.FileName, encoding);
                    lineStringsLearn = lines;
                    progressView = new ProgressView("Загрузка в БД для обучения из файла", lines.Length);
                    progressView.Owner = this;
                    progressView.Show();
                    TaskFactory taskFactory = new TaskFactory();
                    taskFactory.StartNew(LoadToDBLearn);
                }
            }
        }

        private void LoadToDBLearn()
        {
            if (lineStringsLearn.Length > 0)
            {
                double counter = 0;
                foreach (var line in lineStringsLearn)
                {
                    var spline = line.Split(':');
                    if (spline.Length>=2)
                    {
                        SplitPhraseAndAddToDb(spline[0].ToLower());
                        SplitPhraseAndAddToDb(spline[1].ToLower());
                        learnDbRepository.AddStorageRow(spline[0].ToLower(), spline[1].ToLower());
                    }
                    counter++;
                    if (System.Windows.Application.Current == null) return;
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(new MethodInvoker(delegate
                    {
                        progressView.SetProgress(counter);
                    }));
                }
            }
        }

        private void SplitPhraseAndAddToDb(string phrase)
        {
            var mass = phrase.Split(' ');
            if (mass.Length>0)
            {
                foreach (var slovo in mass)
                {
                    dbRepository.AddStorageRow(slovo);
                }
            }
        }
    }
}
