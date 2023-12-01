using Examath.Core.Environment;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using UniTable.Properties;

namespace UniTable
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private UniModel? _UniModel;

        public MainWindow()
        {
            InitializeComponent();
            Settings.Default.PropertyChanged += Settings_PropertyChanged;
        }

        private void Settings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Settings.Default.CommuteTime):
                case nameof(Settings.Default.CommuteUniToBusTime):
                case nameof(Settings.Default.FarePeak):
                case nameof(Settings.Default.FareOffPeak):
                    _UniModel?.ComputeStatistics();
                    break;
            }
        }

        private OpenFileDialog CreateOpenFileDialog()
        {
            return new OpenFileDialog()
            {
                Title = "Select the text file containing copy pasted university course tables",
                Filter = UniModel.FILTER,
            };
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string fileName = ((App)App.Current).StartFile ?? Settings.Default.LastFileName;
            _UniModel = (UniModel)DataContext;

            if (!File.Exists(fileName))
            {
                OpenFileDialog openFileDialog = CreateOpenFileDialog();
                bool? result = openFileDialog.ShowDialog();

                // Process open file dialog box results
                if (result == true)
                {
                    fileName = openFileDialog.FileName;
                }
                else
                {
                    Close();
                }
            }

            try
            {
                await _UniModel.LoadUniTable(fileName);
            }
            catch (Exception ee)
            {
                Messager.Out($"Inner exception: {ee.Message}", "Loading .cuacv", ConsoleStyle.ErrorBlockStyle);
                Close();
                return;
            }

            // State memory
            if (fileName == Settings.Default.LastFileName) _UniModel.Selected = Settings.Default.LastSelection;
            else Settings.Default.LastFileName = fileName;

            Root.Opacity = 1;
            Title = $"{System.IO.Path.GetFileName(fileName)} | Unitable v{System.Reflection.Assembly.GetExecutingAssembly()?.GetName()?.Version?.ToString()[0..^2]}";
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Settings.Default.LastSelection = _UniModel?.Selected;
            Settings.Default.Save();
            base.OnClosing(e);
        }

        private void ThemeButton_Click(object sender, RoutedEventArgs e)
        {
            ThemeButton.IsEnabled = false;
            OptionsExpander.IsExpanded = false;

            Resources["BackgroundColourKey"] = new SolidColorBrush(Color.FromArgb(60, 250, 250, 250));
            Resources["DialogBackgroundColourKey"] = new SolidColorBrush(Colors.White);
            Resources["PanelColourKey"] = new SolidColorBrush(Color.FromArgb(60, 155, 155, 155));
            Resources["PanelFaintColourKey"] = new SolidColorBrush(Color.FromArgb(30, 155, 155, 155));
            Resources["ForegroundColourKey"] = new SolidColorBrush(Colors.Black);
            Resources["ForegroundMinorColourKey"] = new SolidColorBrush(Color.FromArgb(127, 0, 0, 0));
        }

        private void CompactModeCheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void CompactModeCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void UniClassRootGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            if (((Grid)sender).DataContext is UniClass uniClass)
            {
                uniClass.IsMouseOver = true;
            }
        }

        private void UniClassRootGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            if (((Grid)sender).DataContext is UniClass uniClass)
            {
                uniClass.IsMouseOver = false;
            }
        }

        private async void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = CreateOpenFileDialog();
            bool? result = openFileDialog.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                UniModel uniModel = new();

                try
                {
                    await uniModel.LoadUniTable(openFileDialog.FileName);
                }
                catch (Exception ee)
                {
                    Messager.Out($"Inner exception: {ee.Message}", "Loading .cuacv", ConsoleStyle.ErrorBlockStyle);
                    return;
                }

                _UniModel = uniModel;
            }
        }
    }
}
