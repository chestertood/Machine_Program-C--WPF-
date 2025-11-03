
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for Page2.xaml
    /// </summary>
    public partial class Page2 : UserControl
    {

     
        private void OK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(EN_ID.Text))
                {
                    MessageBox.Show("Please Insert Your EN ID");
                    return;
                }
                if ((EN_ID.Text.Length <= 12))
                {
                    string selectedProduct = ((ComboBoxItem)ProductSelector.SelectedItem).Content.ToString();
                    string EN = EN_ID.Text.ToString();
                    if (Application.Current.MainWindow is MainWindow mainWindow)
                    {
                        ((App)Application.Current).EN = EN;
                        ((App)Application.Current).SelectedProduct = selectedProduct;
                        mainWindow.NavigateToState(MainWindow.AppPage.Page3);

                    }

                }
                else
                {
                    MessageBox.Show("Please Try Again");
                }
                

            }
            catch (Exception ex)
            {
                MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message);
            }
                       
        }

        private void Page2_Loaded(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this);
            if (mainWindow != null)
            {
                mainWindow.WindowStartupLocation = WindowStartupLocation.Manual;
                mainWindow.Left = 0;
                mainWindow.Top = 100;
                EN_ID.Focus();
            }
        }
        public Page2()
        {
            InitializeComponent();            
            this.Loaded += Page2_Loaded;

        }
    }
}
