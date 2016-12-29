using System.Configuration;
using System.Data.SqlClient;
using Core;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using NUnit.Framework;
using Tests.Core;

namespace Tests.SqlServer {
    [TestFixture]
    public class DatabaseTest : Test {
        private readonly string serverHostname = ConfigurationManager.AppSettings["ServerHostname"];
        private readonly string databaseName = ConfigurationManager.AppSettings["DatabaseName"];
        private readonly string user = ConfigurationManager.AppSettings["User"];
        private readonly string password = ConfigurationManager.AppSettings["Password"];

        private Server server;
        protected Database Database;
        protected ConnectionInfo ConnectionInfo;

        [TestFixtureSetUp]
        public virtual void InitializeClass() {
            InitializeServer();
            Database = CreateDatabase();

            ConnectionInfo = new ConnectionInfo {
                HostName = serverHostname,
                DatabaseName = databaseName,
                User = user,
                Password = password
            };
        }

        [TestFixtureTearDown]
        public void FinishClass() {
            if (Database != null && server.Databases.Contains(databaseName)) {
                string query = string.Format(@"USE master
                                           ALTER DATABASE {0} SET SINGLE_USER WITH ROLLBACK IMMEDIATE
                                           DROP DATABASE {0};", Database.Name);

                Database.ExecuteNonQuery(query);
            }
        }

        protected void RemoveTable(string tableName, string schema = null) {
            var table = string.IsNullOrWhiteSpace(schema) ? Database.Tables[tableName] : Database.Tables[tableName, schema];
            if (table != null) table.Drop();
        }

        private void InitializeServer() {
            var serverConnection = new ServerConnection(serverHostname, user, password);
            server = new Server(serverConnection);
        }

        private Database CreateDatabase() {
            server.Databases.Refresh();
            if (server.Databases.Contains(databaseName)) {
                server.Databases[databaseName].Drop();
            }

            var database = new Database(server, databaseName);
            server.Databases.Add(database);
            database.Create();

            return database;
        }
    }
}