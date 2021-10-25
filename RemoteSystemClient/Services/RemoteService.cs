using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using RemoteSystem.Remote;

namespace RemoteSystemClient.Services
{
    public class RemoteService : Remote.RemoteBase
    {
        public override Task<HeartBeat> GetHeartBeat(Empty request, ServerCallContext context)
        {
            HeartBeat heartBeat = new HeartBeat
            {
                Message = "hello",
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm::ss")
            };
            return Task.FromResult(heartBeat);
        }
    }
}
