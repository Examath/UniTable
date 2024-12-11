using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using UniTable.Model;
using UniTable.Properties;

namespace UniTable
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private VM? _VM;

		#region Constructor and Loading

		public MainWindow()
		{
			// Crash Handler
			AppDomain currentDomain = AppDomain.CurrentDomain;
			currentDomain.UnhandledException += new UnhandledExceptionEventHandler(CrashHandler);

			Settings.Default.PropertyChanged += Settings_PropertyChanged;
			//Title = $"UniTable v{System.Reflection.Assembly.GetExecutingAssembly()?.GetName()?.Version?.ToString(2)}";

			InitializeComponent();

			if (!Path.Exists(Settings.Default.LastFileName)) Recent.Visibility = Visibility.Collapsed;
		}

		private async void Window_Loaded(object sender, RoutedEventArgs e)
		{
			_VM = (VM)DataContext;

			if (((App)App.Current).StartFile is string fileLocation)
			{
				await _VM.Open(fileLocation);
			}

			if (_VM.Data == null) _VM.CreateFile();

			Root.Opacity = 1;
			//Title = $"{System.IO.Path.GetFileName(fileName)} | Unitable v{System.Reflection.Assembly.GetExecutingAssembly()?.GetName()?.Version?.ToString(2)}";
		}

		private async void RecentButton_Click(object sender, RoutedEventArgs e)
		{
			if (_VM != null)
			{
				await _VM.Open(Settings.Default.LastFileName);
			}
		}

		#endregion

		#region Closing

		private bool _IsReadyToClose = false;

		protected override async void OnClosing(CancelEventArgs e)
		{
			// Avoid Refire
			if (_IsReadyToClose) return;
			base.OnClosing(e);

			// If dirty
			if (_VM != null && _VM.IsModified)
			{
				// Temp cancel Closing
				e.Cancel = true;

				if (await _VM.IsUserReadyToPartWithCurrentFile())
				{
					// Restart closing
					_IsReadyToClose = true;
					Application.Current.Shutdown();
				}
			}

			Settings.Default.LastFileName = _VM?.FileLocation;
			Settings.Default.Save();
		}

		#endregion

		#region Settings

		private void Settings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case nameof(Settings.Default.CommuteTime):
				case nameof(Settings.Default.CommuteUniToBusTime):
				case nameof(Settings.Default.FarePeak):
				case nameof(Settings.Default.FareOffPeak):
					_VM?.ComputeStatistics();
					break;
			}
		}

		#endregion

		#region Theme And UX

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

		#endregion

		#region Crash Handler

		private void CrashHandler(object sender, UnhandledExceptionEventArgs args)
		{
#pragma warning disable CS0162 // Unreachable code detected when DEBUG config
			try
			{
				if (_VM != null)
				{
					_VM.SaveFile();
#if DEBUG
					return;
#endif

					Exception e = (Exception)args.ExceptionObject;
					MessageBox.Show($"{e.GetType().Name}: {e.Message}\nThe timetable plan was saved. See crash-info.txt fore more info.", " An Unhandled Exception Occurred", MessageBoxButton.OK, MessageBoxImage.Error);
					System.IO.File.AppendAllLines(System.IO.Path.GetDirectoryName(_VM.FileLocation) + "\\crash-info.txt",
						[
							"______________________________________________________",
							$"An unhandled exception occurred at {DateTime.Now:g}",
							$"A backup of Scoresheet was saved at {_VM.FileLocation}.crashed",
							$"Error Message:\t{e.Message}",
							$"Stack Trace:\n{e.StackTrace}",
						]
					);
				}

			}
			catch (Exception)
			{
				MessageBox.Show($"An exception occurred in the crash-handler. The timetable is unlikely to have been saved.", "Dual Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);
			}
#pragma warning restore CS0162 // Unreachable code detected
		}
		#endregion
	}
}
