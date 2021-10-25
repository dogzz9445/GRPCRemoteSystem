using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using RemoteSystem.Remote;

namespace RemoteSystemServer
{
    public class RemoteService : Remote.RemoteBase
    {
        private PerformanceCounter m_cpuPerformanceCounter =
            new PerformanceCounter("Processor", "% Processor Time", "_Total");

        private PerformanceCounter m_memoryPerformanceCounter =
            new PerformanceCounter("Memory", "% Committed Bytes in Use");

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
        public override Task<Performance> GetPerformance(Empty request, ServerCallContext context)
        {
            return Task.FromResult(new Performance
            {
                CpuUsage = 94.0F,
                MemoryUsage = 50.0F,
                NetworkUsage = 100.0F
            });
        }

    }
}