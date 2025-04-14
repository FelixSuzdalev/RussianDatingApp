using RussianDatingApp.Model;
using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows.Input;

namespace RussianDatingApp.ViewModel
{
    public class MainViewModel : ABaseViewModel
    {
        private readonly RussianDatingAppEntities _dbContext;

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

        public ICommand RefreshCommand => new RelayCommand(RefreshData);

        public MainViewModel()
        {
            _dbContext = new RussianDatingAppEntities();
            LoadAllData();
            AddTestDataIfEmpty();
            FilteredUsers = new ObservableCollection<UserProfile>(UserProfiles);
        }

        private void LoadAllData()
        {
            _dbContext.UserAccount
                .Include(a => a.Registration)
                .Include(a => a.UserProfile)
                .Load();

            _dbContext.UserProfile
                .Include(p => p.City)
                .Include(p => p.ZodiacSign)
                .Include(p => p.AgeCategory)
                .Include(p => p.Registration)
                .Load();

            _dbContext.AgeCategory.Load();
            _dbContext.City.Load();
            _dbContext.PartnerPreference
                .Include(p => p.UserProfile)
                .Include(p => p.ZodiacSign)
                .Load();
            _dbContext.Registration.Load();
            _dbContext.ZodiacSign.Load();

            UserAccounts = _dbContext.UserAccount.Local;
            UserProfiles = _dbContext.UserProfile.Local;
            AgeCategories = _dbContext.AgeCategory.Local;
            Cities = _dbContext.City.Local;
            PartnerPreferences = _dbContext.PartnerPreference.Local;
            Registrations = _dbContext.Registration.Local;
            ZodiacSigns = _dbContext.ZodiacSign.Local;
        }

        private void UpdateFilteredUsers()
        {
            if (string.IsNullOrWhiteSpace(UserSearchText))
            {
                FilteredUsers = new ObservableCollection<UserProfile>(UserProfiles);
            }
            else
            {
                var searchText = UserSearchText.ToLower();
                FilteredUsers = new ObservableCollection<UserProfile>(UserProfiles
                    .Where(u =>
                        u.FirstName.ToLower().Contains(searchText) ||
                        u.LastName.ToLower().Contains(searchText) ||
                        (u.MiddleName != null && u.MiddleName.ToLower().Contains(searchText)) ||
                        u.City.Name.ToLower().Contains(searchText) ||
                        u.ZodiacSign.RussianName.ToLower().Contains(searchText)));
            }
            OnPropertyChanged(nameof(FilteredUsers));
        }

        private void AddTestDataIfEmpty()
        {
            if (!AgeCategories.Any())
            {
                _dbContext.AgeCategory.Add(new AgeCategory { AgeCategoryID = 1, AgeFrom = 18, AgeTo = 25, Description = "18-25 лет" });
                _dbContext.AgeCategory.Add(new AgeCategory { AgeCategoryID = 2, AgeFrom = 26, AgeTo = 35, Description = "26-35 лет" });
                _dbContext.SaveChanges();
            }

            if (!Cities.Any())
            {
                _dbContext.City.Add(new City { Name = "Москва", Region = "Центральный" });
                _dbContext.City.Add(new City { Name = "Санкт-Петербург", Region = "Северо-Западный" });
                _dbContext.SaveChanges();
            }

            if (!ZodiacSigns.Any())
            {
                _dbContext.ZodiacSign.Add(new ZodiacSign { ZodiacSignID = 1, Name = "Aries", RussianName = "Овен" });
                _dbContext.ZodiacSign.Add(new ZodiacSign { ZodiacSignID = 2, Name = "Taurus", RussianName = "Телец" });
                _dbContext.SaveChanges();
            }

            if (!Registrations.Any())
            {
                _dbContext.Registration.Add(new Registration { RegistrationDate = DateTime.Now });
                _dbContext.SaveChanges();
            }

            if (!UserProfiles.Any() && Registrations.Any())
            {
                var registration = _dbContext.Registration.First();
                var city = _dbContext.City.First();
                var zodiac = _dbContext.ZodiacSign.First();
                var ageCategory = _dbContext.AgeCategory.First();

                var profile = new UserProfile
                {
                    ProfileID = 1,
                    RegistrationID = registration.RegistrationID,
                    FirstName = "Иван",
                    LastName = "Иванов",
                    BirthDate = new DateTime(1990, 5, 15),
                    Gender = "Мужской",
                    InterestedInGender = "Женский",
                    CityID = city.CityID,
                    AboutText = "Программист, люблю спорт и путешествия",
                    ZodiacSignID = zodiac.ZodiacSignID,
                    AgeCategoryID = ageCategory.AgeCategoryID,
                    HasPhoto = false,
                    IsVerified = false
                };

                _dbContext.UserProfile.Add(profile);
                _dbContext.SaveChanges();

                if (!UserAccounts.Any())
                {
                    _dbContext.UserAccount.Add(new UserAccount
                    {
                        RegistrationID = registration.RegistrationID,
                        ProfileID = profile.ProfileID,
                        Email = "ivan@example.com",
                        PasswordHash = "hashed_password_123"
                    });
                    _dbContext.SaveChanges();
                }
            }
        }

        private void RefreshData()
        {
            _dbContext.SaveChanges();
            LoadAllData();
            UpdateFilteredUsers();
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;
        public void Execute(object parameter) => _execute();
    }
}