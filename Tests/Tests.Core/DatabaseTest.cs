using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using NUnit.Framework;

namespace Tests.Core {
    [TestFixture]
    public class DatabaseTest {
        protected string ServerHostname = @"localhost\sqlexpress";
        protected string DatabaseName = "TESTS_PARILIS";
        protected string User = "parilis";
        protected string Password = "parilis";
        private Server server;
        protected Database Database;

        [TestFixtureSetUp]
        public void InitializeClass() {
            InitializeServer();
            Database = CreateDatabase();
        }

        [TestFixtureTearDown]
        public void FinishClass() {
            if (Database != null && server.Databases.Contains(DatabaseName)) {
                Database.Drop();
            }
        }

        private void InitializeServer() {
            ServerConnection serverConnection = new ServerConnection(ServerHostname, User, Password);
            server = new Server(serverConnection);
        }

        private Database CreateDatabase() {
            server.Databases.Refresh();
            if (server.Databases.Contains(DatabaseName)) {
                server.Databases[DatabaseName].Drop();
            }

            Database database = new Database(server, DatabaseName);
            server.Databases.Add(database);
            database.Create();

            return database;
        }
    }
}
