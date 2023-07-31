using discord_template.wol;
using Newtonsoft.Json;
using System.Configuration;
using System.Text;

namespace discord_template
{
    internal class Settings
    {

        private readonly object LockSet = new object();

        private static Cache<Settings> CachedSettings = new Cache<Settings>(() => new Settings());
        public static Settings Shared => CachedSettings.Value;


        public readonly string m_Token;
        public readonly string m_DiscordAPIVersion;
        public readonly string m_ApplicationId;
        public readonly string[] m_GuildIds;
        public readonly string[] m_AdminIds;

        public Dictionary<string, MachineInfo> m_WolMachines;


        public Settings()
        {
            #region AppSettingsReaderからの設定読み込み
            var reader = new AppSettingsReader();
            var error_code = string.Empty;

            //config内のtokenを取得する
            m_Token = (string)reader.GetValue("token", typeof(string));
            if (m_Token.IsNullOrEmpty()) { throw new Exception($"{nameof(m_Token)}\ntokenがnullもしくは空白です。"); }
            else if(m_Token == "{token}") { error_code += "Must setup token in App.config\n"; }

            //config内のdiscordapi_versionを取得する
            m_DiscordAPIVersion = (string)reader.GetValue("discordapi_version", typeof(string));
            if (m_DiscordAPIVersion.IsNullOrEmpty()) { throw new Exception($"{m_DiscordAPIVersion}.\ndiscordapi_versionがnullもしくは空白です。"); }

            //config内のapplication_idを取得する
            m_ApplicationId = (string)reader.GetValue("application_id", typeof(string));
            if (m_ApplicationId.IsNullOrEmpty()) { throw new Exception($"{nameof(m_ApplicationId)}.\napplication_idがnullもしくは空白です。"); }
            else if (m_ApplicationId == "{application_id}") { error_code += "Must setup application_id in App.config\n"; }

            //config内の","で区切られたguild_idを取得する
            var guildId = (string)reader.GetValue("guild_id", typeof(string));
            if (guildId.IsNullOrEmpty()) { throw new Exception($"{nameof(guildId)}.\nguild_idがnullもしくは空白です。"); }
            else if(guildId == "{guild_id}") { error_code += "Must setup guild_id in App.config\n"; }
            m_GuildIds = guildId!.Split(',');

            //config内の","で区切られたadmin_idを取得する
            string adminId = (string)reader.GetValue("admin_id", typeof(string));
            if (adminId.IsNullOrEmpty()) { throw new Exception($"{nameof(adminId)}.\nadmin_idがnullもしくは空白です。"); }
            else if(adminId == "{user_id}") { error_code += "Must setup admin_id in App.config\n"; }
            m_AdminIds = adminId!.Split(',');

            if(error_code != string.Empty)
            {
                Console.WriteLine(error_code);
                Environment.Exit(0);
            }

            m_WolMachines = GetMachineInfo();
            #endregion
        }

        private Dictionary<string, MachineInfo> GetMachineInfo()
        {
            Dictionary<string, MachineInfo> machines = new();
            
            string jsonstr = string.Empty;
            using (StreamReader sr = new StreamReader($"{Directory.GetCurrentDirectory()}/config/machines.json"))
            {
                jsonstr = sr.ReadToEnd();
                sr.Close();
            }

            List<MachineInfo> machineinfo = JsonConvert.DeserializeObject<List<MachineInfo>>(jsonstr)!;

            foreach(var info in machineinfo)
            {
                machines.Add(info.Name, info);
            }

            return machines;
        }

        private void WriteMachineInfo()
        {
            _ = Task.Run(() =>
            {
                if (!Monitor.TryEnter(LockSet))
                {
                    return;
                }

                try
                {
                    List<MachineInfo> machinesInfo;
                    Dictionary<string, MachineInfo> temp_Machines;

                    do
                    {
                        temp_Machines = new(m_WolMachines);
                        machinesInfo = new(m_WolMachines.Values);
                        string savejson = JsonConvert.SerializeObject(machinesInfo, Formatting.Indented);
                        Console.WriteLine(savejson);
                        using (StreamWriter sw = new StreamWriter($"{Directory.GetCurrentDirectory()}/config/machines.json", false, Encoding.UTF8))
                        {
                            sw.Write(savejson);
                        }

                    } while ((temp_Machines.Count != m_WolMachines.Count) || temp_Machines.All(_ => !m_WolMachines.TryGetValue(_.Key, out var value) || value != _.Value));
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error: {e}");
                }
                finally
                { 
                    Monitor.Exit(LockSet);
                }
            });
        }

        internal void ReloadMachineInfo()
        {
            m_WolMachines = GetMachineInfo();
        }

        internal void SetEditMachineInfo(MachineInfo machineInfo)
        {
            MachineInfo targetInfo = m_WolMachines.FirstOrDefault(_ => _.Key == machineInfo.Name).Value;

            try
            {
                if (targetInfo == null)
                {
                    targetInfo = machineInfo;
                    m_WolMachines.Add(targetInfo.Name, targetInfo);
                }
                else
                {
                    targetInfo = machineInfo;
                    m_WolMachines[targetInfo.Name] = targetInfo;
                }
                WriteMachineInfo();
            }
            catch
            {
                throw;
            }
        }
    }
}
