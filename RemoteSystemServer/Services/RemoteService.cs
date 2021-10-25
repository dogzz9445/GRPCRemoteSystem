using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using RemoteSystem.Remote;
using Grpc.Core;
using Microsoft.Extensions.Logging;

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
    }
}