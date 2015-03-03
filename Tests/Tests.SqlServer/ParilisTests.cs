using Core;
using Core.Descriptions;
using NUnit.Framework;

namespace Tests.SqlServer {
    [TestFixture]
    public class ParilisTests {
        //[Ignore]
        [Test]
        public void TestParilis() {
            var actual = new DatabaseDescription(new ConnectionInfo {
                DatabaseName = "actualsidneuma",
                HostName = @"pcdesvm",
                User = "parilis",
                Password = "yourpassword"
            });

            var reference = new DatabaseDescription(new ConnectionInfo {
                DatabaseName = "reference",
                HostName = @"pcdesvm",
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
                DatabaseName = "actualsidneuma",
                HostName = @"pcdesvm",
                User = "parilis",
                Password = "yourpassword"
            }).FilterBySchema("dbo");

            var reference = new DatabaseDescription(new ConnectionInfo {
                DatabaseName = "reference",
                HostName = @"pcdesvm",
                User = "parilis",
                Password = "yourpassword"
            }).FilterBySchema("dbo");

            var parilis = new Parilis(actual, reference);
            parilis.Run();
        }
    }
}