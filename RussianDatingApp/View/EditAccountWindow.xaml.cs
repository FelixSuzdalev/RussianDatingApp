using RussianDatingApp.Model;
using RussianDatingApp.ViewModel;
using System.Windows;

namespace RussianDatingApp.View
{
    public partial class EditAccountWindow : Window
    {
        public EditAccountWindow(UserAccount account, RussianDatingAppEntities dbContext)
        {
            InitializeComponent();
            DataContext = new EditAccountViewModel(account, dbContext);
        }
    }
}
