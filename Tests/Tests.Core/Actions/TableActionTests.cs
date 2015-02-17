using Core.Actions;
using Core.Descriptions;
using Core.Interfaces;
using NSubstitute;
using NUnit.Framework;

namespace Tests.Core.Actions {
    [TestFixture]
    public class TableActionTests : MockTest {
        [TestFixtureSetUp]
        public override void InitializeClass() {
            base.InitializeClass();
            Mock<ITable>();
        }

        [Test]
        public void TableCreationMustCallTablesCreateMethod() {
            var tableDescription = new TableDescription { Name = "test_name" };
            var tableCreation = new TableCreation(ConnectionInfo, tableDescription);

            tableCreation.Execute();

            tableCreation.Tables.Received(1).Create(Arg.Is<TableDescription>(
                d => d.Name.Equals(tableDescription.Name)));
        }

        [Test]
        public void TableRemovalMustCallTablesRemoveMethod() {
            var tableDescription = new TableDescription { Schema = "test_schema", Name = "test_name" };
            var tableCreation = new TableRemoval(ConnectionInfo, tableDescription);

            tableCreation.Execute();

            tableCreation.Tables.Received(1).Remove(Arg.Is<string>(d => d.Equals(tableDescription.Schema)),
                Arg.Is<string>(d => d.Equals(tableDescription.Name)));
        }
    }
}