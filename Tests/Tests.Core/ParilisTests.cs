using Core;
using Core.Descriptions;
using NUnit.Framework;

namespace Tests.Core {
    [TestFixture]
    public class ParilisTests {

        [Ignore]
        [Test]
        public void TestParilis() {
            DatabaseDescription actual = new DatabaseDescription(new ConnectionInfo {
                DatabaseName = "masterbr",
                HostName = @"localhost\SQLEXPRESS",
                User = "sa",
                Password = "ndqualidade"
            });

            DatabaseDescription reference = new DatabaseDescription(new ConnectionInfo {
                DatabaseName = "feature",
                HostName = @"localhost\SQLEXPRESS",
                User = "sa",
                Password = "ndqualidade"
            });

            Parilis parilis = new Parilis(actual, reference);
            parilis.Run();
        }
    }
}
