using Core;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using NUnit.Framework;

namespace Tests.Core {
    [TestFixture]
    public class DatabaseTest {
        private const string ServerHostname = @"localhost\sqlexpress";
        private const string DatabaseName = "TESTS_PARILIS";
        private const string User = "parilis";
        private const string Password = "parilis";
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
                Database.Drop();
            }
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