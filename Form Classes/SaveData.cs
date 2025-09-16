namespace DXT_Resultmaker
{
    public class SaveData
    {
        public List<ulong> Admins { get; set; } = new();
        public List<Franchise> Franchises { get; set; } = new();
        public List<ulong> ChannelIds { get; set; } = new();
        public List<ulong> GuildIds { get; set; } = new();
        public List<ulong> RoleIds { get; set; } = new();
        public ulong EmoteGuild { get; set; } = new();
        public string DefaultFranchise { get; set; } = "";
        public string DefaultAPIUrl { get; set; } = "";
        public int SeasonCalenderWeek { get; set; } = 33;
        public DateTime SeasonStart { get; set; } = HelperFactory.GetGermanTime();
        public DateTime StartDate { get; set; } = new DateTime(25, 8, 25, 14, 0, 0);
        public uint MainColor { get; set; } = new Discord.Color(0x000000);
        public static Dictionary<int, uint> TierColors { get; set; } = HelperFactory.TierColors;
        public int UpdateInterval { get; set; } = 1;
        public int ReminderMinutes { get; set; } = 120;
        public int MatchTimeOffset { get; set; } = 0;
        public Dictionary<ulong, ulong> MessageIds { get; set; } = [];
        public int LastScheduleWeek { get; set; } = 1;
        public List<int> RelevantMatchesStored = [];
    }
}
