using System;
using Windows.Networking.Connectivity;
using Windows.Networking.NetworkOperators;

namespace Mini.Utils.Windows
{
    public class MobileHotSpot
    {
        public static async void Start()
        {
            var connectionprofile = NetworkInformation.GetInternetConnectionProfile();
            var tethering = NetworkOperatorTetheringManager.CreateFromConnectionProfile(connectionprofile);

            if (tethering.TetheringOperationalState == TetheringOperationalState.On)
            {
                Console.WriteLine("Mobile hotspot is enabled");
                return;
            }
            Console.WriteLine("Start mobile hotspot");
            await tethering.StartTetheringAsync();
        }

        public static async void Stop()
        {
            var connectionprofile = NetworkInformation.GetInternetConnectionProfile();
            var tethering = NetworkOperatorTetheringManager.CreateFromConnectionProfile(connectionprofile);

            if (tethering.TetheringOperationalState == TetheringOperationalState.Off)
            {
                Console.WriteLine("Mobile hotspot is disabled");
                return;
            }
            Console.WriteLine("Stop mobile hotspot");
            await tethering.StopTetheringAsync();
        }
    }
}
