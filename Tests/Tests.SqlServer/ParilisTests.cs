using System;
using System.Diagnostics;
using System.Linq;
using Core;
using Core.Descriptions;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using NUnit.Framework;
using Tests.Core.Actions;

namespace Tests.SqlServer {
    [TestFixture]
    public class ParilisTests {
        string referenceDatabaseName = "reference";
        string actualDatabaseName = "actual";
        private SqlConnectionInfo sqlconnectionInfo;
        private Server server;
        string serverName = @"localhost\sqlserver";
        string userName = "parilis";
        string password = "yourpassword";

        [TestFixtureSetUp]
        public void FixtureSetUp() {
            sqlconnectionInfo = new SqlConnectionInfo(serverName, userName, password);
            server = new Server(new ServerConnection(sqlconnectionInfo));

            DatabaseOperations.Drop(server, referenceDatabaseName);
            DatabaseOperations.Drop(server, actualDatabaseName);
        }

        [Ignore, Test]
        public void WhenRunPassingAnEmptyActualDbAgainstAdventureWorksAsReferenceDb_ShouldCreate68TableOnActualDb() {
            DatabaseOperations.Create(server, actualDatabaseName);
            var actualDescription = new DatabaseDescription(new ConnectionInfo {
                DatabaseName = actualDatabaseName,
                HostName = serverName,
                User = userName,
                Password = password
            });

            var referenceConnectionInfo = new ConnectionInfo {
                DatabaseName = referenceDatabaseName,
                HostName = serverName,
                User = userName,
                Password = password
            };
            var referenceDatabase = DatabaseOperations.Create(server, referenceDatabaseName);
            new DatabaseOperations(referenceDatabase).CreateFromFile(@"reference\install-adventureworks.sql");

            var referenceDescription = new DatabaseDescription(referenceConnectionInfo);

            var parilis = new Parilis(actualDescription, referenceDescription);
            parilis.Run();

            server.Refresh();
            var database = server.Databases[actualDatabaseName];
            database.Refresh();
            database.Tables.Refresh();
            var qtySchemaTables = database.Tables.Count;

            Assert.That(qtySchemaTables, Is.EqualTo(68));
        }        
    }
}