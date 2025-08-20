using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DXT_Resultmaker
{
    public class SpreadsheetData
    {
        public List<List<string>> Rows { get; private set; } = new List<List<string>>();

        public SpreadsheetData(List<List<string>> rows)
        {
            Rows = rows ?? new List<List<string>>();
        }

        // Hilfsmethode: Wandelt eine Spaltenbezeichnung (z.B. "A", "B", "AA") in einen 0-basierten Index um.
        private int ColumnLetterToIndex(string column)
        {
            if (string.IsNullOrEmpty(column))
                throw new ArgumentException("Spaltenbezeichnung darf nicht leer sein.");

            int sum = 0;
            foreach (char c in column.ToUpper())
            {
                if (c < 'A' || c > 'Z')
                    throw new ArgumentException("Ungültige Spaltenbezeichnung.");
                sum *= 26;
                sum += (c - 'A' + 1);
            }
            return sum - 1;
        }

        // Optional: Wandelt einen 0-basierten Index in eine Spaltenbezeichnung um.
        public string ColumnIndexToLetter(int index)
        {
            if (index < 0)
                throw new ArgumentException("Index muss >= 0 sein.");

            string result = "";
            while (index >= 0)
            {
                result = (char)('A' + (index % 26)) + result;
                index = index / 26 - 1;
            }
            return result;
        }

        // Standardzugriff über 0-basierte Indizes.
        public string GetValue(int col, int row)
        {
            if (row < Rows.Count && col < Rows[row].Count)
                return Rows[row][col];
            return "";

        }

        // Standard-Setzen eines Wertes über 0-basierte Indizes.
        public void SetValue(int row, int col, string value)
        {
            while (Rows.Count <= row)
                Rows.Add(new List<string>());

            while (Rows[row].Count <= col)
                Rows[row].Add("");

            Rows[row][col] = value;
        }

        // Neuer Zugriff: Setze einen Wert anhand von Spaltenbuchstabe und Zeilennummer (beginnend bei 1)
        public void SetValue(string column, int row, string value)
        {
            int colIndex = ColumnLetterToIndex(column);
            int rowIndex = row - 1;
            SetValue(rowIndex, colIndex, value);
        }
    }
}
