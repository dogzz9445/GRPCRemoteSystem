using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;

namespace Mini.Utils.Windows
{
    public class MacAddress
    {
        public static List<string> GetMacAddresses()
        {
            return NetworkInterface.GetAllNetworkInterfaces()
                .Where(nic =>
                    nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .Select(nic => nic.GetPhysicalAddress().ToString()).ToList();
        }

        public static List<string> GetMacAddressesAlive()
        {
            return NetworkInterface.GetAllNetworkInterfaces()
                .Where(nic =>
                    nic.OperationalStatus == OperationalStatus.Up &&
                    nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .Select(nic => nic.GetPhysicalAddress().ToString()).ToList();
        }
    }
}
