using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RussianDatingApp.ViewModel
{
    public abstract class ABaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Вызывает событие PropertyChanged для свойства с именем propertyName.
        /// </summary>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual bool SetPropertyChanged<T>(ref T source, T value, [CallerMemberName] string propertyName = null, Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(source, value))
                return false;

            source = value;
            OnPropertyChanged(propertyName);
            onChanged?.Invoke();
            return true;
        }
    }
}