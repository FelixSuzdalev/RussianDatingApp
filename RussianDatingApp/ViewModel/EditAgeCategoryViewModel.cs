using RussianDatingApp.Model;
using System;
using System.ComponentModel;
using System.Data.Entity.Infrastructure;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace RussianDatingApp.ViewModel
{
    public class EditAgeCategoryViewModel : INotifyPropertyChanged
    {
        private readonly AgeCategory _original;
        private readonly RussianDatingAppEntities _dbContext;

        public AgeCategory CurrentAgeCategory { get; set; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public EditAgeCategoryViewModel(AgeCategory category, RussianDatingAppEntities dbContext)
        {
            _original = category;
            _dbContext = dbContext;

            // Если переданный объект категории не null, создаем копию для редактирования.
            CurrentAgeCategory = new AgeCategory
            {
                AgeCategoryID = category.AgeCategoryID,
                AgeFrom = category.AgeFrom,
                AgeTo = category.AgeTo,
                Description = category.Description
            };

            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
        }

        private void Save()
        {
            // Проверка, что возраст "от" не больше возраста "до"
            if (CurrentAgeCategory.AgeFrom > CurrentAgeCategory.AgeTo)
            {
                MessageBox.Show("Возраст 'от' не может быть больше возраста 'до'.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                // Если это новая категория (AgeCategoryID == 0), добавляем ее в базу данных
                if (CurrentAgeCategory.AgeCategoryID == 0)
                {
                    _dbContext.AgeCategory.Add(CurrentAgeCategory);
                }
                else
                {
                    // Если это существующая категория, обновляем ее данные
                    _dbContext.Entry(_original).CurrentValues.SetValues(CurrentAgeCategory);
                }

                _dbContext.SaveChanges();
                CloseWindow(true);
            }
            catch (DbUpdateException ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.InnerException?.Message ?? ex.Message}",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                var message = new StringBuilder();
                var current = ex;
                while (current != null)
                {
                    message.AppendLine(current.Message);
                    current = current.InnerException;
                }

                MessageBox.Show($"Ошибка при сохранении:\n{message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
