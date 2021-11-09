using Newtonsoft.Json;
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
        private string _computerMacAddress;
        private ItemObservableCollection<Program> _programs;

        [JsonIgnore]
        public bool IsSelected 
        { 
            get => _isSelected; 
            set => SetProperty(ref _isSelected, value); 
        }

        [JsonProperty("ComputerName")]
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

        [JsonProperty("ComputerIp")]
        public string ComputerIp
        {
            get => _computerIp;
            set => SetProperty(ref _computerIp, value);
        }

        [JsonProperty("Programs")]
        public ItemObservableCollection<Program> Programs
        {
            get => _programs;
            set => _programs = value;
        }

        [JsonProperty("ComputerMacAddress")]
        public string ComputerMacAddress
        {
            get => _computerMacAddress;
            set => SetProperty(ref _computerMacAddress, value);
        }

        public Computer()
        {
            _programs = new ItemObservableCollection<Program>();
        }

        public void AddProgram(Program program)
        {
            program.ComputerName = ComputerName;
            _programs.Add(program);
        }
    }
}
