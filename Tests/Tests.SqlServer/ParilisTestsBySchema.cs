using System;
using System.Linq;
using Castle.Core.Internal;
using Core;
using Core.Descriptions;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using NUnit.Framework;

namespace Tests.SqlServer {
    [TestFixture]
    public class ParilisTestsBySchema {
        private readonly string serverName = @"localhost\sqlserver";
        private readonly string userName = "parilis";
        private readonly string password = "yourpassword";
        private readonly string referenceDatabaseName = "reference";
        private readonly  string actualDatabaseName = "actual";

        private readonly SqlConnectionInfo sqlconnectionInfo;
        private readonly Server server;

        public ParilisTestsBySchema() {
            sqlconnectionInfo = new SqlConnectionInfo(serverName, userName, password);
            server = new Server(new ServerConnection(sqlconnectionInfo));
        }

        [TestFixtureSetUp]
        public void FixtureSetUp() {
            DatabaseOperations.Drop(server, actualDatabaseName);
            DatabaseOperations.Drop(server, referenceDatabaseName);

            var referenceDatabase = DatabaseOperations.Create(server, referenceDatabaseName);
            new DatabaseOperations(referenceDatabase).CreateFromFile(@"reference\install-adventureworks.sql");

            DatabaseOperations.Create(server, actualDatabaseName);
        }

        [TestCase("dbo", 1)]
        [TestCase("HumanResources", 7)]
        [TestCase("Person", 6)]
        [TestCase("Production", 25)]
        [TestCase("Purchasing", 7)]
        [TestCase("Sales", 22)]
        public void ShouldCreateTheExpectedNumberOfTablesBySchema(string schema, int expectedQtyOfTables) {
            
            var actualDescription = new DatabaseDescription(new ConnectionInfo {
                DatabaseName = actualDatabaseName,
                HostName = serverName,
                User = userName,
                Password = password
            }).FilterBySchema(schema);

            var referenceConnectionInfo = new ConnectionInfo {
                DatabaseName = referenceDatabaseName,
                HostName = serverName,
                User = userName,
                Password = password
            };

            var referenceDescription = new DatabaseDescription(referenceConnectionInfo).FilterBySchema(schema);

            var parilis = new Parilis(actualDescription, referenceDescription);
            parilis.Run();
            
            server.Refresh();
            var database = server.Databases[actualDatabaseName];
            database.Refresh();
            database.Tables.Refresh();
            var qtySchemaTables = database.Tables
                                          .Cast<Table>()
                                          .Count(table=>table.Schema.Equals(schema, StringComparison.InvariantCultureIgnoreCase));

            Assert.That(qtySchemaTables, Is.EqualTo(expectedQtyOfTables));
        }
    }
}