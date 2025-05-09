using RussianDatingApp.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace RussianDatingApp.ViewModel
{
    public class EditUserProfileViewModel : INotifyPropertyChanged
    {
        private readonly RussianDatingAppEntities _dbContext;

        public UserProfile CurrentProfile { get; set; }

        public ObservableCollection<City> Cities { get; }
        public ObservableCollection<ZodiacSign> ZodiacSigns { get; }
        public ObservableCollection<AgeCategory> AgeCategories { get; }
        public ObservableCollection<Registration> Registrations { get; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public List<string> Genders { get; } = new List<string> { "Мужской", "Женский" };
        public event PropertyChangedEventHandler PropertyChanged;

        // События для взаимодействия с View
        public event EventHandler<bool> RequestClose;
        public event EventHandler<MessageBoxEventArgs> ShowMessage;

        public EditUserProfileViewModel(UserProfile profile, RussianDatingAppEntities context)
        {
            _dbContext = context ?? throw new ArgumentNullException(nameof(context));
            CurrentProfile = profile ?? throw new ArgumentNullException(nameof(profile));

            // Загружаем справочники
            _dbContext.City.Load();
            _dbContext.ZodiacSign.Load();
            _dbContext.AgeCategory.Load();
            _dbContext.Registration.Load();

            Cities = _dbContext.City.Local;
            ZodiacSigns = _dbContext.ZodiacSign.Local;
            AgeCategories = _dbContext.AgeCategory.Local;
            Registrations = _dbContext.Registration.Local;

            // Значения по умолчанию
            if (CurrentProfile.RegistrationID == 0 && Registrations.Any())
                CurrentProfile.RegistrationID = Registrations.First().RegistrationID;

            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
        }

        private void Save()
        {
            try
            {
                // Простая валидация
                if (string.IsNullOrWhiteSpace(CurrentProfile.FirstName) || string.IsNullOrWhiteSpace(CurrentProfile.LastName))
                {
                    ShowMessage?.Invoke(this, new MessageBoxEventArgs("Поля 'Имя' и 'Фамилия' обязательны.", "Ошибка", MessageBoxImage.Error));
                    return;
                }

                var entry = _dbContext.Entry(CurrentProfile);

                // Если объект отсоединён — прикрепляем
                if (entry.State == EntityState.Detached)
                {
                    _dbContext.UserProfile.Attach(CurrentProfile);
                    entry = _dbContext.Entry(CurrentProfile);
                }

                entry.State = EntityState.Modified;

                _dbContext.SaveChanges();

                ShowMessage?.Invoke(this, new MessageBoxEventArgs("Профиль успешно сохранён.", "Успех", MessageBoxImage.Information));
                RequestClose?.Invoke(this, true);
            }
            catch (Exception ex)
            {
                ShowMessage?.Invoke(this, new MessageBoxEventArgs($"Ошибка при сохранении профиля: {ex.Message}", "Ошибка", MessageBoxImage.Error));
            }
        }

        private void Cancel()
        {
            RequestClose?.Invoke(this, false);
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Обёртка для передачи информации о MessageBox в View.
    /// </summary>
    public class MessageBoxEventArgs : EventArgs
    {
        public string Message { get; }
        public string Caption { get; }
        public MessageBoxImage Icon { get; }

        public MessageBoxEventArgs(string message, string caption, MessageBoxImage icon)
        {
            Message = message;
            Caption = caption;
            Icon = icon;
        }
    }
}
