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

                var matches = HelperFactory.Franchises.Where(x => x.name.StartsWith(value, StringComparison.OrdinalIgnoreCase));
                return Task.FromResult(AutocompletionResult.FromSuccess(matches.Select(x => new AutocompleteResult(x.name, x.name))));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Task.FromResult(AutocompletionResult.FromError(ex));
            }
        }
        protected override string GetLogString(IInteractionContext context) => $"Accessing DB from {context.Guild}-{context.Channel}";
    }
}