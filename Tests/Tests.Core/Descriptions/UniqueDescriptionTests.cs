using System.Collections.Generic;
using Core.Descriptions;
using NUnit.Framework;

namespace Tests.Core.Descriptions {
    [TestFixture]
    public class UniqueDescriptionTests : Test {
        private ColumnDescription column1, column2;

        [TestFixtureSetUp]
        public void InitializeClass() {
            column1 = CreateColumnDescription("id1");
            column2 = CreateColumnDescription("id2");
        }

        [Test]
        public void WhenUniquesHaveDifferentFullNames_EqualsMustReturnFalse() {
            var unique1 = new UniqueDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "unique1",
                Columns =  {column1, column2}
            };

            var unique2 = new UniqueDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "unique2",
                Columns =  {column1, column2}
            };

            var result = unique1.Equals(unique2);

            Assert.IsFalse(result);
        }

        [Test]
        public void WhenUniquesHaveADifferentAmountOfColumns_EqualsMustReturnFalse() {
            var unique1 = new UniqueDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "unique",
                Columns =  {column1, column2}
            };

            var unique2 = new UniqueDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "unique",
                Columns =  {column1}
            };

            var result = unique1.Equals(unique2);

            Assert.IsFalse(result);
        }

        [Test]
        public void WhenUniquesHaveDifferentColumns_EqualsMustReturnFalse() {
            var column3 = CreateColumnDescription("id2", allowsNull: false);

            var unique1 = new UniqueDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "unique",
                Columns =  {column1, column2}
            };

            var unique2 = new UniqueDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "unique",
                Columns =  {column1, column3}
            };

            var result = unique1.Equals(unique2);

            Assert.IsFalse(result);
        }

        [Test]
        public void WhenUniquesHaveTheSameFullNameAndColumns_EqualsMustReturnTrue() {
            var unique1 = new UniqueDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "unique",
                Columns =  {column1, column2}
            };

            var unique2 = new UniqueDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "unique",
                Columns =  {column1, column2}
            };

            var result = unique1.Equals(unique2);

            Assert.IsTrue(result);
        }

        [Test]
        public void WhenOneOfTheUniquesIsNull_EqualsMustReturnFalse() {
            var unique1 = new UniqueDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "unique",
                Columns =  {column1, column2}
            };

            var result = unique1.Equals(null);

            Assert.False(result);
        }

        [Test]
        public void WhenOneOfTheObjectsIsNotAUniquesDescription_EqualsMustReturnFalse() {
            var unique1 = new UniqueDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "unique",
                Columns =  {column1, column2}
            };

            var index = new IndexDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "index1",
                Columns =  {column1, column2},
                Unique = true
            };

            var result = unique1.Equals(index);

            Assert.False(result);
        }
    }
}