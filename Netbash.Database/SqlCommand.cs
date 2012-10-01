using System;
using System.Collections.Generic;
using System.Data.EntityClient;
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
                //{ "i|info", "Shows database information",
                //    v => _command = Command.Info },

                { "t|tables", "Lists tables and space used optional filter on provided table name",
                    v => _command = Command.Tables },

                { "s|schema", "Display table schema for the provided table name",
                    v => _command = Command.Schema },

                { "clear", "Removes all rows from database",
                    v => _command = Command.Clear },

                { "cn|connectionstring","Returns the connectionstring name that will be used", 
                    v=>_command=Command.ConnectionStringName},

                { "lcn|listcn","Shows all connectionstring names found",
                    v=>_command = Command.ListConnectionStrings},

                { "c=|conn=", "Name of connection string to use (defaults to first found)",
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

            switch (_command)
            {
                case Command.Execute:
                    return execute(query);

                case Command.Info:
                    return executeEmbedded("DbInfo.sql");

                case Command.Tables:
                    return getTables(query);

                case Command.Schema:
                    return showSchema(query);

                case Command.Clear:
                    return clearRecords();

                case Command.ConnectionStringName:
                    return getConnectionStringName();

                case Command.ListConnectionStrings:
                    return getConnectionStringNames();

                case Command.Help:
                default:
                    return showHelp(p);
            }
        }

        private string getTables(string table)
        {
            var q = EmbeddedResourceHelper.GetResource("TableInfo.sql");

            using (var conn = openConnection())
            {
                var cmd = new SqlCommand(q, conn);

                if (!string.IsNullOrWhiteSpace(table))
                    cmd.Parameters.Add(new SqlParameter("TableQuery", table));
                else
                    cmd.Parameters.Add(new SqlParameter("TableQuery", DBNull.Value));

                var da = new SqlDataAdapter();
                da.SelectCommand = cmd;

                var dt = new DataTable();
                da.Fill(dt);

                return dt.ToConsoleTable();
            }
        }

        private string showSchema(string table)
        {
            if (string.IsNullOrWhiteSpace(table))
                throw new ApplicationException("Please provide a table name eg. sql -t \"Products\"");

            //sql inject your own db, see if i care
            var q = string.Format("exec sp_help '{0}'", table);

            using (var conn = openConnection())
            {
                var cmd = new SqlCommand(q, conn);

                var da = new SqlDataAdapter();
                da.SelectCommand = cmd;

                var ds = new DataSet();
                da.Fill(ds);

                if (ds.Tables.Count == 0)
                    throw new ApplicationException(string.Format("Table '{0}' not found", table));

                return ds.Tables[1].ToConsoleTable();
            }
        }

        private string clearRecords()
        {
            executeEmbedded("ClearRecords.sql");
            return "All records cleared check with \"sql -t\"";
        }

        private string executeEmbedded(string filename)
        {
            var q = EmbeddedResourceHelper.GetResource(filename);
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
            using (var conn = openConnection())
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

            using (var conn = openConnection())
            {
                var cmd = new SqlCommand(q, conn);
                result = cmd.ExecuteNonQuery();
            }

            return result;
        }

        private SqlConnection openConnection()
        {
            var connString = getConnectionString();

            var connection = new SqlConnection(connString);

            return connection;
        }

        private string getConnectionString()
        {
            var connString = ConfigurationManager.ConnectionStrings[_connectionName] ??
                             ConfigurationManager.ConnectionStrings[0];

            string connectionString = CheckForEntityFrameworkConnectionString(connString.ConnectionString);
            return connectionString;
        }

        private string getConnectionStringName()
        {
            if (string.IsNullOrEmpty(_connectionName))
            {
                try
                {
                    return ConfigurationManager.ConnectionStrings[0].Name;
                }
                catch (Exception ex)
                {
                    return "No connectionstrings defined";
                }
            }
            return _connectionName;
        }

        private string getConnectionStringNames()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < ConfigurationManager.ConnectionStrings.Count; i++)
            {
                sb.AppendLine(ConfigurationManager.ConnectionStrings[i].Name);
            }
            return sb.ToString();
        }

        private string CheckForEntityFrameworkConnectionString(string connString)
        {
            if (connString.Contains("provider connection string="))
            {
                //Parse connectionstring from Entity connectionstring
                var entityBuilder = new EntityConnectionStringBuilder(connString);
                return entityBuilder.ProviderConnectionString;
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
            Schema,
            Clear,
            Help,
            ConnectionStringName,
            ListConnectionStrings
        }
    }
}
