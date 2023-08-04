using discord_template;
using discord_template.wol;
using Newtonsoft.Json;

namespace discord_wolbot.power_controller
{
    internal class MachineInfoTerminal
    {
        public static string GetMachineInfo(string machineName)
        {
            string machineInfo = string.Empty;

            MachineInfo machine = Settings.Shared.m_WolMachines.FirstOrDefault(_ => _.Key == machineName).Value;
            if (machine != null)
            {
                machineInfo = JsonConvert.SerializeObject(machine, Formatting.Indented);
            }
            else
            {
                machineInfo = "unknown machine " + machineName; 
            }

            return machineInfo;
        }
    }
}
