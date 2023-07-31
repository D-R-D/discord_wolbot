using Discord;
using Discord.Commands;
using Discord.WebSocket;
using discord_template.wol;
using discord_wol;
using discord_wol.power_controller;
using System.Configuration;
using System.Net;

namespace discord_template
{
    class Program
    {
        public static AppSettingsReader reader = new AppSettingsReader();

        private static DiscordSocketClient? _client;
        private static CommandService? _commands;

        public static void Main(string[] args)
        {
            InitDirectory.init();

            // ギルドコマンドを登録する
            CommandSender.RegisterGuildCommands();
            Console.WriteLine("CommandSender SUCCESS!!");

            _ = new Program().MainAsync();

            Thread.Sleep(-1);
        }

        public async Task MainAsync()
        {
            _client = new DiscordSocketClient();
            _client.Log += Log;
            _client.Ready += Client_Ready;
            _client.SlashCommandExecuted += SlashCommandHandler;
            _client.SelectMenuExecuted += SelectMenuHandler;
            _client.ModalSubmitted += ModalHandler;

            _commands = new CommandService();
            _commands.Log += Log;

            await _client.LoginAsync(TokenType.Bot, reader.GetValue("token", typeof(string)).ToString());
            await _client.StartAsync();

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        private Task Log(LogMessage message)
        {
            if (message.Exception is CommandException cmdException)
            {
                Console.WriteLine($"[Command/{message.Severity}] {cmdException.Command.Aliases.First()}" + $" failed to execute in {cmdException.Context.Channel}.");
                Console.WriteLine(cmdException);
            }
            else { Console.WriteLine($"[General/{message.Severity}] {message}"); }

            return Task.CompletedTask;
        }
        public async Task Client_Ready()
        {
            //クライアント立ち上げ時の処理
            await Task.CompletedTask;
        }

        //
        // スラッシュコマンド
        private async Task SlashCommandHandler(SocketSlashCommand command)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    if(Settings.Shared.m_WolMachines.Count == 0)
                    {
                        await command.RespondAsync("コマンド実行に必用なデータが存在しません。");
                        return;
                    }
                    if(!command.GuildId.HasValue)
                    {
                        await command.RespondAsync("ギルド専用コマンドです。");
                        return;
                    }

                    ulong guildid = command.GuildId.Value;
                    string commandname = command.Data.Name;

                    string commandoption = string.Empty;
                    string message = string.Empty;
                    SelectMenuBuilder menuBuilder = new SelectMenuBuilder();
                    ComponentBuilder builder = new ComponentBuilder();

                    switch (command.CommandName)
                    {
                        case "powerctrl":
                            commandoption = command.Data.Options.First().Value.ToString()!;
                            message = $"[/{commandname}:{commandoption}]@(p.0)\n以下の選択肢からマシンを選択してください。";
                            menuBuilder = await SelectMenuEditor.CreateMachineMenu(0, commandoption);
                            builder = new ComponentBuilder().WithSelectMenu(menuBuilder);

                            await command.RespondAsync(message, components: builder.Build(), ephemeral: true);
                            break;

                        case "powersetting":
                            commandoption = command.Data.Options.First().Value.ToString()!;

                            if(commandoption == "reload")
                            {
                                Settings.Shared.ReloadMachineInfo();

                                await command.RespondAsync(text: "マシン一覧のリロードが完了しました。", ephemeral: true);
                            }
                            else
                            {
                                var machineName = new TextInputBuilder().WithLabel("MACHINE NAME").WithCustomId("NAME").WithStyle(TextInputStyle.Short).WithMinLength(1).WithRequired(true).WithPlaceholder("マシン名を入力");
                                var machineIPAddress = new TextInputBuilder().WithLabel("MACHINE IPADDRESS").WithCustomId("IPADDRESS").WithStyle(TextInputStyle.Short).WithMinLength(7).WithMaxLength(21).WithRequired(true).WithPlaceholder("マシンのIPAddressとPortを入力");
                                var machineMACAddress = new TextInputBuilder().WithLabel("MACHINE MACADDRESS").WithCustomId("MACADDRESS").WithStyle(TextInputStyle.Short).WithMinLength(17).WithMaxLength(17).WithRequired(true).WithPlaceholder("マシンのMACアドレスを入力");
                                var shutdownPort = new TextInputBuilder().WithLabel("WOL PORT").WithCustomId("WOL_PORT").WithStyle(TextInputStyle.Short).WithMinLength(1).WithMaxLength(5).WithValue("12000").WithRequired(true).WithPlaceholder("マシンのシャットダウン用ポートを入力");
                                var shutdownMessage = new TextInputBuilder().WithLabel("SHUTDOWM MESSAGE").WithCustomId("MESSAGE").WithStyle(TextInputStyle.Paragraph).WithMinLength(1).WithMaxLength(64).WithValue("power_off_signal").WithRequired(true).WithPlaceholder("マシンのシャットダウン用メッセージを入力");
                                var modalbuilder = new ModalBuilder().WithTitle("MACHINE SETUP").WithCustomId("MACHINE_SETUP").AddTextInput(machineName).AddTextInput(machineIPAddress).AddTextInput(machineMACAddress).AddTextInput(shutdownPort).AddTextInput(shutdownMessage);

                                await command.RespondWithModalAsync(modalbuilder.Build());
                            }
                            break;

                        default:
                            await command.RespondAsync(text: $"存在しないコマンド名[{command.CommandName}]が指定されました。");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    if (command.HasResponded)
                    {
                        await command.ModifyOriginalResponseAsync(m => { m.Content = ex.Message; });
                        return;
                    }
                    await command.RespondAsync(ex.Message);
                }
            });

