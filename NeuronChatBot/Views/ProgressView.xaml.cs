using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NeuronChatBot.Views
{
    /// <summary>
    /// Логика взаимодействия для ProgressView.xaml
    /// </summary>
    public partial class ProgressView : Window
    {
        private string Titlefirst;
        public ProgressView(string title, double maxCount)
        {
            InitializeComponent();
            this.Title = title;
            Titlefirst = title;
            ProgressBar.Maximum = maxCount;
        }

        public void SetProgress(double value)
        {
            this.Title = $"{Titlefirst} :  {value} из {ProgressBar.Maximum}";
            ProgressBar.Value = value;
            if (value == ProgressBar.Maximum)
            {
                this.Close();
            }
        }
    }
}
