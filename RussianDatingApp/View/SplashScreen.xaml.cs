using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace RussianDatingApp.View
{
    public partial class SplashScreen : Window
    {
        public SplashScreen()
        {
            InitializeComponent();
            Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(1),
                FillBehavior = FillBehavior.HoldEnd
            };

            BeginAnimation(OpacityProperty, fadeIn);
        }

        public async Task FadeOutAsync()
        {
            var fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.7),
                FillBehavior = FillBehavior.Stop
            };

            BeginAnimation(OpacityProperty, fadeOut);
            await Task.Delay(700);
            Close();
        }
    }
}
