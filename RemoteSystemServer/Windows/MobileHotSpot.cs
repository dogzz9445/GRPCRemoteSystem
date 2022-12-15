using System;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;
using Windows.Networking.NetworkOperators;

namespace Mini.Utils.Windows
{
    public class MobileHotSpot
    {
        private static NetworkOperatorTetheringManager _manager;
        public static NetworkOperatorTetheringManager Manager
        {
            get
            {
                if (_manager != null)
                {
                    return _manager;
                }
                var connectionprofile = NetworkInformation.GetInternetConnectionProfile();
                _manager = NetworkOperatorTetheringManager.CreateFromConnectionProfile(connectionprofile);
                return _manager;
            }
        }

        public static NetworkOperatorTetheringManager CreateManager()
        {
            var connectionprofile = NetworkInformation.GetInternetConnectionProfile();
            var tethering = NetworkOperatorTetheringManager.CreateFromConnectionProfile(connectionprofile);
            return tethering;
        }

        public static async Task StartAsync()
        {
            var connectionprofile = NetworkInformation.GetInternetConnectionProfile();
            var tethering = NetworkOperatorTetheringManager.CreateFromConnectionProfile(connectionprofile);

            if (Manager.TetheringOperationalState == TetheringOperationalState.On)
            {
                Console.WriteLine("Mobile hotspot is already enabled");
                return;
            }
            Console.WriteLine("Start mobile hotspot");
            await tethering.StartTetheringAsync();
        }

        public static async Task StopAsync()
        {
            var connectionprofile = NetworkInformation.GetInternetConnectionProfile();
            var tethering = NetworkOperatorTetheringManager.CreateFromConnectionProfile(connectionprofile);

            if (tethering.TetheringOperationalState == TetheringOperationalState.Off)
            {
                Console.WriteLine("Mobile hotspot is already disabled");
                return;
            }
            Console.WriteLine("Stop mobile hotspot");
            await tethering.StopTetheringAsync();
        }

        public static TetheringOperationalState GetState()
        {
            var connectionprofile = NetworkInformation.GetInternetConnectionProfile();
            var tethering = NetworkOperatorTetheringManager.CreateFromConnectionProfile(connectionprofile);

            return tethering.TetheringOperationalState;
        }

        public static uint GetClientCount()
        {
            var connectionprofile = NetworkInformation.GetInternetConnectionProfile();
            var tethering = NetworkOperatorTetheringManager.CreateFromConnectionProfile(connectionprofile);

            return tethering.ClientCount;
        }

        public static bool IsOn()
        {
            return GetState() == TetheringOperationalState.On;
        }

        public static bool IsOff()
        {
            return GetState() == TetheringOperationalState.On;
        }
    }
}
