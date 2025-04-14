using RussianDatingApp.ViewModel;
using System.Windows;

namespace RussianDatingApp.View
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}