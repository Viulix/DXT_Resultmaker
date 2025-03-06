using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Util.Store;


namespace DXT_Resultmaker
{
    public static class Authenticator
    {
        private static readonly string TokenFilePath = "token.json";


        public static UserCredential GetUserCredential(string googleClientId, string googleSecret, string[] scopes)
        {
            ClientSecrets secrets = new ClientSecrets()
            {
                ClientId = googleClientId,
                ClientSecret = googleSecret
            };

            // Erzeuge neuen UserCredential, um das Google-Konto zu autorisieren
            return GoogleWebAuthorizationBroker.AuthorizeAsync(secrets, scopes, "user", CancellationToken.None, new FileDataStore(TokenFilePath)).Result;
        }
    }
}
