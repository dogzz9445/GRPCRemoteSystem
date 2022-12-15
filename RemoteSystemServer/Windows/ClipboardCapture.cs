using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

namespace RemoteSystemServer.Windows
{
    public class ClipboardCapture
    {

        public async Task GetDataAsync()
        {
            var data = Clipboard.GetContent();
            var text = await data.GetTextAsync();

        }
    }
}
