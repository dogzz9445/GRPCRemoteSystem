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

        public bool IsSelected 
        { 
            get => _isSelected; 
            set => SetProperty(ref _isSelected, value); 
        }

        public string ProgramName
        { 
            get => _programName;
            set => SetProperty(ref _programName, value);
        }
        public string ProgramPath
        {
            get => _programPath;
            set => SetProperty(ref _programPath, value);
        }

        public string ProgramProcessName
        {
            get => Path.GetFileName(_programPath);
        }
        public string ComputerName 
        {
            get => _computerName; 
            set => SetProperty(ref _computerName, value);
        }
    }
}
