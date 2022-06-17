using RemoteSystemService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;

namespace RemoteSystemTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            try
            {
                var channel = GrpcChannel.ForAddress("https://localhost:9690");
                //Channel channel = new Channel("192.168.0.105", 9690, ChannelCredentials.SecureSsl);
                var clinet = new Greeter.GreeterClient(channel);
                var reply = clinet.SayHello(new HelloRequest() { Name = "hi" });
                Console.WriteLine($"{reply.Message}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Console.ReadLine();
        }
    }
}
