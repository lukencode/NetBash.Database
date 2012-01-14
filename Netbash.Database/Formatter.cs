using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace NetBash.Database
{
    internal static class Formatter
    {
        internal static string ToConsoleTable(this DataTable dt)
        {
            if (dt == null)
                throw new ArgumentNullException();

            var isEmpty = dt.Rows.Count == 0;
            var rows = dt.AsEnumerable();

            var sb = new StringBuilder();

            //get the column widths
            var columnWidths = new Dictionary<string, int>();
            foreach (DataColumn c in dt.Columns)
            {
                var max = c.ColumnName.Length;

                if (!isEmpty)
                {
                    max = rows.Max(r => r.Field<object>(c.ColumnName).ToString().Length);

                    if (c.ColumnName.Length > max)
                        max = c.ColumnName.Length;
                }

                //Add some space
                columnWidths.Add(c.ColumnName, max + 3);
            }

            foreach (DataColumn c in dt.Columns)
            {
                sb.AppendFormat("{0,-" + columnWidths[c.ColumnName] + "}", c.ColumnName.ToUpper());
            }

            //Bust out a linebreak after the headers
            sb.AppendLine();

            //Another dashed linebreak
            for (int i = 0; i < columnWidths.Sum(c => c.Value); i++)
            {
                sb.Append("-");
            }

            sb.AppendLine();

            if (!isEmpty)
            {
                foreach (var r in rows)
                {
                    foreach (DataColumn c in dt.Columns)
                    {
                        var v = r[c] != null ? r[c].ToString() : "NULL";
                        sb.AppendFormat("{0,-" + columnWidths[c.ColumnName] + "}", v);
                    }

                    sb.AppendLine();
                }
            }
            else
            {
                sb.AppendLine("NO RESULTS");
            }

            //Another dashed linebreak
            for (int i = 0; i < columnWidths.Sum(c => c.Value); i++)
            {
                sb.Append("-");
            }

            sb.AppendLine();

            return sb.ToString();
        }
    }
}
