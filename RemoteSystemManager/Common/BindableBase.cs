using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RemoteSystemManager.Common
{
    public class BindableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return false;
            }
            storage = value;
            RaisePropertyChangedEvent(propertyName);
            return true;
        }

        protected bool SetObservableProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null) where T : INotifyPropertyChanged
        {
            if (storage != null)
            {
                storage.PropertyChanged -= new PropertyChangedEventHandler(RaisePropertyChangedEvent);
            }
            bool result = SetProperty(ref storage, value);
            if (storage != null)
            {
                storage.PropertyChanged += new PropertyChangedEventHandler(RaisePropertyChangedEvent);
            }
            return result;
        }

        //protected bool SetObservableCollection<T>(ref T storage, T value, [CallerMemberName] string propertyName = null) where T : IColl

        protected void RaisePropertyChangedEvent([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void RaisePropertyChangedEvent(object sender, PropertyChangedEventArgs eventArgs)
        {
            PropertyChanged?.Invoke(sender, eventArgs);
        }
    }
}
