using Google.Apis.Auth.OAuth2;


namespace DXT_Resultmaker
{
    public static class SheetHandler
    {
        public static SheetManager manager;
        readonly static string clientId = "883967830518-30n372t554dtr09bo1tiedukbnj778v1.apps.googleusercontent.com";
        readonly static string clientSecret = "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";
        readonly static string[] scopes = new[] { Google.Apis.Sheets.v4.SheetsService.Scope.Spreadsheets };
        public static string ERS_SHEET_URL = "1W3XQzMjh7npdcuDnOQsyE3Ol_6I8RYXnkMS2lsXzTbk";
        public static string DXT_SHEET_URL = "1EWZW7TIFI2nbzgmHJWVIh0Znx_6xtJCFmuA60VK1iRw";
    }
}
