namespace discord_template.wol
{
    public class MachineInfo
    {
        public string Name { get; set; }
        public WolInfo wolInfo { get; set; }
        public StdmInfo stdmInfo { get; set; }
        
        public MachineInfo()
        {
            Name = string.Empty;
            wolInfo = new WolInfo();
            stdmInfo = new StdmInfo();
        }
    }

    public class WolInfo
    {
        public string NewtworkIP { get; set; }
        public int Port { get; set; }
        public string MACAddress { get; set; }

        public WolInfo()
        {
            NewtworkIP = string.Empty;
            Port = 0;
            MACAddress = string.Empty;
        }
    }

    public class StdmInfo
    {
        public string IPAddress { get; set; }
        public int Port { get; set; }
        public string message { get; set; }

        public StdmInfo()
        {
            IPAddress = string.Empty;
            Port = 0;
            message = string.Empty;
        }
    }
}
