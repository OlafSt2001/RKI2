using System.Windows;
using RKI2.ViewModels;

namespace RKI2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly RKIMainViewModel MVVM;
        public MainWindow()
        {
            InitializeComponent();
            MVVM = new RKIMainViewModel();
        }
    }
}