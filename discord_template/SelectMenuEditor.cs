using Discord;
using discord_template;

namespace discord_wol
{
    internal class SelectMenuEditor
    {
        public static async Task<SelectMenuBuilder> CreateMachineMenu(int page, string CommandMode)
        {
            SelectMenuBuilder builder = new SelectMenuBuilder().WithPlaceholder($"Machine List p.{page}").WithCustomId($"machine:{CommandMode}").WithMinValues(1).WithMaxValues(1);

            await Task.Run(() =>
            {
                var machines = Settings.Shared.m_WolMachines.Keys.Skip(16 * page).Take(16).ToArray();
                if (page > 0)
                {
                    builder.AddOption("Previous page.", $"page@{page - 1}", $"Go to page {(page - 1)}.");
                }

                foreach (var machine in machines)
                {
                    builder.AddOption(machine, $"machine@{machine}");
                }

                if (Settings.Shared.m_WolMachines.Keys.ToArray().Count() > (16 * (page + 1)))
                {
                    builder.AddOption("Next page.", $"page@{page + 1}", $"Go to page {page + 1}.");
                }
            });

            return builder;
        }
    }
}
