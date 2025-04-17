using RussianDatingApp.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace RussianDatingApp.ViewModel
{
    public class EditPartnerPreferenceViewModel : INotifyPropertyChanged
    {
        private readonly PartnerPreference _original;
        private readonly RussianDatingAppEntities _dbContext;

        public PartnerPreference CurrentPreference { get; set; }
        public ObservableCollection<UserProfile> UserProfiles { get; }
        public ObservableCollection<ZodiacSign> ZodiacSigns { get; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public EditPartnerPreferenceViewModel(PartnerPreference preference, RussianDatingAppEntities context)
        {
            _original = preference;
            _dbContext = context;

            _dbContext.UserProfile.Load();
            _dbContext.ZodiacSign.Load();

            UserProfiles = _dbContext.UserProfile.Local;
            ZodiacSigns = _dbContext.ZodiacSign.Local;

            CurrentPreference = new PartnerPreference
            {
                PreferenceID = preference.PreferenceID,
                UserProfile = preference.UserProfile,
                MinAge = preference.MinAge,
                MaxAge = preference.MaxAge,
                Gender = preference.Gender,
                ZodiacSignID = preference.ZodiacSignID
            };

            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
        }

        private void Save()
        {
            _original.UserProfile = CurrentPreference.UserProfile;
            _original.MinAge = CurrentPreference.MinAge;
            _original.MaxAge = CurrentPreference.MaxAge;
            _original.Gender = CurrentPreference.Gender;
            _original.ZodiacSignID = CurrentPreference.ZodiacSignID;

            _dbContext.SaveChanges();
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
