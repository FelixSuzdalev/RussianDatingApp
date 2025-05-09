using RussianDatingApp.Model;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
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
            // Валидация Email и Phone перед сохранением
            if (!IsValidEmail(CurrentAccount.Email))
            {
                MessageBox.Show("Некорректный формат email.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!IsValidPhone(CurrentAccount.Phone))
            {
                MessageBox.Show("Некорректный формат телефона.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

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

        // Метод для проверки формата email с использованием регулярного выражения
        private bool IsValidEmail(string email)
        {
            var emailRegex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
            return emailRegex.IsMatch(email);
        }

        // Метод для проверки формата телефона (например, российский номер)
        private bool IsValidPhone(string phone)
        {
            var phoneRegex = new Regex(@"^\+7\d{10}$"); // Формат: +7XXXXXXXXXX
            return phoneRegex.IsMatch(phone);
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
