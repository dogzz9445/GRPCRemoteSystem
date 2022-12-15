using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteSystemManager.Common;

namespace RemoteSystemManager.Model
{
    public class RemoteComputer : BindableBase
    {
        private bool _isSelected;
        private string _computerName;
        private string _computerIp;
        private string _computerMacAddress;
        private string _computerStatus;
        private ItemObservableCollection<RemoteProgram> _programs;

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
        public ItemObservableCollection<RemoteProgram> Programs
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

        [JsonProperty("ComputerStatus")]
        public string ComputerStatus
        {
            get => _computerStatus;
            set => SetProperty(ref _computerStatus, value);
        }

        public RemoteComputer()
        {
            _programs = new ItemObservableCollection<RemoteProgram>();
        }

        public void AddProgram(RemoteProgram program)
        {
            program.ComputerName = ComputerName;
            _programs.Add(program);
        }
    }
}
