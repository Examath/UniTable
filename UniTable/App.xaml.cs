using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace UniTable
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        internal UniModel? ViewModel;

        public string StartFile { get; private set; } = string.Empty;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            if (e.Args.Length > 0)
            {
                var filename = e.Args[0];
                if (filename.EndsWith(".cuacv"))
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
