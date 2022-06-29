using System;
using System.Collections.Concurrent;

using SharpAdbClient;

namespace RemoteSystemServer
{
    public class LogShellOutputReceiver : IShellOutputReceiver
    {
        public readonly ConcurrentQueue<string> LogShellOutputs = new ConcurrentQueue<string>();

        public bool ParsesErrors => false;

        public void AddOutput(string line)
        {
            LogShellOutputs.Enqueue(line);
        }

        public void Flush()
        {
            string outputBuffer;
            while (!LogShellOutputs.IsEmpty)
            {
                if (LogShellOutputs.TryDequeue(out outputBuffer))
                {
                    Console.WriteLine(outputBuffer);
                }
            }
        }
    }
}
