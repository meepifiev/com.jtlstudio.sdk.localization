using System.Collections.Generic;
using System.Text;

namespace JTLStudio.SDK.Localization.Editor
{
    public static class LocalizationCsv
    {
        private const char Separator = ',';

        public static string Export(LocalizationTable table, IReadOnlyList<Language> languages)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("key");

            foreach (Language language in languages)
            {
                builder.Append(Separator).Append(language);
            }

            builder.Append('\n');

            foreach (LocalizationEntry entry in table.Entries)
            {
                builder.Append(Escape(entry.Key));

                foreach (Language language in languages)
                {
                    builder.Append(Separator).Append(Escape(entry.Get(language)));
                }

                builder.Append('\n');
            }

            return builder.ToString();
        }

        public static int Import(LocalizationTable table, string csv)
        {
            List<List<string>> rows = Parse(csv);

            if (rows.Count < 2)
            {
                return 0;
            }

            List<string> header = rows[0];
            List<Language?> columns = new List<Language?>();

            for (int index = 1; index < header.Count; index++)
            {
                columns.Add(System.Enum.TryParse(header[index].Trim(), out Language language) ? language : (Language?)null);
            }

            int changed = 0;

            for (int row = 1; row < rows.Count; row++)
            {
                List<string> cells = rows[row];

                if (cells.Count == 0 || string.IsNullOrWhiteSpace(cells[0]))
                {
                    continue;
                }

                LocalizationEntry entry = table.Add(cells[0].Trim());

                for (int column = 0; column < columns.Count; column++)
                {
                    int cell = column + 1;

                    if (columns[column].HasValue == false || cell >= cells.Count)
                    {
                        continue;
                    }

                    if (entry.Get(columns[column].Value) != cells[cell])
                    {
                        entry.Set(columns[column].Value, cells[cell]);
                        changed++;
                    }
                }
            }

            table.Invalidate();
            return changed;
        }

        public static List<List<string>> Parse(string csv)
        {
            List<List<string>> rows = new List<List<string>>();
            List<string> row = new List<string>();
            StringBuilder cell = new StringBuilder();
            bool quoted = false;

            for (int index = 0; index < csv.Length; index++)
            {
                char symbol = csv[index];

                if (quoted)
                {
                    if (symbol == '"')
                    {
                        if (index + 1 < csv.Length && csv[index + 1] == '"')
                        {
                            cell.Append('"');
                            index++;
                            continue;
                        }

                        quoted = false;
                        continue;
                    }

                    cell.Append(symbol);
                    continue;
                }

                switch (symbol)
                {
                    case '"':
                        quoted = true;
                        break;

                    case Separator:
                        row.Add(cell.ToString());
                        cell.Clear();
                        break;

                    case '\r':
                        break;

                    case '\n':
                        row.Add(cell.ToString());
                        cell.Clear();
                        rows.Add(row);
                        row = new List<string>();
                        break;

                    default:
                        cell.Append(symbol);
                        break;
                }
            }

            if (cell.Length > 0 || row.Count > 0)
            {
                row.Add(cell.ToString());
                rows.Add(row);
            }

            return rows;
        }

        private static string Escape(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "";
            }

            bool needsQuotes = value.IndexOf(Separator) >= 0 || value.IndexOf('"') >= 0 || value.IndexOf('\n') >= 0 || value.IndexOf('\r') >= 0;
            string escaped = value.Replace("\"", "\"\"");
            return needsQuotes ? "\"" + escaped + "\"" : escaped;
        }
    }
}
