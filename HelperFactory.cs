using Discord;
using System.Linq;

namespace DXT_Resultmaker
{
    public static class HelperFactory
    {
        public static string[] tiers = { "Master", "Elite", "Rival", "Challenger", "Prospect" };
        public static ulong emote_guild = 1093943074746531910;
        public static List<ulong> captains;
        public static List<Franchise> Franchises = new();
        public static List<string> playerIds;
        public static Discord.Color dxtColor = new(0x9ee345);
        public static string userDataPath = "./userData.json";
        public static string ers_ballchasing_id = "ers-2024-97crxbujmw";
        public static List<string> ballchasingGroupIds;
        public static DateTime seasonStart = new(2025, 2, 24);
        public static int seasonCalenderWeek = 8;
        public static string MakeDiscordEmoteString(string abbrevation, ulong guildId)
        {
            try
            {

                List<Discord.GuildEmote> emotes = CommandHandler._client.Guilds
                    .Where(x => x.Id == guildId || x.Id == 690948036540760147) // Alle passenden Guilds filtern
                    .SelectMany(g => g.Emotes) // Alle Emotes aus den gefundenen Guilds zusammenfügen
                    .ToList(); // In eine Liste umwandeln


                string msg = "N/V";
            foreach (var emote in emotes)
            {
                if (emote.Name == abbrevation)
                {
                    msg = "<:" + abbrevation + ":" + emote.Id + ">";
                    break;
                }
            }
            return msg;
            }
            catch  (Exception ex)
            {
                Console.WriteLine(ex);
                return "N/V";
            }
        }
        public static string MakeDiscordEmoteUrl(string abbrevation, ulong guildId)
        {
            try
            {

                List<Discord.GuildEmote> emotes = CommandHandler._client.Guilds
                    .Where(x => x.Id == guildId || x.Id == 690948036540760147) // Alle passenden Guilds filtern
                    .SelectMany(g => g.Emotes) // Alle Emotes aus den gefundenen Guilds zusammenfügen
                    .ToList(); // In eine Liste umwandeln


                string msg = "N/V";
                foreach (var emote in emotes)
                {
                    if (emote.Name == abbrevation)
                    {
                        msg = emote.Url;
                        break;
                    }
                }
                return msg;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return "N/V";
            }
        }
        public static List<string> SplitAndSortResultString(string raw_result) { 
            var result = new List<string>();
            var splitted_raw_Result = raw_result.Split(':').ToList();
            splitted_raw_Result = splitted_raw_Result.Where((item, index) => index % 2 != 0).ToList();
            return splitted_raw_Result;
        }
        public static Discord.Color GetTierColor(int tier)
        {
            Discord.Color color = new Discord.Color(000000);

            switch (tier)
            {
                case 0:
                    color = new Discord.Color(0x6ed1ff);
                        break;
                case 1:
                    color = new Discord.Color(0x2ccc6f);
                    break;
                case 2:
                    color = new Discord.Color(0xf1c40d);
                    break;
                case 3:
                    color = new Discord.Color(0xe67c20);
                    break;
                case 4:
                    color = new Discord.Color(0xe06564);
                    break;
            }
            return color;
        }
        public static Discord.EmbedBuilder DefaultEmbed(Discord.IUser author)
        {
            var embed = new EmbedBuilder() { }
                .WithCurrentTimestamp()
                .WithColor(dxtColor);
            return embed;
                
        }
    }
}
