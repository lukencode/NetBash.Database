using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetBash;
using System.IO;
using NDesk.Options;
using System.Data.SqlClient;
using System.Data.Common;
using System.Configuration;
using System.Data;

namespace NetBash.Database
{
    [WebCommand("sql", "Query your db with NetBash")]
    public class SqlNetBashCommand : IWebCommand
    {
        private Command _command = Command.Help;
        private string _connectionName;

        public string Process(string[] args)
        {
            var sw = new StringWriter();

            var p = new OptionSet() {
                { "e|execute", "Executes an sql query",
                    v => _command = Command.Execute },
                { "i|info", "Provides database information",
                    v => _command = Command.Info },
                { "t|tables", "Lists tables and space used",
                    v => _command = Command.Tables },
                { "c=|conn=", "name of connection string to use (defaults to first found)",
                  v => _connectionName = v },
                { "h|help", "show this list of options",
                    v => _command = Command.Help }
            };

            List<string> extras;
            try
            {
                extras = p.Parse(args);
            }
            catch (OptionException e)
            {
                sw.Write("sql: ");
                sw.WriteLine(e.Message);
                sw.WriteLine("Try `sql --help' for more information.");
                return sw.ToString();
            }

            var query = extras.FirstOrDefault();

            if (_command == Command.Execute && string.IsNullOrWhiteSpace(query))
                throw new ArgumentNullException("A query must be provided");

            switch(_command)
            {
                case Command.Execute:
                    return execute(query);

                case Command.Info:
                    return getInfo();

                case Command.Tables:
                    return getTableInfo();

                case Command.Help:
                default:
                    return showHelp(p);
            }
        }

        private string getInfo()
        { 
            var connString = getConnectionString();
            var ssb = new SqlConnectionStringBuilder(connString.ConnectionString);
            var db = ssb.InitialCatalog;
            
            return query("sp_helpdb " + db).ToConsoleTable();
        }

        private string getTableInfo()
        {
            var q = EmbeddedResourceHelper.GetResource("TableInfo.sql");
            return query(q).ToConsoleTable();
        }

        private string execute(string q)
        {
            if (isNonQuery(q))
            {
                var results = nonQuery(q);
                return results + " rows affected";
            }
            else
            {
                var dt = query(q);
                return dt.ToConsoleTable();
            }
        }

        private bool isNonQuery(string q)
        {
            var lowered = q.ToLower();

            //this is dodge
            return lowered.StartsWith("update ") || lowered.StartsWith("delete ") || lowered.StartsWith("insert ");
        }

        private DataTable query(string q)
        {
            using (var conn = OpenConnection())
            {
                var cmd = new SqlCommand(q, conn);

                var da = new SqlDataAdapter();
                da.SelectCommand = cmd;

                var dt = new DataTable();
                da.Fill(dt);

                return dt;
            }
        }

        private int nonQuery(string q)
        {
            var result = 0;

            using (var conn = OpenConnection())
            {
                var cmd = new SqlCommand(q, conn);
                result = cmd.ExecuteNonQuery();
            }

            return result;
        }

        private SqlConnection OpenConnection() 
        {
            var connString = getConnectionString();

            var connection = new SqlConnection(connString.ConnectionString);
            connection.Open();

            return connection;
        }

        private ConnectionStringSettings getConnectionString()
        {
            var connString = ConfigurationManager.ConnectionStrings[_connectionName];

            if (connString == null)
            {
                connString = ConfigurationManager.ConnectionStrings[0];
            }
            return connString;
        }

        private string showHelp(OptionSet p)
        {
            var sb = new StringWriter();

            sb.WriteLine("Usage: sql [OPTIONS] [QUERY]");
            sb.WriteLine("Execute sql on your database using NetBash");
            sb.WriteLine();
            sb.WriteLine("Options:");

            p.WriteOptionDescriptions(sb);

            return sb.ToString();
        }

        public bool ReturnHtml
        {
            get { return false; }
        }

        private enum Command
        {
            Execute,
            Info, 
            Tables,
            Help
        }
    }
}
