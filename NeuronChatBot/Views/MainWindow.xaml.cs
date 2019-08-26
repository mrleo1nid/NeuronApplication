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
using System.Windows.Navigation;
using System.Windows.Shapes;
using NeuronChatBot.Storage;

namespace NeuronChatBot.Views
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonAddDB_OnClick(object sender, RoutedEventArgs e)
        {
            var text = TextBoxInput.Text;
            if (!String.IsNullOrEmpty(text))
            {
                text = text.ToLower();
                var textArr = text.Split(' ');
                StorageDBRepository dbRepository = new StorageDBRepository();
                foreach (var slovo in textArr)
                {
                    var row = dbRepository.GetStorageRowFromText(slovo);
                    if (row.ID==-1)
                    {
                        dbRepository.AddStorageRow(slovo);
                    }
                }
            }
        }
    }
}
