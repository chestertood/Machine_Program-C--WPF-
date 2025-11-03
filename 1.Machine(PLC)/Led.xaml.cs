using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfApp1
{
    public partial class Led : UserControl
    {
        public Led()
        {
            InitializeComponent();
            UpdateLed();
        }

        // Dependency Property: IsActive
        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register("IsActive", typeof(bool), typeof(Led),
                new PropertyMetadata(false, OnIsActiveChanged));

        public bool IsActive
        {
            get => (bool)GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Led)d).UpdateLed();
        }

        // สี LED
        public Brush ColorOn { get; set; } = Brushes.LimeGreen;
        public Brush ColorOff { get; set; } = Brushes.Red;

        private void UpdateLed()
        {
            borderLed.Background = IsActive ? ColorOn : ColorOff;
        }
    }
}
