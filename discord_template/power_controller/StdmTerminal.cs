using discord_template;
using discord_template.wol;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace discord_wol.power_controller
{
    internal class StdmTerminal
    {
        public static void SendStdmMessage(MachineInfo machine)
        {
            StdmInfo stdminfo = machine.stdmInfo;
            int port = Tools.IsPortNumber(stdminfo.Port) ? stdminfo.Port : 23000;

            try
            {
                byte[] data = Encoding.UTF8.GetBytes(stdminfo.message);

                IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(stdminfo.IPAddress), port);
                using (Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    sock.Connect(ipe);
                    sock.Send(data);
                    sock.Shutdown(SocketShutdown.Both);
                    sock.Close();
                }
            }
            catch
            {
                throw;
            }
        }
    }
}
