using RussianDatingApp.Model;
using RussianDatingApp.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RussianDatingApp.View
{
    public partial class EditUserProfileWindow : Window
    {
        public EditUserProfileWindow(UserProfile profile, RussianDatingAppEntities dbContext)
        {
            InitializeComponent();

            var viewModel = new EditUserProfileViewModel(profile, dbContext);

            // Подписка на события ViewModel
            viewModel.RequestClose += (s, result) => this.DialogResult = result;
            viewModel.ShowMessage += (s, args) =>
                MessageBox.Show(args.Message, args.Caption, MessageBoxButton.OK, args.Icon);

            DataContext = viewModel;
        }
    }
}
