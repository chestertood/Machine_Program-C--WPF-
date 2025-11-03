using service;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
//using System.Windows.Shapes;
using static WpfApp1.MainWindow;




namespace WpfApp1
{    
    public partial class Page1 : UserControl
    {

        private void Config_Button_Click(object sender, RoutedEventArgs e)
        {
            string configpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config.ini");
            Process.Start(new ProcessStartInfo()
            {
                FileName = "notepad.exe",
                UseShellExecute = true,
                Arguments = configpath
            });
        }

        public async void Retry(object sender, RoutedEventArgs e)
        {

            var main = Application.Current.MainWindow as MainWindow;
            main?.CheckConnection(); 

        }
        public Page1()
        {
            InitializeComponent();
    }
    }
}
