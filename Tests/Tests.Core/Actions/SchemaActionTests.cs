using Core.Actions;
using Core.Interfaces;
using NSubstitute;
using NUnit.Framework;

namespace Tests.Core.Actions {
    [TestFixture]
    public class SchemaActionTests : MockTest {
        [TestFixtureSetUp]
        public override void InitializeClass() {
            base.InitializeClass();
            Mock<ISchema>();
        }

        [Test]
        public void SchemaCreationMustCallSchemasCreateMethod() {
            var schemaName = "test_name";
            var tableCreation = new SchemaCreation(ConnectionInfo, schemaName);

            tableCreation.Execute();

            tableCreation.Schemas.Received(1).Create(Arg.Is<string>(d => d.Equals(schemaName)));
        }

        [Test]
        public void SchemaRemovalMustCallSchemasRemoveMethod() {
            var schemaName = "test_name";
            var tableCreation = new SchemaRemoval(ConnectionInfo, schemaName);

            tableCreation.Execute();

            tableCreation.Schemas.Received(1).Remove(Arg.Is<string>(d => d.Equals(schemaName)));
        }
    }
}