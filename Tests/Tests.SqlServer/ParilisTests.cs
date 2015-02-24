using Core;
using Core.Descriptions;
using NUnit.Framework;

namespace Tests.SqlServer {
    [TestFixture]
    public class ParilisTests {

        //[Ignore]
        [Test]
        public void TestParilis() {
            DatabaseDescription actual = new DatabaseDescription(new ConnectionInfo {
                DatabaseName = "actualcateb",
                HostName = @"localhost\SQLEXPRESS",
                User = "parilis",
                Password = "yourpassword"
            });

            DatabaseDescription reference = new DatabaseDescription(new ConnectionInfo {
                DatabaseName = "reference",
                HostName = @"localhost\SQLEXPRESS",
                User = "parilis",
                Password = "yourpassword"
            });

            Parilis parilis = new Parilis(actual, reference);
            parilis.Run();
        }
    }
}
