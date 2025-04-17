using RussianDatingApp.Model;
using RussianDatingApp.View;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace RussianDatingApp
{
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var splash = new RussianDatingApp.View.SplashScreen();
            splash.Show();

            await InitializeApplicationAsync();        
            await splash.FadeOutAsync();             

            await Task.Delay(800);                    

            var main = new MainWindow();
            main.Show();
        }

        private async Task InitializeApplicationAsync()
        {
            await Task.Delay(5000); // Имитация загрузки

            using (var db = new RussianDatingAppEntities())
            {
                db.Database.CreateIfNotExists();
            }
        }
    }
}
