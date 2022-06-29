using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using RemoteSystem.Remote;

namespace RemoteSystemServer
{
    public class RemoteService : Remote.RemoteBase
    {
        private ADBForwarder _adbForwarder;

        private readonly ILogger<RemoteService> _logger;
        public RemoteService(ILogger<RemoteService> logger)
        {
            _logger = logger;

            //_logger.Log(LogLevel.Information, "Start RemoteService");

            //_adbForwarder = new ADBForwarder();
            //_adbForwarder.Initialize();

            //MobileHotSpot.Start();
        }

        //public RemoteService(ILoggerFactory logger)
        //{
        //    _logger = logger.CreateLogger<RemoteService>();

        //    _logger.Log(LogLevel.Information, "Start RemoteService");

        //    _adbForwarder = new ADBForwarder();
        //    _adbForwarder.Initialize();

        //    MobileHotSpot.Start();
        //}

        public override Task<HeartBeat> GetHeartBeat(Empty request, ServerCallContext context)
        {
            return Task.FromResult(new HeartBeat
            {
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                Message = "Hello"
            });
        }

        public override Task<MessageMacAddress> GetMacAddress(Empty request, ServerCallContext context)
        {
            var macAddress = NetworkInterface.GetAllNetworkInterfaces()
                .Where(nic =>
                    nic.OperationalStatus == OperationalStatus.Up &&
                    nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .Select(nic => nic.GetPhysicalAddress().ToString()).FirstOrDefault();
            
            return Task.FromResult(new MessageMacAddress
            {
                MacAddress = macAddress
            });
        }

        public override Task<Performance> GetPerformance(Empty request, ServerCallContext context)
        {
            return Task.FromResult(new Performance
            {
                CpuUsage = 94.0F,
                MemoryUsage = 50.0F,
                NetworkUsage = 100.0F
            });
        }

        public override Task<Empty> PostComputerControlMessage(ComputerControl request, ServerCallContext context)
        {
            if (request.Control == ComputerControl.Types.ComputerControlType.Start)
            {

            }
            else if (request.Control == ComputerControl.Types.ComputerControlType.Restart)
            {
                Process.Start("shutdown.exe", "-r");
            }
            else if (request.Control == ComputerControl.Types.ComputerControlType.Stop)
            {
                Process.Start("shutdown.exe", "-s -f");
            }
            return Task.FromResult(new Empty());
        }

        public override Task<MessageResult> PostProgramControlMessage(ProgramControl request, ServerCallContext context)
        {
            if (request.Control == ProgramControl.Types.ProgramControlType.Start)
            {
                // TODO:
                // 고치기
                Process.Start(request.FileName);
            }
            else if (request.Control == ProgramControl.Types.ProgramControlType.Stop)
            {
                foreach (Process process in Process.GetProcesses())
                {
                    if (process.ProcessName.StartsWith(request.ProcessName))
                    {
                        process.Kill();
                    }
                }
            }
            return Task.FromResult(new MessageResult
            {
                Result = MessageResult.Types.MessageResultType.Success,
                ResultMessage = "Success"
            });
        }

        public override Task<MessageResult> PostVRControlMessage(VRControl request, ServerCallContext context)
        {
            if (request.Control == VRControl.Types.VRControlType.AlvrClientStart)
            {
                if (_adbForwarder == null)
                {
                    _adbForwarder = new ADBForwarder();
                    _adbForwarder.Initialize();
                }
                _adbForwarder.StartALVRClient();
            }
            else if (request.Control == VRControl.Types.VRControlType.AlvrClientStop)
            {
                if (_adbForwarder != null)
                {
                    _adbForwarder.ForceStopALVRClient();
                }
            }
            else if (request.Control == VRControl.Types.VRControlType.MobileHotspotStart)
            {
                MobileHotSpot.Start();
            }
            else if (request.Control == VRControl.Types.VRControlType.MobileHotspotStop)
            {
                MobileHotSpot.Stop();
            }

            return Task.FromResult(new MessageResult
            {
                Result = MessageResult.Types.MessageResultType.Success,
                ResultMessage = "Success"
            });
        }
    }
}