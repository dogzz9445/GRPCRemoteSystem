using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RemoteSystemManager.Common
{
    [Flags]
    public enum KeyModifier
    {
        None = 0x0000,
        Alt = 0x0001,
        Ctrl = 0x0002,
        AltCtrl = 0x0003,
        Shift = 0x0004,
        ShiftAlt = 0x0005,
        ShiftCtrl = 0x0006,
        Win = 0x0008,
        NoRepeat = 0x4000
    }

    public class HotKeyEventArgs : EventArgs
    {
        public HotKeyEventArgs(KeyModifier keyModifier, Keys key)
        {
            KeyModifier = keyModifier;
            Key = key;
        }

        public HotKeyEventArgs(IntPtr hotKeyParam)
        {
            uint param = (uint)hotKeyParam.ToInt64();
            Key = (Keys)((param & 0xffff0000) >> 16);
            KeyModifier = (KeyModifier)(param & 0x0000ffff);
        }

        public KeyModifier KeyModifier { get; private set; }
        public Keys Key { get; private set; }
    }


    public class HotKey : INotifyPropertyChanged
    {
        private KeyModifier _firstKeyModifier;
        private KeyModifier _secondKeyModifier;
        private Keys _actionKey;

        public KeyModifier FirstKeyModifier
        {
            get => _firstKeyModifier;
            set
            {
                _firstKeyModifier = value;
                RaisePropertyChanged("FirstKeyModifier");
            }
        }

        public KeyModifier SecondKeyModifier
        {
            get => _secondKeyModifier;
            set
            {
                _secondKeyModifier = value;
                RaisePropertyChanged("SecondKeyModifier");
            }
        }

        public Keys ActionKey
        {
            get => _actionKey;
            set
            {
                _actionKey = value;
                RaisePropertyChanged("ActionKey");
            }
        }

        public KeyModifier GetKeyModifier
        {
            get
            {
                return (_firstKeyModifier | _secondKeyModifier);
            }
        }

        public HotKey()
        {
            FirstKeyModifier = KeyModifier.Ctrl;
            SecondKeyModifier = KeyModifier.Shift;
            ActionKey = Keys.A;
        }

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
