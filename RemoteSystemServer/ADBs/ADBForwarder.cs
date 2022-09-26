using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Collections.Generic;

using SharpAdbClient;
using ICSharpCode.SharpZipLib.Zip;

namespace RemoteSystemServer
{
    public class ADBForwarder
    {
        private readonly string[] deviceNames =
        {
            "monterey",        // Oculus Quest 1
            "hollywood",       // Oculus Quest 2
            "pacific",         // Oculus Go
            "vr_monterey",     // Edge case for linux, Quest 1
            "vr_hollywood",    // Edge case for linux, Oculus Quest 2
            "vr_pacific"       // Edge case for linux, Oculus Go
        };

        private readonly AdbClient client = new AdbClient();
        private readonly AdbServer server = new AdbServer();
        private readonly IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, AdbClient.AdbServerPort);
        private readonly LogShellOutputReceiver shellOutputReceiver = new LogShellOutputReceiver();
        private readonly LogEventOutputReceiver eventOutputReceiver = new LogEventOutputReceiver();

        private bool connectionSession = false;
        private Dictionary<string, CancellationTokenSource> HashTokenSources;
        private Dictionary<string, DeviceData> HashDeviceData;

        private DeviceData _deviceData;

        public DeviceData DeviceData { get => _deviceData; set => _deviceData = value; }

        public void Initialize()
        {
            Console.ResetColor();
            if (HashTokenSources != null)
            {
                Console.WriteLine($"Cancel all task tokens {HashTokenSources.Count}");
                foreach (var (key, tokenSource) in HashTokenSources)
                {
                    tokenSource.Cancel();
                    tokenSource.Dispose();
                }
            }
            if (HashDeviceData != null)
            {
                Console.WriteLine($"Dispose all device data {HashDeviceData.Count}");
                foreach (var (key, deviceData) in HashDeviceData)
                {

                }
            }
            HashTokenSources = new Dictionary<string, CancellationTokenSource>();
            HashDeviceData = new Dictionary<string, DeviceData>();

            var currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (currentDirectory == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Path error!");
                return;
            }
            eventOutputReceiver.OutputEvent += OnResumedActivityChecked;

            var adbPath = "adb/platform-tools/{0}";
            var downloadUri = "https://dl.google.com/android/repository/platform-tools-latest-{0}.zip";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Console.WriteLine("Platform: Linux");

                adbPath = string.Format(adbPath, "adb");
                downloadUri = string.Format(downloadUri, "linux");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Console.WriteLine("Platform: Windows");

                adbPath = string.Format(adbPath, "adb.exe");
                downloadUri = string.Format(downloadUri, "windows");
            }
            else
            {
                Console.WriteLine("Unsupported platform!");
                return;
            }

            var absoluteAdbPath = Path.Combine(currentDirectory, adbPath);
            if (!File.Exists(absoluteAdbPath))
            {
                Console.WriteLine("ADB not found, downloading in the background...");
                DownloadADB(downloadUri);

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    SetExecutable(absoluteAdbPath);
            }

            Console.WriteLine("Starting ADB Server...");
            server.StartServer(absoluteAdbPath, false);

            client.Connect(endPoint);

            var monitor = new DeviceMonitor(new AdbSocket(endPoint));
            monitor.DeviceConnected += Monitor_DeviceConnected;
            monitor.DeviceDisconnected += Monitor_DeviceDisconnected;
            monitor.Start();
        }

        private async void Monitor_DeviceConnected(object sender, DeviceDataEventArgs e)
        {
            HashTokenSources[e.Device.Name] = new CancellationTokenSource();
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine($"Connected device: {e.Device.Serial}");
            Connect(e.Device);

            await Task.Delay(1000);

            Forward();
            await StartALVRClient(device: e, isLoop: true);
        }

        public async Task StartALVRClient(DeviceDataEventArgs device, bool isLoop = false)
        {
            if (DeviceData == null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Failed to start alvr client app");
                return;
            }

            client.ExecuteRemoteCommand("am start -n alvr.client.quest/com.polygraphene.alvr.OvrActivity", DeviceData, shellOutputReceiver);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Successfully start app: {DeviceData.Serial} [{DeviceData.Product}");

            if (isLoop)
            {
                await CheckResumedActivity(device);
            }
        }

        public async Task CheckResumedActivity(DeviceDataEventArgs e)
        {
            await Task.Delay(5000);

            if (HashTokenSources[e.Device.Name].IsCancellationRequested)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"CancellationToken is requested");
                return;
            }

            client.ExecuteRemoteCommand("dumpsys activity activities | grep mResumedActivity", DeviceData, eventOutputReceiver);
        }

        private async void OnResumedActivityChecked(object sender, string log)
        {
            if (!log.Contains("com.oculus"))
            {
                await Task.Delay(1000);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(log);
                Console.WriteLine($"Stopped ALVR client: {DeviceData.Serial} [{DeviceData.Product}");

                await StartALVRClient(isLoop: true);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Check resumedActivity: {log}");

                await CheckResumedActivity();
            }
        }

        private void Monitor_DeviceDisconnected(object sender, DeviceDataEventArgs e)
        {
            HashTokenSources[e.Device.Name].Cancel();
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"Disconnected device: {e.Device.Serial}");
        }

        public void ForceStopALVRClient()
        {
            if (DeviceData == null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Failed to stop alvr client app");
                return;
            }

            client.ExecuteRemoteCommand("am force-stop alvr.client.quest", DeviceData, shellOutputReceiver);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Successfully stop app: {DeviceData.Serial} [{DeviceData.Product}");
        }

        private void Connect(DeviceData device)
        {
            // DeviceConnected calls without product set yet
            Thread.Sleep(1000);

            foreach (var deviceData in client.GetDevices().Where(deviceData => device.Serial == deviceData.Serial))
            {
                if (!deviceNames.Contains(deviceData.Product))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Skipped connecting device: {(string.IsNullOrEmpty(deviceData.Product) ? deviceData.Serial : deviceData.Product)}");
                    return;
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Successfully connected device: {deviceData.Serial} [{deviceData.Product}]");
                DeviceData = deviceData;

                return;
            }
        }

        private void Forward()
        {
            if (DeviceData == null)
            {
                Console.WriteLine($"Failed to forward device");
                return;
            }

            client.CreateForward(DeviceData, 9943, 9943);
            client.CreateForward(DeviceData, 9944, 9944);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Successfully forwarded device: {DeviceData.Serial} [{DeviceData.Product}]");
        }

        private void DownloadADB(string downloadUri)
        {
            using var web = new WebClient();
            web.DownloadFile(downloadUri, "adb.zip");
            Console.WriteLine("Download successful");

            var zip = new FastZip();
            zip.ExtractZip("adb.zip", "adb", null);
            Console.WriteLine("Extraction successful");

            File.Delete("adb.zip");
        }

        private void SetExecutable(string fileName)
        {
            Console.WriteLine("Giving adb executable permissions");

            var args = $"chmod u+x {fileName}";
            var escapedArgs = args.Replace("\"", "\\\"");

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\""
                }
            };

            process.Start();
            process.WaitForExit();
        }
    }
}