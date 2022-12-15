using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RemoteSystemServer.FireXR
{
    interface IMonitorable
    {
        Task Monitor();
    }
}