            await Task.CompletedTask;
        }

        //
        // セレクトメニューのイベント処理
        private static async Task SelectMenuHandler(SocketMessageComponent arg)
        {
            _ = Task.Run(async() =>
            {
                try
                {
                    if (arg.GuildId == null)
                    {
                        await arg.RespondAsync("不明なコマンドが実行されました。");
                        return;
                    }
                    ulong guildid = arg.GuildId.Value;

                    string[] CustomID = arg.Data.CustomId.Split(':');          // コマンド名[.機能名] : [エンジン名] : [話者名] : コマンドモード
                    string[] CustomValue = arg.Data.Values.First().Split('@'); // 内部コマンド名 @ コマンド値

                    string commandName = CustomID.First();
                    string CommandMode = CustomID.Last();
                    string InnerCommandName = CustomValue.First();
                    string InnerCommandValue = CustomValue.Last();

                    switch (CommandMode)
                    {
                        case "wol":
                            WolTerminal.SendMagicPacket(Settings.Shared.m_WolMachines[InnerCommandValue]);
                            break;
                        case "stdm":
                            StdmTerminal.SendStdmMessage(Settings.Shared.m_WolMachines[InnerCommandValue]);
                            break;
                        default:
                            return;
                    }

                    await arg.RespondAsync("Packet Send!!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    if (arg.HasResponded)
                    {
                        await arg.ModifyOriginalResponseAsync(m => { m.Content = ex.Message; });
                        return;
                    }
                    await arg.RespondAsync(ex.Message);
                }
            });
            await Task.CompletedTask;
        }

        //
        // モーダルのイベント処理
        private static async Task ModalHandler(SocketModal modal)
        {
            _ = Task.Run(async() =>
            {
                if (modal.GuildId == null)
                {
                    await modal.RespondAsync("不正なコマンドが実行されました。");
                    return;
                }
                await modal.RespondAsync("PROCESSING...");

                try
                {
                    List<SocketMessageComponentData> components = modal.Data.Components.ToList();
                    var CustomID = modal.Data.CustomId;

                    Console.WriteLine(CustomID);

                    if (CustomID == "MACHINE_SETUP")
                    {
                        MachineInfo machineInfo = new MachineInfo();
                        WolInfo wolInfo = new WolInfo();
                        StdmInfo stdmInfo = new StdmInfo();
                        int wolPort = 12000;
                        int stdmPort = 23000;

                        // stdmInfoを初期化する
                        string[] str_ipaddress = components.First(_ => _.CustomId == "IPADDRESS").Value.Split(':');
                        string str_message = components.First(_ => _.CustomId == "MESSAGE").Value;
                        string[] str_ipPart = str_ipaddress[0].Split('.');
                        string str_stdmPort = str_ipaddress.Length == 2 ? str_ipaddress[1] : "23000";
                        if (int.TryParse(str_stdmPort, out wolPort) && !Tools.IsPortNumber(stdmPort))
                        {
                            stdmPort = 23000;
                        }
                        stdmInfo.IPAddress = str_ipaddress[0];
                        stdmInfo.Port = stdmPort;
                        stdmInfo.message = str_message;

                        // wolInfoを初期化する
                        string str_macaddress = components.First(_ => _.CustomId == "MACADDRESS").Value;
                        string str_wolPort = components.First(_ => _.CustomId == "WOL_PORT").Value;
                        if (!int.TryParse(str_wolPort, out wolPort) && Tools.IsPortNumber(wolPort))
                        {
                            wolPort = 12000;
                        }
                        wolInfo.NewtworkIP = $"{str_ipPart[0]}.{str_ipPart[1]}.{str_ipPart[2]}.255";
                        wolInfo.Port = wolPort;
                        wolInfo.MACAddress = str_macaddress;

                        // machineInfoを初期化する
                        string str_name = components.First(_ => _.CustomId == "NAME").Value;
                        machineInfo.Name = str_name;
                        machineInfo.wolInfo = wolInfo;
                        machineInfo.stdmInfo = stdmInfo;

                        Settings.Shared.SetEditMachineInfo(machineInfo);

                        await modal.ModifyOriginalResponseAsync(m => m.Content = "変更を行いました。");
                    }
                }catch(Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    if (modal.HasResponded)
                    {
                        await modal.ModifyOriginalResponseAsync(m => { m.Content = ex.Message; });
                        return;
                    }
                    await modal.RespondAsync(ex.Message);
                }
            });

            await Task.CompletedTask;
        }
    }
}