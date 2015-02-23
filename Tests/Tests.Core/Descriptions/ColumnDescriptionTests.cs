using Core.Descriptions;
using NSubstitute;
using NUnit.Framework;

namespace Tests.Core.Descriptions {
    [TestFixture]
    public class ColumnDescriptionTests {
        [Test]
        public void WhenColumnsHaveDifferentFullNames_EqualsMustReturnFalse() {
            var column1 = Substitute.For<ColumnDescription>();
            column1.FullName.Returns("Name 1");
            var column2 = Substitute.For<ColumnDescription>();
            column2.FullName.Returns("Name 2");

            var result = column1.Equals(column2);

            Assert.IsFalse(result);
        }

        [Test]
        public void WhenColumnsTheSameFullName_EqualsMustReturnTrue() {
            var column1 = Substitute.For<ColumnDescription>();
            column1.FullName.Returns("Name");
            var column2 = Substitute.For<ColumnDescription>();
            column2.FullName.Returns("Name");

            var result = column1.Equals(column2);

            Assert.IsTrue(result);
        }

        [Test]
        public void WhenOneOfTheColumnsIsNull_EqualsMustReturnFalse() {
            var column1 = Substitute.For<ColumnDescription>();
            column1.FullName.Returns("Name");

            var result = column1.Equals(null);

            Assert.False(result);
        }

        [Test]
        public void WhenOneOfTheObjectsIsNotAColumnDescription_EqualsMustReturnFalse() {
            var column = Substitute.For<ColumnDescription>();
            column.FullName.Returns("Name");
            var table = Substitute.For<TableDescription>();
            table.FullName.Returns("Name");

            var result = column.Equals(table);

            Assert.False(result);
        }
    }
}