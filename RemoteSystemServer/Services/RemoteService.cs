﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        private readonly ILogger<RemoteService> _logger;
        public RemoteService(ILogger<RemoteService> logger)
        {
            _logger = logger;
        }

        public override Task<HeartBeat> GetHeartBeat(Empty request, ServerCallContext context)
        {
            return Task.FromResult(new HeartBeat
            {
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                Message = "Hello "
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
                if (!string.IsNullOrEmpty(request.ProcessName))
                {
                    string processName = Path.GetFileName(request.ProcessName).Split('.')[0];
                    foreach (Process process in Process.GetProcesses())
                    {
                        if (process.ProcessName.StartsWith(processName))
                        {
                            process.Kill();
                        }
                    }
                }
            }
            return Task.FromResult(new MessageResult
            {
                Result = MessageResult.Types.MessageResultType.Success,
                ResultMessage = "suc"
            });
        }
    }
}