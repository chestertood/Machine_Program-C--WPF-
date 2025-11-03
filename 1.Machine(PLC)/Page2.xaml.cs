
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace WpfApp1
{
    public partial class Page2 : UserControl
    {

        // Event handler for the "OK" button click
        private void OK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Check if the EN_ID field is empty or not
                if (string.IsNullOrEmpty(EN_ID.Text))
                {
                    MessageBox.Show("Please Insert Your EN ID");
                    return;
                }
                // Check if the EN_ID length is less than or equal to 12 characters
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
            // Handle any exceptions that may occur
            catch (Exception ex)
            {
                MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message);
            }
                       
        }

        
        private void Page2_Loaded(object sender, RoutedEventArgs e) 
        {
            // Set the window startup location and focus on the EN_ID field when the page is loaded
            var mainWindow = Window.GetWindow(this);
            if (mainWindow != null)
            {
                mainWindow.WindowStartupLocation = WindowStartupLocation.Manual;
                mainWindow.Left = 0;
                mainWindow.Top = 200;
                EN_ID.Focus();
            }
        }
        public Page2()
        {
            InitializeComponent();
            // Subscribe to the Loaded event of the page to set the window properties
            this.Loaded += Page2_Loaded;

        }
    }
}
