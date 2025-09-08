using Discord;
using Discord.Interactions;

namespace DXT_Resultmaker.Modules;

public class Resultmodule : InteractionModuleBase<SocketInteractionContext>
{

    
    [SlashCommand("results", "Returns a formatted result message of current week.")]
    public async Task Results([Choice("1", "1"), Choice("2", "2"), Choice("3", "3"), Choice("4", "4"), Choice("5", "5"), Choice("6", "6"), Choice("7", "7"), Choice("8", "8"), Choice("9", "9")] string week = "")
    {
        await RespondAsync("This command is deprecated and will be removed in the future. Please use `/fixtures` instead to get the results of the current week.", ephemeral: true);

    }
    
    [SlashCommand("roster", "Displays current roster of the franchise and tier you asked for.")]
    public async Task Roster([Choice("Master", (int)TierId.Master), Choice("Elite", (int)TierId.Elite), Choice("Rival", (int)TierId.Rival),
            Choice("Challenger", (int)TierId.Challenger), Choice("Prospect", (int)TierId.Prospect), Choice("Academy", (int)TierId.Academy)] int tier, [Autocomplete(typeof(AutoCompleteHandlerBase))] string franchiseName = HelperFactory.DefaultFranchiseConst)
    {
        await DeferAsync();
        try
        {
            var client = new ApiClient(HelperFactory.DefaultAPIUrl);
            FranchiseTeam franchiseTeam = await client.GetFranchiseTeamAsync(franchiseName, tier);
            Franchise franchise = await client.GetFranchiseByNameAsync(franchiseName);
            if (franchiseTeam is null || franchise is null)
            {
                await FollowupAsync("Did not find a team for this franchise.");
                return;
            }

            var playerDetails = franchiseTeam.Players
                .Select(player => $"{(player.Captain == true ? "(C) " : "")}{player.User.Name} `{(player.Cmv.HasValue ? player.Cmv.Value.ToString() : "N/V")}`")
                .ToList();

            var playerFieldContent = string.Join("\n", playerDetails);
            var titel = franchiseTeam.Team.Name;

            var userName = Context.Guild.Users.Where(x => x.Id == Context.User.Id).FirstOrDefault()?.Nickname;
            userName ??= Context.User.GlobalName;

            var emb = new EmbedBuilder()
                .WithColor(HelperFactory.GetTierColor(tier))
                .WithTitle(HelperFactory.MakeDiscordEmoteString(HelperFactory.Tiers.Where(x => x.Value == tier).FirstOrDefault().Key, HelperFactory.SaveData.EmoteGuild, true) + " " + franchiseName + $" - \"{titel}\"\n")
                .WithDescription("*Team Overview*")
                .AddField("Players", playerFieldContent, false)
                .WithCurrentTimestamp()
                .WithThumbnailUrl(franchise.Logo)
                .WithFooter("Requested By " + userName, Context.User.GetAvatarUrl());

            await FollowupAsync(embed: emb.Build());
        }
        catch ( Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}
