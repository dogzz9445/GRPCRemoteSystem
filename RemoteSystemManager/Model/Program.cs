using Newtonsoft.Json;
using RemoteSystemManager.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteSystemManager.Model
{
    public class Program : BindableBase
    {
        private bool _isSelected;
        private string _computerName;
        private string _programName;
        private string _programPath;
        private string _programStatus;

        [JsonIgnore]
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        [JsonProperty("ProgramName")]
        public string ProgramName
        {
            get => _programName;
            set => SetProperty(ref _programName, value);
        }

        [JsonProperty("ProgramPath")]
        public string ProgramPath
        {
            get => _programPath;
            set => SetProperty(ref _programPath, value);
        }

        [JsonIgnore]
        public string ProgramProcessName
        {
            get => Path.GetFileName(_programPath);
        }

        [JsonProperty("ComputerName")]
        public string ComputerName
        {
            get => _computerName;
            set => SetProperty(ref _computerName, value);
        }

        [JsonProperty("ProgramStatus")]
        public string ProgramStatus
        {
            get => _programStatus;
            set => SetProperty(ref _programStatus, value);
        }
    }
}
