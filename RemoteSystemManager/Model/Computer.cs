using RemoteSystemManager.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteSystemManager.Model
{
    public class Computer : BindableBase
    {
        private bool _isSelected;
        private string _computerName;
        private string _computerIp;
        private ItemObservableCollection<Program> _programs;

        public bool IsSelected 
        { 
            get => _isSelected; 
            set => SetProperty(ref _isSelected, value); 
        }

        public string ComputerName
        {
            get => _computerName;
            set
            {
                foreach (var program in _programs)
                {
                    program.ComputerName = value;
                }
                SetProperty(ref _computerName, value);
            }
        }
        
        public string ComputerIp
        {
            get => _computerIp;
            set => SetProperty(ref _computerIp, value);
        }

        public ItemObservableCollection<Program> Programs
        {
            get => _programs;
            set => _programs = value;
        }
        public Computer()
        {
            _programs = new ItemObservableCollection<Program>();
        }
    }
}
