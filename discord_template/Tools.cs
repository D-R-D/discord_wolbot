namespace discord_template
{
    public static class Tools
    {
        public static bool IsNullOrEmpty(this string? str)
        {
            if (str == null) { return true; }
            if (str == "") { return true; }
            return false;
        }

        public static bool IsPortNumber(int? port)
        {
            if (port == null) { return false; }
            if ((port <= 0) || (port > 65535)) { return false; }
            return true;
        }
    }
}
