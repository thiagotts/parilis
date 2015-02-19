using Core;
using Core.Descriptions;
using NUnit.Framework;

namespace Tests.SqlServer {
    [TestFixture]
    public class ParilisTests {

//        [Ignore]
        [Test]
        public void TestParilis() {
            DatabaseDescription actual = new DatabaseDescription(new ConnectionInfo {
                DatabaseName = "actual",
                HostName = @"localhost\SQLEXPRESS",
                User = "sa",
                Password = "ndqualidade"
            });

            DatabaseDescription reference = new DatabaseDescription(new ConnectionInfo {
                DatabaseName = "reference",
                HostName = @"localhost\SQLEXPRESS",
                User = "sa",
                Password = "ndqualidade"
            });

            Parilis parilis = new Parilis(actual, reference);
            parilis.Run();
        }
    }
}
