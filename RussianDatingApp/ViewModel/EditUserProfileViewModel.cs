
using RussianDatingApp.Model;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace RussianDatingApp.ViewModel
{
    public class EditUserProfileViewModel : INotifyPropertyChanged
    {
        private readonly UserProfile _original;
        private readonly RussianDatingAppEntities _dbContext;

        public UserProfile CurrentProfile { get; set; }

        public ObservableCollection<City> Cities { get; }
        public ObservableCollection<ZodiacSign> ZodiacSigns { get; }
        public ObservableCollection<AgeCategory> AgeCategories { get; }
        public ObservableCollection<Registration> Registrations { get; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public EditUserProfileViewModel(UserProfile profile, RussianDatingAppEntities context)
        {
            _original = profile;
            _dbContext = context;

            _dbContext.City.Load();
            _dbContext.ZodiacSign.Load();
            _dbContext.AgeCategory.Load();
            _dbContext.Registration.Load();

            Cities = _dbContext.City.Local;
            ZodiacSigns = _dbContext.ZodiacSign.Local;
            AgeCategories = _dbContext.AgeCategory.Local;
            Registrations = _dbContext.Registration.Local;

            CurrentProfile = new UserProfile
            {
                ProfileID = profile.ProfileID,
                FirstName = profile.FirstName,
                LastName = profile.LastName,
                MiddleName = profile.MiddleName,
                Gender = profile.Gender,
                BirthDate = profile.BirthDate,
                CityID = profile.CityID,
                ZodiacSignID = profile.ZodiacSignID,
                AgeCategoryID = profile.AgeCategoryID,
                HasPhoto = profile.HasPhoto,
                IsVerified = profile.IsVerified,
                InterestedInGender = profile.InterestedInGender,
                AboutText = profile.AboutText,
                RegistrationID = profile.RegistrationID > 0 ? profile.RegistrationID : Registrations.FirstOrDefault()?.RegistrationID ?? 1
            };

            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
        }

        private void Save()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(CurrentProfile.FirstName) || string.IsNullOrWhiteSpace(CurrentProfile.LastName))
                {
                    MessageBox.Show("Поля 'Имя' и 'Фамилия' обязательны.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _original.FirstName = CurrentProfile.FirstName?.Trim();
                _original.LastName = CurrentProfile.LastName?.Trim();
                _original.MiddleName = CurrentProfile.MiddleName?.Trim();
                _original.Gender = CurrentProfile.Gender;
                _original.BirthDate = CurrentProfile.BirthDate;
                _original.CityID = CurrentProfile.CityID;
                _original.ZodiacSignID = CurrentProfile.ZodiacSignID;
                _original.AgeCategoryID = CurrentProfile.AgeCategoryID;
                _original.HasPhoto = CurrentProfile.HasPhoto;
                _original.IsVerified = CurrentProfile.IsVerified;
                _original.InterestedInGender = CurrentProfile.InterestedInGender;
                _original.AboutText = CurrentProfile.AboutText;
                _original.RegistrationID = CurrentProfile.RegistrationID;

               
                _dbContext.SaveChanges();

                CloseWindow(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении профиля: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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