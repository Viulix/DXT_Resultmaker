using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;

namespace DXT_Resultmaker
{

    public class SheetManager : SheetInterface
    {

        private readonly SheetsService service;

        //public SheetManager(string googleClientId, string googleSecret, string[] scopes)
        //{
        //    if (string.IsNullOrEmpty(googleClientId))
        //        throw new ArgumentNullException(nameof(googleClientId));

        //    if (string.IsNullOrEmpty(googleSecret))
        //        throw new ArgumentNullException(nameof(googleSecret));

        //    if (scopes == null || scopes.Length == 0)
        //        throw new ArgumentException("Scopes must not be null or empty");

        //    // Erzeuge das UserCredential-Objekt
        //    UserCredential credential = Authenticator.GetUserCredential(googleClientId, googleSecret, scopes);

        //    // Initialisiere den SheetsService
        //    service = new SheetsService(new BaseClientService.Initializer()
        //    {
        //        HttpClientInitializer = credential
        //    });
        //}
        public SheetManager(string serviceAccountCredentialsFilePath, string[] scopes)
        {
            if (string.IsNullOrEmpty(serviceAccountCredentialsFilePath))
                throw new ArgumentNullException(nameof(serviceAccountCredentialsFilePath));

            if (scopes == null || scopes.Length == 0)
                throw new ArgumentException("Scopes must not be null or empty");

            // Lade das Service Account-Credential aus der JSON-Datei
            GoogleCredential credential;
            using (var stream = new FileStream(serviceAccountCredentialsFilePath, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(scopes);
            }

            // Initialisiere den SheetsService
            service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential
            });
        }

        public Spreadsheet CreateNew(string sheetName)
        {
            if (string.IsNullOrEmpty(sheetName))
                throw new ArgumentNullException(nameof(sheetName));

            var sheetCreationRequest = service.Spreadsheets.Create(body: new Spreadsheet()
            {
                Sheets = new List<Sheet>()
            {
                new Sheet()
                {
                    Properties = new SheetProperties()
                    {
                        Title = sheetName
                    }
                }
            },
                Properties = new SpreadsheetProperties() { Title = sheetName }
            });
            return sheetCreationRequest.Execute();
        }
    
