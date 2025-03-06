namespace DXT_Resultmaker
{
    public class SaveData
    {
        public string token { get; set; }
        public List<ulong> admins { get; set; }
        public string database_sheet { get; set; }
        public string dxt_sheet { get; set; }
        public List<Franchise> franchises { get; set; }
    }
    public class Franchise
    {
        public string name { get; set; }
        public List<Subteam> subteams { get; set; }
        public string manager { get; set; }
        public List<string> assistentManagers { get; set; }
        public string rank { get; set; }
        public string abbreviation { get; set; }
        public string logo_url { get; set; }
        public string bannerurl{ get; set; }
        public string emoteUrl { get; set; }
        public string main_color { get; set; }

    }
    public class Subteam
    {
        public string name { get; set; }
        public List<string> players { get; set; }
        public string captain { get; set; }
    }
}
