using Google.Apis.Sheets.v4.Data;

namespace DXT_Resultmaker
{
    public interface SheetInterface
    {
        Spreadsheet CreateNew(string sheetName);
    }
}
