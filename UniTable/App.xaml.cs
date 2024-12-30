using System.Windows;

namespace UniTable
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public string? StartFile { get; private set; }

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);
			if (e.Args.Length > 0)
			{
				var filename = e.Args[0];
				if (filename.EndsWith(".cuacv") || filename.EndsWith(".uttp"))
				{
					StartFile = filename;
				}
			}
		}

		private void Application_Exit(object sender, ExitEventArgs e)
		{
			//ViewModel?.Save();
		}
	}
}
