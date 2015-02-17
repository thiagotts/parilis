using Core.Actions;
using Core.Descriptions;
using Core.Interfaces;
using NSubstitute;
using NUnit.Framework;

namespace Tests.Core.Actions {
    [TestFixture]
    public class ColumnActionTests : MockTest {
        [TestFixtureSetUp]
        public override void InitializeClass() {
            base.InitializeClass();
            Mock<IColumn>();
        }

        [Test]
        public void ColumnCreationMustCallColumnsCreateMethod() {
            var columnDescription = new ColumnDescription { Name = "test_name" };
            var columnCreation = new ColumnCreation(ConnectionInfo, columnDescription);

            columnCreation.Execute();

            columnCreation.Columns.Received(1).Create(Arg.Is<ColumnDescription>(
                d => d.Name.Equals(columnDescription.Name)));
        }

        [Test]
        public void ColumnRemovalMustCallColumnsRemoveMethod() {
            var columnDescription = new ColumnDescription { Name = "test_name" };
            var columnCreation = new ColumnRemoval(ConnectionInfo, columnDescription);

            columnCreation.Execute();

            columnCreation.Columns.Received(1).Remove(Arg.Is<ColumnDescription>(
                d => d.Name.Equals(columnDescription.Name)));
        }

        [Test]
        public void ColumnModificationMustCallColumnsChangeTypeMethod() {
            var columnDescription = new ColumnDescription { Name = "test_name" };
            var columnCreation = new ColumnModification(ConnectionInfo, columnDescription);

            columnCreation.Execute();

            columnCreation.Columns.Received(1).ChangeType(Arg.Is<ColumnDescription>(
                d => d.Name.Equals(columnDescription.Name)));
        }
    }
}