        public Spreadsheet GetSpreadsheet(string sheetId) {
            if (string.IsNullOrEmpty(sheetId))
                throw new ArgumentNullException(nameof(sheetId));
            {
                
                var sheetCreationRequest = service.Spreadsheets.Get(sheetId);

                return sheetCreationRequest.Execute();
                
            }
        }
        public async Task<List<List<string>>> GetAllSpreadsheetValues(string sheetId, string range)
        {
            if (string.IsNullOrEmpty(sheetId))
                throw new ArgumentNullException(nameof(sheetId));
            ValueRange response;

            var request = service.Spreadsheets.Values.Get(sheetId, range);
            // Anfrage zum Abrufen der Werte aus der Tabelle erstellen
            response = await request.ExecuteAsync();
            
            // Werte aus der Tabelle abrufen
            IList<IList<object>> values = response.Values;

            // Liste zum Speichern der Tabellendaten erstellen
            List<List<string>> spreadsheetData = new List<List<string>>();

            // Überprüfen, ob Daten vorhanden sind
            if (values != null && values.Count > 0)
            {
                // Daten in die Liste speichern
                foreach (var row in values)
                {
                    List<string> rowData = new List<string>();
                    foreach (var col in row)
                    {
                        rowData.Add(col?.ToString() ?? "0"); // Wenn "null", füge "0" hinzu
                    }
                    spreadsheetData.Add(rowData);
                }
            }

            return spreadsheetData;
        }
        public async Task<List<List<string>>> GetAllSpreadsheetValuesColumn(string sheetId, string range)
        {
            if (string.IsNullOrEmpty(sheetId))
                throw new ArgumentNullException(nameof(sheetId));
            ValueRange response;
            
            var request = service.Spreadsheets.Values.Get(sheetId, range);
            // Anfrage zum Abrufen der Werte aus der Tabelle erstellen
            response = await request.ExecuteAsync();
            
            // Werte aus der Tabelle abrufen
            IList<IList<object>> values = response.Values;

            // Liste zum Speichern der Tabellendaten erstellen
            List<List<string>> spreadsheetData = new List<List<string>>();

            // Überprüfen, ob Daten vorhanden sind
            if (values != null && values.Count > 0)
            {
                // Bestimme die maximale Anzahl von Spalten über alle Zeilen hinweg
                int numCols = values.Max(row => row.Count);

                // Iteriere über die Spalten
                for (int j = 0; j < numCols; j++)
                {
                    List<string> colData = new List<string>();

                    // Iteriere über die Zeilen und füge die Daten spaltenweise hinzu
                    foreach (var row in values)
                    {
                        if (j < row.Count && row[j] != null)
                            colData.Add(row[j].ToString());
                        else
                            colData.Add("N/V"); // Wenn "null", füge "0" hinzu
                    }
                    spreadsheetData.Add(colData);
                }
            }

            return spreadsheetData;
        }
        public string GetSpreadsheetData(string sheetId, string range)
        {
            if (string.IsNullOrEmpty(sheetId))
                throw new ArgumentNullException(nameof(sheetId));
            {

                var sheetCreationRequest = service.Spreadsheets.Values.Get(sheetId, range);

                var values = sheetCreationRequest.Execute().Values;

                // Überprüfen, ob Werte vorhanden sind
                if (values != null && values.Count > 0)
                {
                    // Rückgabe des Werts der ersten Zelle
                    return values[0][0].ToString();
                }
                else
                {
                    return "";
                }
                
            }
        }
        public List<List<string>> GetSpreadsheetDataListRow(string sheetId, string range)
        {
            if (string.IsNullOrEmpty(sheetId))
                throw new ArgumentNullException(nameof(sheetId));

                var sheetCreationRequest = service.Spreadsheets.Values.Get(sheetId, range);

                var response = sheetCreationRequest.Execute();
                var values = response.Values;

                var result = new List<List<string>>();

                if (values != null && values.Count > 0)
                {
                    foreach (var row in values)
                    {
                        var rowData = new List<string>();
                        foreach (var cell in row)
                        {
                            if (cell is string stringValue)
                            {
                                rowData.Add(stringValue);
                            }
                            else
                            {
                                rowData.Add("0"); 
                            }
                        }
                        result.Add(rowData);
                    }
                }
                return result;
        }
        public List<List<string>> GetSpreadsheetDataListColumn(string sheetId, string range)
        {
            if (string.IsNullOrEmpty(sheetId))
                throw new ArgumentNullException(nameof(sheetId));


            var sheetCreationRequest = service.Spreadsheets.Values.Get(sheetId, range);

            var response = sheetCreationRequest.Execute();
            var values = response.Values;

            var result = new List<List<string>>();

            if (values != null && values.Count > 0)
            {
                int numColumns = values[0].Count;

                for (int i = 0; i < numColumns; i++)
                {
                    var columnData = new List<string>();
                    foreach (var row in values)
                    {
                        if (row.Count > i)
                        {
                            var cell = row[i];
                            if (cell is string stringValue)
                            {
                                columnData.Add(stringValue);
                            }
                            else
                            {
                                columnData.Add("0"); 
                            }
                        }
                        else
                        {
                            columnData.Add("0"); 
                        }
                    }
                    result.Add(columnData);
                }
            }
            return result;
            
        }
        public void UpdateSpreadsheetCell(string sheetId, string range, string newValue)
        {
            if (string.IsNullOrEmpty(sheetId))
                throw new ArgumentNullException(nameof(sheetId));

            if (string.IsNullOrEmpty(range))
                throw new ArgumentNullException(nameof(range));

            if (string.IsNullOrEmpty(newValue))
                throw new ArgumentNullException(nameof(newValue));


            // Erstellen eines Update-Requests
            var valueRange = new ValueRange
            {
                Values = new List<IList<object>> { new List<object> { newValue } }
            };

            // Durchführen des Updates
            var updateRequest = service.Spreadsheets.Values.Update(valueRange, sheetId, range);
            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
            var updateResponse = updateRequest.Execute();
            
        }
        public void WriteToSpreadsheet(string sheetId, string range, IList<IList<object>> data)
        {
            if (string.IsNullOrEmpty(sheetId))
                throw new ArgumentNullException(nameof(sheetId));

            if (string.IsNullOrEmpty(range))
                throw new ArgumentNullException(nameof(range));

            if (data == null)
                throw new ArgumentNullException(nameof(data));


            // Erstellen eines Update-Requests
            var valueRange = new ValueRange
            {
                Values = data
            };

            // Durchführen des Updates
            var updateRequest = service.Spreadsheets.Values.Update(valueRange, sheetId, range);
            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
            var updateResponse = updateRequest.Execute();
            
        }
        public void WriteToSpreadsheetColumn(string sheetId, string range, IList<IList<object>> data)
        {
            if (string.IsNullOrEmpty(sheetId))
                throw new ArgumentNullException(nameof(sheetId));

            if (string.IsNullOrEmpty(range))
                throw new ArgumentNullException(nameof(range));

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            // Transponieren der Daten (Umkehrung von Zeilen und Spalten)
            IList<IList<object>> transposedData = TransposeData(data);

            // Erstellen eines Update-Requests
            var valueRange = new ValueRange
            {
                Values = transposedData
            };

            // Durchführen des Updates
            var updateRequest = service.Spreadsheets.Values.Update(valueRange, sheetId, range);
            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
            var updateResponse = updateRequest.Execute();
            
        }
        public IList<IList<object>> TransposeData(IList<IList<object>> data)
        {
            IList<IList<object>> transposedData = new List<IList<object>>();

            if (data.Count == 0)
                return transposedData;

            // Bestimme die maximale Anzahl von Spalten in den Zeilen
            int maxNumCols = data.Max(row => row.Count);

            int numRows = data.Count;

            for (int j = 0; j < maxNumCols; j++)
            {
                List<object> colData = new List<object>();
                for (int i = 0; i < numRows; i++)
                {
                    // Überprüfen, ob die Spalte in dieser Zeile vorhanden ist
                    if (j < data[i].Count)
                        colData.Add(data[i][j]);
                    else
                        colData.Add(null); // Fehlende Elemente mit null auffüllen
                }
                transposedData.Add(colData);
            }


            return transposedData;
        }
        public List<List<object>> TransposeData(List<List<object>> data)
        {
            List<List<object>> transposedData = new List<List<object>>();

            if (data.Count == 0)
                return transposedData;

            // Bestimme die maximale Anzahl von Spalten in den Zeilen
            int maxNumCols = data.Max(row => row.Count);

            int numRows = data.Count;

            for (int j = 0; j < maxNumCols; j++)
            {
                List<object> colData = new List<object>();
                for (int i = 0; i < numRows; i++)
                {
                    // Überprüfen, ob die Spalte in dieser Zeile vorhanden ist
                    if (j < data[i].Count)
                        colData.Add(data[i][j]);
                    else
                        colData.Add(null); // Fehlende Elemente mit null auffüllen
                }
                transposedData.Add(colData);
            }


            return transposedData;
        }
        public void WriteColumnToSpreadsheet(string sheetId, string columnRange, IList<object> columnData)
        {
            if (string.IsNullOrEmpty(sheetId))
                throw new ArgumentNullException(nameof(sheetId));

            if (string.IsNullOrEmpty(columnRange))
                throw new ArgumentNullException(nameof(columnRange));

            if (columnData == null)
                throw new ArgumentNullException(nameof(columnData));


            var valueRange = new ValueRange
            {
                Values = new List<IList<object>> { columnData }
            };

            var updateRequest = service.Spreadsheets.Values.Update(valueRange, sheetId, columnRange);
            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;

            var updateResponse = updateRequest.Execute();
            
        }
        public void CopySheet(string sheetId)
        {
            var sheets = SheetHandler.manager.GetSpreadsheet(SheetHandler.ERS_SHEET_URL);
            CreateNewBased(sheets, "ers copy");
        }
        public  Spreadsheet CreateNewBased(Spreadsheet sheet, string name)
        {

            var sheetCreationRequest = SheetHandler.manager.service.Spreadsheets.Create(body: new Spreadsheet()
            {
                Sheets = sheet.Sheets,

                Properties = new SpreadsheetProperties() { Title = name }
            });
            return sheetCreationRequest.Execute();
        }

        public async Task<SpreadsheetData> ReadSpreadsheet(string sheetId, string range)
        {
            if (string.IsNullOrEmpty(sheetId))
                throw new ArgumentNullException(nameof(sheetId));

            var request = service.Spreadsheets.Values.Get(sheetId, range);
            ValueRange response = await request.ExecuteAsync();
            IList<IList<object>> values = response.Values;

            var rows = values?.Select(row => row.Select(cell => cell?.ToString() ?? "N/V").ToList()).ToList() ?? new List<List<string>>();
            return new SpreadsheetData(rows);
        }

        public async Task WriteSpreadsheet(string sheetId, string range, SpreadsheetData data)
        {
            if (string.IsNullOrEmpty(sheetId))
                throw new ArgumentNullException(nameof(sheetId));

            var valueRange = new ValueRange
            {
                Values = data.Rows.Select(row => (IList<object>)row.Cast<object>().ToList()).ToList()
            };

            var request = service.Spreadsheets.Values.Update(valueRange, sheetId, range);
            request.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
            await request.ExecuteAsync();
        }
    }
}
