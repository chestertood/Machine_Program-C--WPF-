using service;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml.Linq;

namespace WpfApp1
{
    
    public partial class MainWindow : Window
    {
        public enum AppPage
        {
            Page1,
            Page2,
            Page3
        }
        public void NavigateToState(AppPage page)
        {
            switch (page)
            {
                case AppPage.Page1:
                    MainContent.Content = new Page1();
                    break;
                case AppPage.Page2:
                    MainContent.Content = new Page2();
                    break;
                case AppPage.Page3:
                    MainContent.Content = new Page3();
                    break;
            }
            
        }
        public async void CheckConnection()
        {
            bool check_scanner_state = false;
            bool check_camera_state = false;
            bool check_MES_state = false;
            Page1 page1 = new Page1();

            page1.set_button.Visibility = Visibility.Hidden;
            page1.Retry_button.Visibility = Visibility.Hidden;


            string configpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config.ini");

            var config = new FileConfig(configpath);

            // PLC Config
            var PLC_comport = config.Get("PLC", "comport");
            var PLC_buadrate = int.Parse(config.Get("PLC", "buadrate"));
            var PLC_device = config.Get("PLC", "device");

            // MES Config
            var MES_ip = config.Get("MES", "ip");
            var MES_Current_Station = int.Parse(config.Get("MES", "Current_Station"));

            // Scanner Config
            var scanner_ip = config.Get("SR_X300W_1", "ip");
            var scanner_port = int.Parse(config.Get("SR_X300W_1", "Port"));
            var scanner_buffer = int.Parse(config.Get("SR_X300W_1", "buffer_size"));

            // Camera Config
            var camera_ip = config.Get("CV-X490F_1", "ip");
            var camera_port = int.Parse(config.Get("CV-X490F_1", "Port"));
            var camera_buffer = int.Parse(config.Get("CV-X490F_1", "buffer_size"));

            // Path Log
            var path_log = config.Get("Path", "Log");
            var MES_path_log = config.Get("Path", "MES_Log");

            // URL Config
            var URL_Regist = config.Get("URL", "Regist_Data");
            var URL_Delete = config.Get("URL", "Delete_data");
            var URL_Get = config.Get("URL", "Get_Data");
            var URL_getstation = config.Get("URL", "getstation");

            
            TCP_Communication scanner = new TCP_Communication(scanner_ip, scanner_port, scanner_buffer);
            TCP_Communication camera = new TCP_Communication(camera_ip, camera_port , camera_buffer );
            MES MES = new MES();

            var check_scanner = true;
            var check_camera = true;
            var check_MES = true;


            page1.nameBox2.HorizontalAlignment = HorizontalAlignment.Center;
            page1.nameBox2.VerticalAlignment = VerticalAlignment.Center;
            page1.nameBox2.FontSize = 20;
            page1.nameBox2.Text = "Waiting";
            MainContent.Content = page1;
            await Task.Delay(500);
            check_scanner = scanner.Check();
            

            if (check_scanner)
            {
                page1.nameBox2.Text = "✅ Connected";
                page1.nameBox2.Foreground = new SolidColorBrush(Colors.Green);
                page1.led2.IsActive = true;
                check_scanner_state = true;
            }
            else
            {
                page1.nameBox2.Text = "❌ Failed";
                page1.nameBox2.Foreground = new SolidColorBrush(Colors.Red);
                page1.led2.IsActive = false;
                check_scanner_state = false;
            }

            MainContent.Content = page1;
            page1.nameBox3.FontSize = 20;
            page1.nameBox3.HorizontalAlignment = HorizontalAlignment.Center;
            page1.nameBox3.VerticalAlignment = VerticalAlignment.Center;
            page1.nameBox3.Text = "Waiting";
            await Task.Delay(500);
            check_camera = camera.Check();            

            if (check_camera)
            {
                page1.nameBox3.Text = "✅ Connected";
                page1.nameBox3.Foreground = new SolidColorBrush(Colors.Green);
                page1.led3.IsActive = true;
                check_camera_state = true;
            }
            else
            {
                page1.nameBox3.Text = "❌ Failed";
                page1.nameBox3.Foreground = new SolidColorBrush(Colors.Red);
                page1.led3.IsActive = false;
                check_camera_state = false;
            }

            MainContent.Content = page1;

            page1.nameBox4.FontSize = 20;
            page1.nameBox4.HorizontalAlignment = HorizontalAlignment.Center;
            page1.nameBox4.VerticalAlignment = VerticalAlignment.Center;
            page1.nameBox4.Text = "Waiting";
            await Task.Delay(500);
            MES.Connect(MES_ip);
            check_MES = await MES.RequestDataAsync(URL_getstation+"B", HttpMethod.Get);
                       
            if (check_MES)
            {
                page1.nameBox4.Text = "✅ Connected";
                page1.nameBox4.Foreground = new SolidColorBrush(Colors.Green);
                page1.led4.IsActive = true;
                check_MES_state = true;

            }
            else
            {
                page1.nameBox4.Text = "❌ Failed";
                page1.nameBox4.Foreground = new SolidColorBrush(Colors.Red);
                page1.led4.IsActive = false;
                check_MES_state = false;
            }

            MainContent.Content = page1;

            await Task.Delay(500);
            
            if (check_scanner_state && check_camera_state && check_MES_state)
            {
                NavigateToState(AppPage.Page2);
            }
            else
            {
                page1.set_button.Visibility = Visibility.Visible;
                page1.Retry_button.Visibility = Visibility.Visible;
                MessageBox.Show("Please go to \"Setting\" \nchecking path config then \"Retry\"", "Error connection", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            NavigateToState(AppPage.Page1);
            CheckConnection();
        }


    }
}