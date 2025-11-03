using Model;
using service;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Diagnostics;



namespace WpfApp1
{
    public partial class Page3 : UserControl
    {
        public string _product;
        public string _EN;
        
        public Page3()
        {            
            InitializeComponent();
            this.Loaded += Page3_Loaded;
            _product = ((App)Application.Current).SelectedProduct;
            _EN = ((App)Application.Current).EN;
            EN_show.Text = _EN;
            product_show.Text = _product;
            ShowPCBsBasedOnProduct(_product);
        }

        
        private void Page3_Loaded(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this);
            if (mainWindow != null)
            {
                mainWindow.WindowStartupLocation = WindowStartupLocation.Manual;
                mainWindow.Left = 0;
                mainWindow.Top = 50;
            }
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            string selectedProduct = product_show.Text;
            string EN = EN_show.Text;
            // อัปเดตค่าใน App เพื่อให้ส่งต่อ
            ((App)Application.Current).SelectedProduct = selectedProduct;
            ((App)Application.Current).EN = EN;
            MessageBox.Show($"You have changed model to '{selectedProduct}'","Model config", MessageBoxButton.OK, MessageBoxImage.Information);
            // รีเฟรชหน้าใหม่โดยโหลด Page3 ใหม่ (แสดงผลตามค่าใหม่)
            var parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                // สมมุติ MainContent เป็น ContentControl ใน MainWindow
                if (parentWindow is MainWindow mainWin)
                {
                    if (mainWin.MainContent.Content is Page3 oldpage3)
                    {
                        oldpage3.MODEL1?.stop();
                        oldpage3.MODEL2?.stop();

                    }
                    mainWin.MainContent.Content = new Page3();
                }
            }
        }
        private void Registered_log_Button_Click(object sender, RoutedEventArgs e)
        {
            string configpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config.ini");
            var config = new FileConfig(configpath);
            var path_log = config.Get("Path", "Log");
            Process.Start(new ProcessStartInfo()
            {
                FileName = path_log,
                UseShellExecute = true,
                Verb = "open"
            });
        }
        private void MES_log_Button_Click(object sender, RoutedEventArgs e)
        {
            string configpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config.ini");
            var config = new FileConfig(configpath);
            var path_log = config.Get("Path", "MES_Log");

            Process.Start(new ProcessStartInfo()
            {
                FileName = path_log,
                UseShellExecute = true,
                Verb = "open"
            });
        }

        public MODEL1 MODEL1;
        public MODEL2 MODEL2;
        private void ShowPCBsBasedOnProduct(string selectedProduct)
        {
            if (selectedProduct == "MODEL1")
            {
                MODEL1_show.Visibility = Visibility.Visible;
                MODEL2_show.Visibility = Visibility.Collapsed;
                View_site.Height = 301;
                Scroll_view.Height = 218;

                MODEL1 = new MODEL1(this);
                MODEL1.start();
                MODEL1.run();

            }
            else if (selectedProduct == "MODEL2")
            {
                MODEL2_show.Visibility = Visibility.Visible;
                MODEL1_show.Visibility = Visibility.Collapsed;
                View_site.Height = 403;
                Scroll_view.Height = 318;

                MODEL2 = new MODEL2(this);
                MODEL2.start();
                MODEL2.run();
            }
            
            else
            {
                MessageBox.Show("Error");
            }
        }
    }
}
