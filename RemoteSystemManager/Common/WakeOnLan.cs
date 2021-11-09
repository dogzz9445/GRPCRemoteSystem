using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RemoteSystemManager.Common
{
    public static class WakeOnLan
    {
        private const int PING_TIMEOUT = 1000;
        private const int WOL_PACKET_LEN = 102;

        [DllImport("iphlpapi.dll", ExactSpelling = true)]
        private static extern int SendARP(int DestIP, int SrcIP, byte[] pMacAddr, ref uint PhyAddrLen);

        // ------------------------------------------------------------
        //
        // public
        //
        // ------------------------------------------------------------

        public static async Task<string> GetMACAddressFromARP(string hostNameOrAddress)
        {
            if (!IsHostAccessible(hostNameOrAddress))
                return null;

            IPHostEntry hostEntry = await Dns.GetHostEntryAsync(hostNameOrAddress);

            if (hostEntry.AddressList.Length == 0)
                return null;

            byte[] macAddress = new byte[6];
            uint macAddressLength = (uint)macAddress.Length;
            var hResult = await Task.Run(() => SendARP((int) hostEntry.AddressList[0].Address, 0, macAddress, ref macAddressLength));
            
            if (hResult != 0)
                return null;

            StringBuilder macAddressString = new StringBuilder();
            foreach (var macAddressByte in macAddress)
            {
                if (macAddressString.Length > 0)
                {
                    macAddressString.Append(":");
                }
                macAddressString.AppendFormat("{0:x2}", macAddressByte);
            }
            return macAddressString.ToString();
        }

        public static async Task SendMagicPacketAsync(string macAddress)
        {
            try
            {
                byte[] wolBuffer = GetWolPacket(macAddress);

                UdpClient udpClient = new UdpClient();
                udpClient.EnableBroadcast = true;

                IPAddress broadCastAddress = IPAddress.Parse("255.255.255.255");
                await udpClient.SendAsync(wolBuffer, wolBuffer.Length, broadCastAddress.ToString(), 7);
                await udpClient.SendAsync(wolBuffer, wolBuffer.Length, broadCastAddress.ToString(), 9);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        // ------------------------------------------------------------
        //
        // private
        //
        // ------------------------------------------------------------

        private static bool IsHostAccessible(string hostNameOrAddress)
        {
            Ping ping = new Ping();
            PingReply reply = ping.Send(hostNameOrAddress, PING_TIMEOUT);
            return reply.Status == IPStatus.Success;
        }

        private static byte[] GetWolPacket(string macAddress)
        {
            byte[] datagram = new byte[WOL_PACKET_LEN];
            byte[] macBuffer = StringToBytes(macAddress);

            MemoryStream ms = new MemoryStream(datagram);
            BinaryWriter bw = new BinaryWriter(ms);

            for (int i = 0; i < 6; i++)
            {
                bw.Write((byte)0xff);
            }

            for (int i = 0; i < 16; i++)
            {
                bw.Write(macBuffer, 0, macBuffer.Length);
            }

            return datagram;
        }

        private static byte[] StringToBytes(string macAddress)
        {
            macAddress = Regex.Replace(macAddress, "[-|:]", "");
            byte[] buffer = new byte[macAddress.Length / 2];

            for (int i = 0; i < macAddress.Length; i += 2)
            {
                string digit = macAddress.Substring(i, 2);
                buffer[i / 2] = byte.Parse(digit, NumberStyles.HexNumber);
            }

            return buffer;
        }
    }
}
