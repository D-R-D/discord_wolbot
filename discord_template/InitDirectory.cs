namespace discord_template
{
    internal class InitDirectory
    {
        public static void init()
        {
            checkdir("commands");
            checkdir("save");

            checkfile("save/machines.json");
        }

        private static void checkdir(string dirname)
        {
            if (!Directory.Exists($"{Directory.GetCurrentDirectory()}/{dirname}"))
            {
                Directory.CreateDirectory($"{Directory.GetCurrentDirectory()}/{dirname}");
            }
        }

        private static void checkfile(string filename)
        {
            if (!File.Exists($"{Directory.GetCurrentDirectory()}/{filename}"))
            {
                File.Create($"{Directory.GetCurrentDirectory()}/{filename}").Close();
            }
        }
    }
}
