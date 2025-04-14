using RussianDatingApp.Model;
using System.Windows;

namespace RussianDatingApp
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Инициализация базы данных
            using (var db = new RussianDatingAppEntities())
            {
                db.Database.CreateIfNotExists();
            }
        }
    }
}