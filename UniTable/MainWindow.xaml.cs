using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
using UniTable.Properties;

namespace UniTable
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private UniModel? uniModel;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string fileName = ((App)App.Current).StartFile;
            uniModel = (UniModel?)DataContext;

            while (!File.Exists(fileName))
            {
                OpenFileDialog openFileDialog = new()
                {
                    Title = "Select text file containing copy pasted university course tables",
                };
                bool? result = openFileDialog.ShowDialog();

                // Process open file dialog box results
                if (result == true)
                {
                    fileName = openFileDialog.FileName;
                }

            }

            uniModel?.LoadUniTable(fileName);
        }

        private void ThemeButton_Click(object sender, RoutedEventArgs e)
        {
            ThemeButton.IsEnabled = false;

            Resources["BackgroundColourKey"] = new SolidColorBrush(Color.FromArgb(60, 250, 250, 250));
            Resources["DialogBackgroundColourKey"] = new SolidColorBrush(Colors.White);
            Resources["PanelColourKey"] = new SolidColorBrush(Color.FromArgb(60, 155, 155, 155));
            Resources["PanelFaintColourKey"] = new SolidColorBrush(Color.FromArgb(30, 155, 155, 155));
            Resources["ForegroundColourKey"] = new SolidColorBrush(Colors.Black);
            Resources["ForegroundMinorColourKey"] = new SolidColorBrush(Color.FromArgb(127, 0, 0, 0));
        }
    }
}
