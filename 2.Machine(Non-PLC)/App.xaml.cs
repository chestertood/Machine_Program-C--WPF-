using service;

using System.IO;
using System.Windows;


namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {


        public string SelectedProduct { get; set; }
        public string EN { get; set; }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            

        }
    }
}
