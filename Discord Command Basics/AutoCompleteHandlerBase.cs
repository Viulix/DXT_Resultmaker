using Discord;
using Discord.Interactions;


namespace DXT_Resultmaker
{
    public class AutoCompleteHandlerBase : AutocompleteHandler
    {

        public override Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            try
            {
                var value = autocompleteInteraction.Data.Current.Value as string;

                if (string.IsNullOrEmpty(value))
                    return Task.FromResult(AutocompletionResult.FromSuccess());
                var matches = HelperFactory.SaveData.Franchises
                                            .Select(y => y.Name)
                                            .Where(x => x.Contains(value, StringComparison.OrdinalIgnoreCase)) 
                                            .Take(25) 
                                            .ToList();
                return Task.FromResult(AutocompletionResult.FromSuccess(matches.Select(x => new AutocompleteResult(x, x))));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Task.FromResult(AutocompletionResult.FromError(ex));
            }
        }
    }
}