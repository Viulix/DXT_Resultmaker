using Discord;
using Discord.Interactions;

namespace DXT_Resultmaker.Modules;

// Interaction modules must be public and inherit from an IInteractionModuleBase
public class Resultmodule : InteractionModuleBase<SocketInteractionContext>
{

    
    [SlashCommand("results", "Returns a formatted result message of current week.")]
    public async Task Results([Choice("1", "1"), Choice("2", "2"), Choice("3", "3"), Choice("4", "4"), Choice("5", "5"), Choice("6", "6"), Choice("7", "7"), Choice("8", "8"), Choice("9", "9")] string week = "")
    {
        await RespondAsync("This command is deprecated and will be removed in the future. Please use `/fixtures` instead to get the results of the current week.", ephemeral: true);

    }
    
    [SlashCommand("roster", "Displays current roster of the franchise and tier you asked for.")]
    public async Task Roster([Choice("Master", (int)Tiers.Master), Choice("Elite", (int)Tiers.Elite), Choice("Rival", (int)Tiers.Rival),
            Choice("Challenger", (int)Tiers.Challenger), Choice("Prospect", (int)Tiers.Prospect), Choice("Academy", (int)Tiers.Academy)] int tier, [Autocomplete(typeof(AutoCompleteHandlerBase))] string franchiseName = HelperFactory.defaultFranchise)
    {
        await DeferAsync();
        try
        {
            var client = new ApiClient(HelperFactory.defaultAPIUrl);
            FranchiseTeam franchiseTeam = await client.GetFranchiseTeamAsync(franchiseName, tier);
            FranchiseData franchise = await client.GetFranchiseByNameAsync(franchiseName);
            if (franchiseTeam is null || franchise is null)
            {
                await FollowupAsync("Did not find a team for this franchise.");
                return;
            }
            var msg = String.Join("\n", franchiseTeam.Players.Select(x => x.User.Name + $" `{x.Cmv}`"));
            string field1 = "";
            string field2 = "";
            string field3 = "";
            foreach (var player in franchiseTeam.Players)
            {
                if (player.Captain == true)
                {
                    field1 += "C\n";
                }
                field2 += $"`{player.User.Name}`\n";
                if(player.Cmv is not null) field3 += $"`{player.Cmv}`\n";
                else field3 += $"`N/V`\n";
            }
            var titel = franchiseTeam.Team.Name;

            var userName = Context.Guild.Users.Where(x => x.Id == Context.User.Id).FirstOrDefault().Nickname;
            userName ??= Context.User.GlobalName;

            var emb = new EmbedBuilder()
                .WithColor(HelperFactory.GetTierColor(tier))
                .WithTitle(HelperFactory.MakeDiscordEmoteString(HelperFactory.tiers[tier - 36], HelperFactory.emote_guild, true) + " " + franchiseName + $" - \"{titel}\"\n")
                .WithDescription("*Team Overview*");
            if(field1 != "")
            {
                emb.AddField("Captain", field1, true);
            }
            emb.AddField("Player", field2, true)
               .AddField("CMV", field3, true)
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
