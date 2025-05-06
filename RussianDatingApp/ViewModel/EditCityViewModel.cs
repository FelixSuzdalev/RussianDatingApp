using RussianDatingApp.Model;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace RussianDatingApp.ViewModel
{
    public class EditCityViewModel : INotifyPropertyChanged
    {
        private readonly City _original;
        public City CurrentCity { get; set; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public EditCityViewModel(City city)
        {
            _original = city;
            CurrentCity = new City
            {
                CityID = city.CityID,
                Name = city.Name,
                Region = city.Region
            };

            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
        }

        private void Save()
        {
            _original.Name = CurrentCity.Name;
            _original.Region = CurrentCity.Region;
            CloseWindow(true);
        }

        private void Cancel()
        {
            CloseWindow(false);
        }

        private void CloseWindow(bool result)
        {
            foreach (Window w in Application.Current.Windows)
            {
                if (w.DataContext == this)
                {
                    w.DialogResult = result;
                    break;
                }
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
