namespace vault
{
    public static class Icons
    {
        public static string GetIconPath(int gameMode)
        {
            return gameMode switch
            {
                1 => "/images/mode-icons/taiko.svg",
                2 => "/images/mode-icons/ctb.svg",
                3 => "/images/mode-icons/mania.svg",
                _ => "/images/mode-icons/osu.svg"
            };
        }
    }
}