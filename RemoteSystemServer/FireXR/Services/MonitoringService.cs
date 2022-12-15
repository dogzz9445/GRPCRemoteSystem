using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mini.Utils.Windows;

namespace RemoteSystemServer.Services
{
    public class MonitoringService : BackgroundService
    {
        private readonly ILogger<MonitoringService> _logger;
        private Timer? _timer = null;
        private float _monitoringDelay = 5000;
        private bool _isMonitoringADB = false;
        private bool _isMonitoringHotSpot = false;

        public IServiceProvider Services;

        private static ADBForwarder _adbForwarder;
        public static ADBForwarder AdbForwarder { get => _adbForwarder; set => _adbForwarder = value; }

        public MonitoringService(IServiceProvider services, ILogger<MonitoringService> logger)
        {
            Services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("FireXR Service running");

            var appsettings = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            var startup_adb = appsettings.GetValue<bool>("FireXR:startup_adb", false);
            var startup_hotspot = appsettings.GetValue<bool>("FireXR:startup_hotspot", false);
            _monitoringDelay = appsettings.GetValue<float>("FireXR:monitoring_delay", 5000);

            if (startup_adb)
            {
                if (AdbForwarder == null)
                {
                    AdbForwarder = new ADBForwarder();
                    AdbForwarder.Initialize();
                }
            }
            if (startup_hotspot)
            {
                await MobileHotSpot.StartAsync();
            }

            //await DoWork(stoppingToken);
        }

        private async Task DoWork(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await MonitoringADB();
                //await MonitoringMobileHotSpot();
                await Task.Delay(5000);
            }
            //using (var scope = Services.CreateScope())
            //{
            //    var scopedProcessingService =
            //        scope.ServiceProvider
            //            .GetRequiredService<IScopedProcessingService>();

            //    await scopedProcessingService.DoWork(stoppingToken);
            //}
        }

        private async Task MonitoringADB()
        {

        }

        //private async Task MonitoringMobileHotSpot()
        //{
        //    if (MobileHotSpot.GetState() != Windows.Networking.NetworkOperators.TetheringOperationalState.On)
        //    {
        //        await MobileHotSpot.StartAsync();
        //    }
        //}

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("FireXR Service stopping");

            await base.StopAsync(stoppingToken);
        }

    }
}
