using System.Collections.Generic;
using Core;
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
            Mock<IConstraint>();
            Mock<ActionQueue>();
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
            var columnRemoval = new ColumnRemoval(ConnectionInfo, columnDescription);

            columnRemoval.Execute();

            columnRemoval.Columns.Received(1).Remove(Arg.Is<ColumnDescription>(
                d => d.Name.Equals(columnDescription.Name)));
        }

        [Test]
        public void ColumnModificationMustCallColumnsChangeTypeMethod() {
            var columnDescription = new ColumnDescription { Name = "test_name" };
            var columnModification = new ColumnModification(ConnectionInfo, columnDescription);

            columnModification.Execute();

            columnModification.Columns.Received(1).ChangeType(Arg.Is<ColumnDescription>(
                d => d.Name.Equals(columnDescription.Name)));
        }

        [Test]
        public void IfColumnIsReferencedByForeignKeys_ColumnModificationMustRemoveTheForeignKeysAndSetThemUpForLaterCreation() {
            var actionQueue = Components.Instance.GetComponent<ActionQueue>();
            actionQueue.ClearReceivedCalls();
            var database = Components.Instance.GetComponent<IDatabase>();
            var columnDescription = new ColumnDescription { Name = "test_name" };
            var columnModification = new ColumnModification(ConnectionInfo, columnDescription);
            columnModification.Constraints.ClearReceivedCalls();
            columnModification.Columns.ClearReceivedCalls();
            database.GetForeignKeysReferencing(Arg.Any<ColumnDescription>()).Returns(new List<ForeignKeyDescription> {
                new ForeignKeyDescription(),
                new ForeignKeyDescription()
            });

            columnModification.Execute();

            columnModification.Constraints.Received(2).RemoveForeignKey(Arg.Any<ForeignKeyDescription>());
            actionQueue.Received(2).Push(Arg.Any<ForeignKeyCreation>());
            columnModification.Columns.Received(1).ChangeType(Arg.Is<ColumnDescription>(
                d => d.Name.Equals(columnDescription.Name)));
        }
    }
}