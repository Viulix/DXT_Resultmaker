using BallchasingSharp;

namespace DXT_Resultmaker
{
    public class StatsFactory
    {
        private static readonly RequestManager requestManager = new("8NxwPAsYPkSowjP0dUSKhGYzC1XrR50sZMUomp32");
        public static DexterityPlayerList userPlattformIds;
        public static string stats_range = "Season Player Stats!";
        public static string link_range = "Per Week!";

        public static List<List<string>> ProcessReplay(string replayId, string[] player_plattformIds)
        {
            List<List<string>> userStats = new List<List<string>>();
            BallchasingSharp.Replay replay = requestManager.GetReplay(replayId).Result;
            BallchasingSharp.Player player;
            for (int i = 0; i < replay.BlueTeam.Players.Count && i < replay.OrangeTeam.Players.Count; i++)
            {

                if (player_plattformIds.Contains(replay.BlueTeam.Players[i].PlatformId))
                {
                    player = replay.BlueTeam.Players[i];
                }
                else if (player_plattformIds.Contains(replay.OrangeTeam.Players[i].PlatformId))
                {
                    player = replay.OrangeTeam.Players[i];
                }
                else
                {
                    continue;
                }
                List<string> data = new();
                data.Add(player.PlatformId);
                data.Add(player.Name);
                data.Add(player.Stats.Games.ToString());
                data.Add(player.Stats.Wins.ToString());
                data.Add(player.Stats.Core.Score.ToString());
                data.Add(player.Stats.Core.Shots.ToString());
                data.Add(player.Stats.Core.Goals.ToString());
                data.Add(player.Stats.Core.ShootingPercentage.ToString());
                data.Add(player.Stats.Core.ShotsAgainst.ToString());
                data.Add(player.Stats.Core.GoalsAgainst.ToString());
                data.Add(player.Stats.Core.Assists.ToString());
                data.Add(player.Stats.Core.Saves.ToString());
                data.Add(player.Stats.Demo.Inflicted.ToString());
                data.Add(player.Stats.Demo.Taken.ToString());
                data.Add(player.Stats.Boost.BPM.ToString());
                data.Add(player.Stats.Boost.BCPM.ToString());
                data.Add(player.Stats.Boost.AverageAmount.ToString());
                data.Add(player.Stats.Boost.TimeZeroBoost.ToString());
                data.Add(player.Stats.Boost.TimeFullBoost.ToString());
                data.Add(player.Stats.Boost.TimeBoost_0_25.ToString());
                data.Add(player.Stats.Boost.TimeBoost_25_50.ToString());
                data.Add(player.Stats.Boost.TimeBoost_50_75.ToString());
                data.Add(player.Stats.Boost.TimeBoost_75_100.ToString());
                data.Add(player.Stats.Boost.CountCollectedBig.ToString());
                data.Add(player.Stats.Boost.CountCollectedSmall.ToString());
                data.Add(player.Stats.Boost.CountStolenBig.ToString());
                data.Add(player.Stats.Boost.CountStolenSmall.ToString());
                data.Add(player.Stats.Boost.AmountUsedWhileSupersonic.ToString());
                data.Add(player.Stats.Boost.AmountOverfill.ToString());
                data.Add(player.Stats.Boost.AmountOverfillStolen.ToString());
                data.Add(player.Stats.Positioning.PercentDefensiveThird.ToString());
                data.Add(player.Stats.Positioning.PercentNeutralThird.ToString());
                data.Add(player.Stats.Positioning.PercentOffensiveThird.ToString());
                data.Add(player.Stats.Positioning.PercentDefensiveHalf.ToString());
                data.Add(player.Stats.Positioning.PercentOffensiveHalf.ToString());
                data.Add(player.Stats.Positioning.PercentBehindBall.ToString());
                data.Add(player.Stats.Positioning.PercentInfrontBall.ToString());
                data.Add(player.Stats.Positioning.TimeMostBack.ToString());
                data.Add(player.Stats.Positioning.TimeMostForward.ToString());
                data.Add(player.Stats.Positioning.AverageDistanceToBall.ToString());
                data.Add(player.Stats.Movement.AverageSpeed.ToString());
                data.Add(player.Stats.Movement.PercentSlowSpeed.ToString());
                data.Add(player.Stats.Movement.PercentBoostSpeed.ToString());
                data.Add(player.Stats.Movement.PercentSupersonicSpeed.ToString());
                data.Add(player.Stats.Movement.PercentGround.ToString());
                data.Add(player.Stats.Movement.PercentLowAir.ToString());
                data.Add(player.Stats.Movement.PercentHighAir.ToString());
                data.Add(player.Stats.Movement.CountPowerslide.ToString());
                data.Add(player.Stats.Movement.TimePowerslide.ToString());
                userStats.Add(data);

            }
            return userStats;
        }
        public static List<List<string>> ProcessReplayGroup(string groupId, string[] player_plattformIds)
        {
            List<List<string>> userStats = new List<List<string>>();
            BallchasingSharp.ReplayGroup replay = requestManager.GetReplayGroup(groupId).Result;
            
            BallchasingSharp.Player player;
            bool userAlreadyProcessed = false;
            for (int i = 0; i < replay.Players.Count; i++)
            {
                

                if (!player_plattformIds.Contains(replay.Players[i].PlatformId)) continue;
                
                player = replay.Players[i];
                for(int k = 0; k < userStats.Count; k++)
                {
                    if (userStats[k].Contains(player.PlatformId)) userAlreadyProcessed = true;
                }
                if(!userAlreadyProcessed)
                {

                    List<string> data = new();
                    data.Add(player.PlatformId);
                    data.Add(player.Name);
                    data.Add(player.CumulativeGroupStats.Games.ToString());
                    data.Add(player.CumulativeGroupStats.Wins.ToString());
                    data.Add(player.GameAverageGroupStats.Core.Score.ToString());
                    data.Add(player.GameAverageGroupStats.Core.Shots.ToString());
                    data.Add(player.GameAverageGroupStats.Core.Goals.ToString());
                    data.Add(player.GameAverageGroupStats.Core.ShootingPercentage.ToString());
                    data.Add(player.GameAverageGroupStats.Core.ShotsAgainst.ToString());
                    data.Add(player.GameAverageGroupStats.Core.GoalsAgainst.ToString());
                    data.Add(player.GameAverageGroupStats.Core.Assists.ToString());
                    data.Add(player.GameAverageGroupStats.Core.Saves.ToString());
                    data.Add(player.GameAverageGroupStats.Demo.Inflicted.ToString());
                    data.Add(player.GameAverageGroupStats.Demo.Taken.ToString());
                    data.Add(player.GameAverageGroupStats.Boost.BPM.ToString());
                    data.Add(player.GameAverageGroupStats.Boost.BCPM.ToString());
                    data.Add(player.GameAverageGroupStats.Boost.AverageAmount.ToString());
                    data.Add(player.GameAverageGroupStats.Boost.TimeZeroBoost.ToString());
                    data.Add(player.GameAverageGroupStats.Boost.TimeFullBoost.ToString());
                    data.Add(player.GameAverageGroupStats.Boost.TimeBoost_0_25.ToString());
                    data.Add(player.GameAverageGroupStats.Boost.TimeBoost_25_50.ToString());
                    data.Add(player.GameAverageGroupStats.Boost.TimeBoost_50_75.ToString());
                    data.Add(player.GameAverageGroupStats.Boost.TimeBoost_75_100.ToString());
                    data.Add(player.GameAverageGroupStats.Boost.CountCollectedBig.ToString());
                    data.Add(player.GameAverageGroupStats.Boost.CountCollectedSmall.ToString());
                    data.Add(player.GameAverageGroupStats.Boost.CountStolenBig.ToString());
                    data.Add(player.GameAverageGroupStats.Boost.CountStolenSmall.ToString());
                    data.Add(player.GameAverageGroupStats.Boost.AmountUsedWhileSupersonic.ToString());
                    data.Add(player.GameAverageGroupStats.Boost.AmountOverfill.ToString());
                    data.Add(player.GameAverageGroupStats.Boost.AmountOverfillStolen.ToString());
                    data.Add(player.GameAverageGroupStats.Positioning.PercentDefensiveThird.ToString());
                    data.Add(player.GameAverageGroupStats.Positioning.PercentNeutralThird.ToString());
                    data.Add(player.GameAverageGroupStats.Positioning.PercentOffensiveThird.ToString());
                    data.Add(player.GameAverageGroupStats.Positioning.PercentDefensiveHalf.ToString());
                    data.Add(player.GameAverageGroupStats.Positioning.PercentOffensiveHalf.ToString());
                    data.Add(player.GameAverageGroupStats.Positioning.PercentBehindBall.ToString());
                    data.Add(player.GameAverageGroupStats.Positioning.PercentInfrontBall.ToString());
                    data.Add(player.GameAverageGroupStats.Positioning.TimeMostBack.ToString());
                    data.Add(player.GameAverageGroupStats.Positioning.TimeMostForward.ToString());
                    data.Add(player.GameAverageGroupStats.Positioning.AverageDistanceToBall.ToString());
                    data.Add(player.GameAverageGroupStats.Movement.AverageSpeed.ToString());
                    data.Add(player.GameAverageGroupStats.Movement.PercentSlowSpeed.ToString());
                    data.Add(player.GameAverageGroupStats.Movement.PercentBoostSpeed.ToString());
                    data.Add(player.GameAverageGroupStats.Movement.PercentSupersonicSpeed.ToString());
                    data.Add(player.GameAverageGroupStats.Movement.PercentGround.ToString());
                    data.Add(player.GameAverageGroupStats.Movement.PercentLowAir.ToString());
                    data.Add(player.GameAverageGroupStats.Movement.PercentHighAir.ToString());
                    data.Add(player.GameAverageGroupStats.Movement.CountPowerslide.ToString());
                    data.Add(player.GameAverageGroupStats.Movement.TimePowerslide.ToString());
                    userStats.Add(data);
                }
                else
                {
                    userAlreadyProcessed = false;
                }
            }
            return userStats;
        }
        public static void UpdateUserSpreadSheetData(List<List<string>> userData, string[] player_plattformIds, int week)
        {
            var manager = SheetHandler.manager;
            var currentData = manager.GetAllSpreadsheetValuesColumn(SheetHandler.DXT_SHEET_URL, stats_range + (3+(week-1)*50).ToString() + ":" + (3 + week * 50).ToString()).Result;
            currentData.RemoveAt(0);
            var merged = MergePlayerData(userData, currentData);
            foreach (var player in merged)
            {
                player.RemoveAll(x => x == "N/V");
            }

            string[][] stringArray = merged
                    .Select(innerList => innerList.Select(item => item.ToString()).ToArray()).ToArray();

            manager.WriteToSpreadsheetColumn(SheetHandler.DXT_SHEET_URL, stats_range + "B" + (3 + (week - 1) * 50).ToString() + ":" + (3 + week * 50).ToString(), stringArray);
        }
        public static void UpdateUserSpreadSheetDataNoMerge(List<List<string>> userData, string[] player_plattformIds, int week)
        {
            var manager = SheetHandler.manager;
            var currentData = manager.GetAllSpreadsheetValuesColumn(SheetHandler.DXT_SHEET_URL, stats_range + (3 + (week - 1) * 50).ToString() + ":" + (3 + week * 50).ToString()).Result;
            currentData.RemoveAt(0);
            var merged = MergePlayerDataReplace(userData, currentData);
            foreach (var player in merged)
            {
                player.RemoveAll(x => x == "N/V");
            }

            string[][] stringArray = merged
                    .Select(innerList => innerList.Select(item => item.ToString()).ToArray()).ToArray();

            manager.WriteToSpreadsheetColumn(SheetHandler.DXT_SHEET_URL, stats_range + "B" + (3 + (week - 1) * 50).ToString() + ":" + (3 + week * 50).ToString(), stringArray);
        }
        public static List<List<string>> MergePlayerData(List<List<string>> userData, List<List<string>> currentData)
        {
            List<List<string>> mergedData = new List<List<string>>();

            Dictionary<string, List<string>> currentUserDataDict = currentData.ToDictionary(player => player[0]);

            foreach (var player in userData)
            {
                string playerId = player[0];
                if (currentUserDataDict.ContainsKey(playerId))
                {
                    List<string> mergedPlayer = new List<string>(player);
                    mergedPlayer = CombineData(player, currentUserDataDict[playerId]);
                    mergedData.Add(mergedPlayer);
                    currentUserDataDict.Remove(playerId);
                }
                else
                {
                    mergedData.Add(player);
                }
            }

            foreach (var player in currentUserDataDict.Values)
            {
                mergedData.Add(player);
            }

            return mergedData;
        }
        public static List<List<string>> MergePlayerDataReplace(List<List<string>> userData, List<List<string>> currentData)
        {
            List<List<string>> mergedData = new List<List<string>>();

            Dictionary<string, List<string>> currentUserDataDict = currentData.ToDictionary(player => player[0]);

            foreach (var player in userData)
            {
                string playerId = player[0];
                if (currentUserDataDict.ContainsKey(playerId))
                {
                    mergedData.Add(player);
                    currentUserDataDict.Remove(playerId);
                }
                else
                {
                    mergedData.Add(player);
                }
            }

            foreach (var player in currentUserDataDict.Values)
            {
                mergedData.Add(player);
            }

            return mergedData;
        }
        public static List<string> CombineData(List<string> data1, List<string> data2) {
            var result = new List<string>();
            result.Add(data2[0]);
            result.Add(data2[1]);
            int limit = Math.Min(data1.Count, data2.Count);
            if(!(limit > 4))
            {
                List<string> L = new();
                return L;
            }
            for (int i = 2; i < limit; i++)
            {
                try
                {
                    double value1 = 0;
                    double value2 = 0;

                    if (i < data1.Count && !string.IsNullOrEmpty(data1[i]))
                        value1 = Convert.ToDouble(data1[i]);

                    if (i < data2.Count && !string.IsNullOrEmpty(data2[i]))
                        value2 = Convert.ToDouble(data2[i]);

                    if(i < 4)
                    {
                        result.Add(((value1 + value2)).ToString());
                        continue;
                    }
                    result.Add(((value1 + value2)/2).ToString());
                }
                catch (Exception)
                {
                    continue;
                }
            }
            return result;
        }
        public static async void WeeklyStatsUpdate(int week, string[] player_plattformIds)
        {
            var spreadSheetLinks = SheetHandler.manager.GetAllSpreadsheetValuesColumn(SheetHandler.DXT_SHEET_URL, link_range + "A2:I11").Result;
            if(spreadSheetLinks.Count < week) {
                return;
            }
            List<string> allLinks = spreadSheetLinks[week - 1];
            List<List<string>>? userData1 = null;
            List<List<string>>? allData = null;
            for(int i = 0; i < allLinks.Count; i++)
            {
                if (allLinks[i].StartsWith("https://ballchasing.com/group/")) { 
                    int index1 = allLinks[i].IndexOf("/group/") + "/group/".Length;
                    string ballchasingId1 = allLinks[i].Substring(index1);
                    ballchasingId1 = ballchasingId1.Split("/").First();

                    userData1 = ProcessReplayGroup(ballchasingId1, player_plattformIds);
                    if(allData != null)
                    {
                        allData = MergePlayerData(userData1, allData);
                    }
                    else
                    {
                        allData = userData1;
                    }
                }

            }
            if(allData != null)
            {
                string[][] stringArray = allData
                    .Select(innerList => innerList.Select(item => item.ToString()).ToArray()).ToArray();
                SheetHandler.manager.WriteToSpreadsheetColumn(SheetHandler.DXT_SHEET_URL, link_range + "A" + ((week - 1)* 51 + 20), stringArray);
            }

        }
        public static List<string> GetWeeklyStats(int week, string playerId)
        {
            var allData = SheetHandler.manager.GetAllSpreadsheetValuesColumn(SheetHandler.DXT_SHEET_URL, link_range + ((week - 1) * 51 + 20) + ":" + (week * 51 + 18)).Result;
            List<string> userStats = new List<string>();
            for (int i = 0; i< allData.Count; i++)
            {
                if (allData[i].Count > 0)
                {
                    if(allData[i][0] == playerId)
                    {
                        return allData[i];
                    }
                }
            }
            return userStats;
        }
    }
}
