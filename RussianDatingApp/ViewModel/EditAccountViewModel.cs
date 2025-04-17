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
    public class EditAccountViewModel : INotifyPropertyChanged
    {
        private readonly RussianDatingAppEntities _dbContext;
        private readonly UserAccount _originalAccount;

        public UserAccount CurrentAccount { get; set; }
        public ObservableCollection<UserProfile> UserProfiles { get; }

        private UserProfile _selectedUserProfile;
        public UserProfile SelectedUserProfile
        {
            get => _selectedUserProfile;
            set
            {
                if (_selectedUserProfile != value)
                {
                    _selectedUserProfile = value;
                    OnPropertyChanged();

                    if (_selectedUserProfile != null)
                    {
                        var matchingAccount = _dbContext.UserAccount
                            .Include(a => a.Registration)
                            .FirstOrDefault(a => a.ProfileID == _selectedUserProfile.ProfileID);

                        if (matchingAccount != null)
                        {
                            CurrentAccount.Email = matchingAccount.Email;
                            CurrentAccount.Phone = matchingAccount.Phone;
                            CurrentAccount.PasswordHash = matchingAccount.PasswordHash;
                            CurrentAccount.RegistrationID = matchingAccount.RegistrationID;
                            RegistrationDate = matchingAccount.Registration?.RegistrationDate;
                        }

                        CurrentAccount.ProfileID = _selectedUserProfile.ProfileID;
                        OnPropertyChanged(nameof(CurrentAccount));
                        OnPropertyChanged(nameof(RegistrationDate));
                    }
                }
            }
        }

        private DateTime? _registrationDate;
        public DateTime? RegistrationDate
        {
            get => _registrationDate;
            set
            {
                if (_registrationDate != value)
                {
                    _registrationDate = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public EditAccountViewModel(UserAccount account, RussianDatingAppEntities dbContext)
        {
            _dbContext = dbContext;
            _originalAccount = account;

            _dbContext.UserProfile.Load();
            UserProfiles = _dbContext.UserProfile.Local;

            if (account != null)
            {
                CurrentAccount = new UserAccount
                {
                    UserID = account.UserID,
                    Email = account.Email,
                    Phone = account.Phone,
                    PasswordHash = account.PasswordHash,
                    RegistrationID = account.RegistrationID,
                    ProfileID = account.ProfileID
                };

                var reg = _dbContext.Registration.FirstOrDefault(r => r.RegistrationID == account.RegistrationID);
                RegistrationDate = reg?.RegistrationDate;
            }
            else
            {
                CurrentAccount = new UserAccount();
                RegistrationDate = DateTime.Now;
            }

            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
        }

        private void Save()
        {
            try
            {
                _originalAccount.Email = CurrentAccount.Email;
                _originalAccount.Phone = CurrentAccount.Phone;
                _originalAccount.PasswordHash = CurrentAccount.PasswordHash;
                _originalAccount.RegistrationID = CurrentAccount.RegistrationID;
                _originalAccount.ProfileID = CurrentAccount.ProfileID;

                _dbContext.SaveChanges();
                CloseWindow(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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