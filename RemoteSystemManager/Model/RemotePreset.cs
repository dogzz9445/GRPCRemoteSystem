using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteSystemManager.Common;

namespace RemoteSystemManager.Model
{
    public class RemotePreset : BindableBase
    {
        private string _presetName;
        private ItemObservableCollection<RemoteComputer> _computers;
    }
}
