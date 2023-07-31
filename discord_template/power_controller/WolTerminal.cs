using System.Net;
using System.Net.Sockets;

namespace discord_template.wol
{
    internal class WolTerminal
    {
        public static void SendMagicPacket(MachineInfo machine)
        {
            WolInfo wolinfo = machine.wolInfo;
            int port = Tools.IsPortNumber(wolinfo.Port) ? wolinfo.Port : 12000; 
            MemoryStream stream = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(stream);
            string[] macstr = wolinfo.MACAddress.Split(":");
            byte[] macaddress = new byte[6];

            try
            {
                for (int i = 0; i < 6; i++)
                {
                    bw.Write((byte)0xff);
                }
                for (int i = 0; i < macaddress.Length; i++)
                {
                    macaddress[i] = Convert.ToByte(macstr[i], 16);
                }
                for (int i = 0; i < 16; i++)
                {
                    bw.Write(macaddress);
                }

                UdpClient client = new UdpClient();
                client.EnableBroadcast = true;
                client.Send(stream.ToArray(), (int)stream.Position, new IPEndPoint(IPAddress.Parse(wolinfo.NewtworkIP), port));
            }
            catch
            {
                throw;
            }
        }
    }
}
