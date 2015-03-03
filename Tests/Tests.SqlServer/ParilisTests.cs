using Core;
using Core.Descriptions;
using NUnit.Framework;

namespace Tests.SqlServer {
    [TestFixture]
    public class ParilisTests {
        [Ignore]
        [Test]
        public void TestParilis() {
            var actual = new DatabaseDescription(new ConnectionInfo {
                DatabaseName = "actualcateb",
                HostName = @"localhost\SQLEXPRESS",
                User = "parilis",
                Password = "yourpassword"
            });

            var reference = new DatabaseDescription(new ConnectionInfo {
                DatabaseName = "reference",
                HostName = @"localhost\SQLEXPRESS",
                User = "parilis",
                Password = "yourpassword"
            });

            var parilis = new Parilis(actual, reference);
            parilis.Run();
        }

        [Ignore]
        [Test]
        public void TestParilisBySchema() {
            var actual = new DatabaseDescription(new ConnectionInfo {
                DatabaseName = "actualcateb",
                HostName = @"localhost\SQLEXPRESS",
                User = "parilis",
                Password = "yourpassword"
            }, "dbo");

            var reference = new DatabaseDescription(new ConnectionInfo {
                DatabaseName = "reference",
                HostName = @"localhost\SQLEXPRESS",
                User = "parilis",
                Password = "yourpassword"
            }, "dbo");

            var parilis = new Parilis(actual, reference);
            parilis.Run();
        }
    }
}