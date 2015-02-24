using NUnit.Framework;
using SqlServer.Attributes;
using SqlServer.Enums;

namespace Tests.SqlServer {
    [TestFixture]
    public class EnumsTests {
        [Test]
        public void GetAllowsLengthMustReturnAttributeWithAllOfItsProperties() {
            AllowsLengthAttribute result = Enums.GetAllowsLength(DataType.Varbinary);

            Assert.IsTrue(result.AllowsLength);
            Assert.AreEqual(1, result.MinimumValue);
            Assert.AreEqual(8000, result.MaximumValue);
            Assert.IsTrue(result.AllowsMax);
        }
    }
}