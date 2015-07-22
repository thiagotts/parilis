using System.Data.SqlClient;
using Core;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using NUnit.Framework;
using Tests.Core;

namespace Tests.SqlServer {
    [TestFixture]
    public class DatabaseTest : Test {
        private const string ServerHostname = @"localhost\sqlexpress";
        private const string DatabaseName = "TESTS_PARILIS";
        private const string User = "parilis";
        private const string Password = "yourpassword";
        private Server server;
        protected Database Database;
        protected ConnectionInfo ConnectionInfo;

        [TestFixtureSetUp]
        public virtual void InitializeClass() {
            InitializeServer();
            Database = CreateDatabase();

            ConnectionInfo = new ConnectionInfo {
                HostName = ServerHostname,
                DatabaseName = DatabaseName,
                User = User,
                Password = Password
            };
        }

        [TestFixtureTearDown]
        public void FinishClass() {
            if (Database != null && server.Databases.Contains(DatabaseName)) {
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
            var serverConnection = new ServerConnection(ServerHostname, User, Password);
            server = new Server(serverConnection);
        }

        private Database CreateDatabase() {
            server.Databases.Refresh();
            if (server.Databases.Contains(DatabaseName)) {
                server.Databases[DatabaseName].Drop();
            }

            var database = new Database(server, DatabaseName);
            server.Databases.Add(database);
            database.Create();

            return database;
        }
    }
}