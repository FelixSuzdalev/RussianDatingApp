using RussianDatingApp.Model;
using RussianDatingApp.View;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace RussianDatingApp.ViewModel
{
    public class MainViewModel : ABaseViewModel
    {
        private RussianDatingAppEntities _dbContext = new RussianDatingAppEntities();

        public ObservableCollection<UserAccount> UserAccounts { get; set; }
        public ObservableCollection<UserProfile> UserProfiles { get; set; }
        public ObservableCollection<AgeCategory> AgeCategories { get; set; }
        public ObservableCollection<City> Cities { get; set; }
        public ObservableCollection<PartnerPreference> PartnerPreferences { get; set; }
        public ObservableCollection<Registration> Registrations { get; set; }
        public ObservableCollection<ZodiacSign> ZodiacSigns { get; set; }

        private string _userSearchText;
        public string UserSearchText
        {
            get => _userSearchText;
            set => SetPropertyChanged(ref _userSearchText, value, onChanged: UpdateFilteredUsers);
        }

        public ObservableCollection<UserProfile> FilteredUsers { get; set; }

        public ICommand AddUserProfileCommand { get; }
        public ICommand EditUserProfileCommand { get; }
        public ICommand DeleteUserProfileCommand { get; }

        public ICommand AddUserAccountCommand { get; }
        public ICommand EditUserAccountCommand { get; }
        public ICommand DeleteUserAccountCommand { get; }
        public ICommand EditCityCommand { get; }
        public ICommand AddCityCommand { get; }
        public ICommand DeleteCityCommand { get; }

        public ICommand AddAgeCategoryCommand { get; }
        public ICommand DeleteAgeCategoryCommand { get; }

        public ICommand RefreshCommand { get; }
        public ICommand SaveChangesCommand { get; }
        public ICommand EditAgeCategoryCommand { get; }
        public ICommand EditPartnerPreferenceCommand { get; }

        public MainViewModel()
        {
            AddUserProfileCommand = new RelayCommand(AddUserProfile);
            EditUserProfileCommand = new RelayCommand<UserProfile>(EditUserProfile, p => p != null);
            DeleteUserProfileCommand = new RelayCommand<UserProfile>(DeleteUserProfile, p => p != null);

            AddUserAccountCommand = new RelayCommand(AddUserAccount);
            EditUserAccountCommand = new RelayCommand<UserAccount>(EditUserAccount, a => a != null);
            DeleteUserAccountCommand = new RelayCommand<UserAccount>(DeleteUserAccount, a => a != null);

            AddCityCommand = new RelayCommand(AddCity);
            DeleteCityCommand = new RelayCommand<City>(DeleteCity, c => c != null);
            EditCityCommand = new RelayCommand<City>(EditCity, city => city != null);

            EditAgeCategoryCommand = new RelayCommand<AgeCategory>(EditAgeCategory, a => a != null);
            EditPartnerPreferenceCommand = new RelayCommand<PartnerPreference>(EditPartnerPreference, p => p != null);
            
            AddAgeCategoryCommand = new RelayCommand(AddAgeCategory);
            DeleteAgeCategoryCommand = new RelayCommand<AgeCategory>(DeleteAgeCategory, a => a != null);

            RefreshCommand = new RelayCommand(RefreshData);
            SaveChangesCommand = new RelayCommand(SaveChanges);

            LoadAllData();
            FilteredUsers = new ObservableCollection<UserProfile>(UserProfiles);
        }

        private void LoadAllData()
        {
            _dbContext.UserProfile.Include(p => p.City).Include(p => p.ZodiacSign).Include(p => p.AgeCategory).Include(p => p.Registration).Load();
            _dbContext.UserAccount.Include(a => a.UserProfile).Include(a => a.Registration).Load();
            _dbContext.City.Load();
            _dbContext.ZodiacSign.Load();
            _dbContext.AgeCategory.Load();
            _dbContext.Registration.Load();
            _dbContext.PartnerPreference.Include(p => p.UserProfile).Include(p => p.ZodiacSign).Load();

            UserProfiles = _dbContext.UserProfile.Local;
            OnPropertyChanged(nameof(UserProfiles));

            UserAccounts = _dbContext.UserAccount.Local;
            OnPropertyChanged(nameof(UserAccounts));

            Cities = _dbContext.City.Local;
            OnPropertyChanged(nameof(Cities));

            ZodiacSigns = _dbContext.ZodiacSign.Local;
            OnPropertyChanged(nameof(ZodiacSigns));

            AgeCategories = _dbContext.AgeCategory.Local;
            OnPropertyChanged(nameof(AgeCategories));

            Registrations = _dbContext.Registration.Local;
            OnPropertyChanged(nameof(Registrations));

            PartnerPreferences = _dbContext.PartnerPreference.Local;
            OnPropertyChanged(nameof(PartnerPreferences));
        }


        private void SaveChanges()
        {
            try
            {
                _dbContext.SaveChanges();
                LoadAllData();
                UpdateFilteredUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshData()
        {
            try
            {
                _dbContext.Dispose(); // Пересоздаем контекст
                _dbContext = new RussianDatingAppEntities();
                LoadAllData();
                UpdateFilteredUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateFilteredUsers()
        {
            if (string.IsNullOrWhiteSpace(UserSearchText))
            {
                FilteredUsers = new ObservableCollection<UserProfile>(UserProfiles);
            }
            else
            {
                string search = UserSearchText.ToLower();
                FilteredUsers = new ObservableCollection<UserProfile>(UserProfiles
                    .Where(p => (p.FirstName + p.LastName + p.MiddleName).ToLower().Contains(search)));
            }
            OnPropertyChanged(nameof(FilteredUsers));
        }

        private void AddUserProfile()
        {
            var profile = new UserProfile
            {
                Gender = "Мужской",
                InterestedInGender = "Женский",
                BirthDate = DateTime.Today.AddYears(-18),
                RegistrationID = Registrations.FirstOrDefault()?.RegistrationID ?? 1
            };

            _dbContext.UserProfile.Add(profile); // Добавляем до показа окна

            var window = new EditUserProfileWindow
            {
                DataContext = new EditUserProfileViewModel(profile, _dbContext),
                Owner = Application.Current.MainWindow
            };

            if (window.ShowDialog() == true)
            {
                try
                {
                    _dbContext.SaveChanges();
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException ex)
                {
                    string details = string.Join("\n", ex.EntityValidationErrors
                        .SelectMany(e => e.ValidationErrors)
                        .Select(e => $"{e.PropertyName}: {e.ErrorMessage}"));

                    MessageBox.Show($"Ошибка валидации:\n{details}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                var account = new UserAccount
                {
                    Email = $"user{profile.ProfileID}@example.com",
                    PasswordHash = "123456",
                    ProfileID = profile.ProfileID,
                    RegistrationID = profile.RegistrationID
                };
                _dbContext.UserAccount.Add(account);
                _dbContext.SaveChanges();
            }
            else
            {
                _dbContext.UserProfile.Remove(profile);
            }
        }

        private void EditUserProfile(UserProfile profile)
        {
            if (profile == null) return;

            var window = new EditUserProfileWindow
            {
                DataContext = new EditUserProfileViewModel(profile, _dbContext),
                Owner = Application.Current.MainWindow
            };

            if (window.ShowDialog() == true)
            {
                _dbContext.Entry(profile).State = EntityState.Modified;
                try
                {
                    _dbContext.SaveChanges();
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException ex)
                {
                    string message = string.Join("\n", ex.EntityValidationErrors
                        .SelectMany(e => e.ValidationErrors)
                        .Select(err => $"{err.PropertyName}: {err.ErrorMessage}"));

                    MessageBox.Show("Ошибка валидации:\n" + message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteUserProfile(UserProfile profile)
        {
            if (MessageBox.Show($"Удалить профиль {profile.LastName} {profile.FirstName}?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _dbContext.UserProfile.Remove(profile);
                _dbContext.SaveChanges();
            }
        }

        private void AddUserAccount()
        {
            var account = new UserAccount();
            var window = new EditAccountWindow(account, _dbContext)
            {
                Owner = Application.Current.MainWindow
            };

            if (window.ShowDialog() == true)
            {
                _dbContext.UserAccount.Add(account);
                _dbContext.SaveChanges();
            }
        }

        private void EditUserAccount(UserAccount account)
        {
            var window = new EditAccountWindow(account, _dbContext)
            {
                Owner = Application.Current.MainWindow
            };

            if (window.ShowDialog() == true)
            {
                _dbContext.SaveChanges();
            }
        }

        private void DeleteUserAccount(UserAccount account)
        {
            if (MessageBox.Show($"Удалить аккаунт {account.Email}?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _dbContext.UserAccount.Remove(account);
                _dbContext.SaveChanges();
            }
        }

        private void AddCity()
        {
            var city = new City
            {
                Name = "Новый город",
                Region = "Регион"
            };

            _dbContext.City.Add(city);

            var window = new EditCityWindow
            {
                DataContext = new EditCityViewModel(city),
                Owner = Application.Current.MainWindow
            };

            if (window.ShowDialog() == true)
            {
                try
                {
                    _dbContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                _dbContext.City.Remove(city); // откат
            }
        }
        private void EditCity(City city)
        {
            if (city == null) return;

            var window = new EditCityWindow
            {
                DataContext = new EditCityViewModel(city),
                Owner = Application.Current.MainWindow
            };

            if (window.ShowDialog() == true)
            {
                try
                {
                    _dbContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении изменений: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteCity(City city)
        {
            try
            {
                var cityToDelete = _dbContext.City.Find(city.CityID);
                if (cityToDelete != null)
                {
                    _dbContext.City.Remove(cityToDelete);
                    _dbContext.SaveChanges();
                }
                else
                {
                    MessageBox.Show("Город не найден в базе данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении города: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddAgeCategory()
        {
            var newCategory = new AgeCategory
            {
                AgeFrom = 18,
                AgeTo = 25,
                Description = "Новая категория"
            };

            _dbContext.AgeCategory.Add(newCategory);

            var window = new EditAgeCategoryWindow
            {
                DataContext = new EditAgeCategoryViewModel(newCategory, _dbContext),
                Owner = Application.Current.MainWindow
            };

            if (window.ShowDialog() == true)
            {
                try
                {
                    _dbContext.SaveChanges();
                    OnPropertyChanged(nameof(AgeCategories));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                _dbContext.AgeCategory.Remove(newCategory); // откат
            }
        }
        private void EditAgeCategory(AgeCategory category)
        {
            if (category == null) return;

            var window = new EditAgeCategoryWindow
            {
                DataContext = new EditAgeCategoryViewModel(category, _dbContext),
                Owner = Application.Current.MainWindow
            };

            if (window.ShowDialog() == true)
            {
                _dbContext.SaveChanges();
                RefreshData(); // Чтобы обновилось в UI
            }
        }
        private void DeleteAgeCategory(AgeCategory cat)
        {
            if (cat == null) return;

            var result = MessageBox.Show(
                $"Удалить возрастную категорию: {cat.AgeFrom}-{cat.AgeTo} лет?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _dbContext.AgeCategory.Remove(cat);
                    _dbContext.SaveChanges();
                    AgeCategories.Remove(cat);
                    OnPropertyChanged(nameof(AgeCategories));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void EditPartnerPreference(PartnerPreference pref)
        {
            if (pref == null) return;

            var window = new EditPartnerPreferenceWindow
            {
                DataContext = new EditPartnerPreferenceViewModel(pref, _dbContext),
                Owner = Application.Current.MainWindow
            };

            if (window.ShowDialog() == true)
            {
                _dbContext.SaveChanges();
                RefreshData();
            }
        }
    }
}
