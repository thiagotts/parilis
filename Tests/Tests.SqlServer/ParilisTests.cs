using Core;
using Core.Descriptions;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using NUnit.Framework;

namespace Tests.SqlServer {
    [TestFixture]
    public class ParilisTests {
        
        [Ignore,Test]
        public void TestParilis() {
            var actual = new DatabaseDescription(new ConnectionInfo {
                DatabaseName = "actual",
                HostName = @"localhost\sqlserver",
                User = "parilis",
                Password = "yourpassword"
            });

            var reference = new DatabaseDescription(new ConnectionInfo {
                DatabaseName = "reference",
                HostName = @"localhost\sqlserver",
                User = "parilis",
                Password = "yourpassword"
            });

            var parilis = new Parilis(actual, reference);
            parilis.Run();
        }
        
        [Test] 
        public void TestParilisBySchema() {
            var serverName = @"localhost\sqlserver";
            var userName = "parilis";
            var password = "yourpassword";

            var sqlconnectionInfo = new SqlConnectionInfo(serverName, userName, password);
            var server = new Server(new ServerConnection(sqlconnectionInfo));
            var actualDatabaseName = "actual";
            DatabaseOperations.Create(server, actualDatabaseName);
            var actualDescription = new DatabaseDescription(new ConnectionInfo {
                DatabaseName = actualDatabaseName,
                HostName = serverName,
                User = userName,
                Password = password
            }).FilterBySchema("dbo");

            var referenceDatabaseName = "reference";
            var referenceConnectionInfo = new ConnectionInfo {
                DatabaseName = referenceDatabaseName,
                HostName = serverName,
                User = userName,
                Password = password
            };
            var referenceDatabase = DatabaseOperations.Create(server, referenceDatabaseName);
            new DatabaseOperations(referenceDatabase).CreateSchemaTablesFrom("dbo", "reference");

            var referenceDescription = new DatabaseDescription(referenceConnectionInfo).FilterBySchema("dbo");

            var parilis = new Parilis(actualDescription, referenceDescription);
            parilis.Run();
        }
    }
}