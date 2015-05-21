using System.Collections.Generic;
using Core.Descriptions;
using NUnit.Framework;

namespace Tests.Core.Descriptions {
    [TestFixture]
    public class UniqueDescriptionTests : Test {
        private ColumnDescription column1, column2;

        [TestFixtureSetUp]
        public void InitializeClass() {
            column1 = column2 = CreateColumnDescription();
        }

        [Test]
        public void WhenUniquesHaveDifferentFullNames_EqualsMustReturnFalse() {
            var unique1 = new UniqueDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "unique1",
                ColumnNames = new List<string> {"column1", "column2"},
            };

            var unique2 = new UniqueDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "unique2",
                ColumnNames = new List<string> { "column1", "column2" },
            };

            var result = unique1.Equals(unique2);

            Assert.IsFalse(result);
        }

        [Test]
        public void WhenUniquesHaveADifferentAmountOfColumnNames_EqualsMustReturnFalse() {
            var unique1 = new UniqueDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "unique",
                ColumnNames = new List<string> { "column1", "column2" },
            };

            var unique2 = new UniqueDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "unique",
                ColumnNames = new List<string> {"column1"},
            };

            var result = unique1.Equals(unique2);

            Assert.IsFalse(result);
        }

        [Test]
        public void WhenUniquesHaveDifferentColumnNames_EqualsMustReturnFalse() {
            var unique1 = new UniqueDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "unique",
                ColumnNames = new List<string> { "column1", "column2" },
            };

            var unique2 = new UniqueDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "unique",
                ColumnNames = new List<string> { "column1", "column3" },
            };

            var result = unique1.Equals(unique2);

            Assert.IsFalse(result);
        }

        [Test]
        public void WhenUniquesHaveTheSameFullNameAndColumnNames_EqualsMustReturnTrue() {
            var unique1 = new UniqueDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "unique",
                ColumnNames = new List<string> { "column1", "column2" },
            };

            var unique2 = new UniqueDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "unique",
                ColumnNames = new List<string> { "column1", "column2" },
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
                ColumnNames = new List<string> { "column1", "column2" },
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
                ColumnNames = new List<string> { "column1", "column2" },
            };

            var index = new IndexDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "index1",
                Columns = new List<ColumnDescription> {column1, column2},
                Unique = true
            };

            var result = unique1.Equals(index);

            Assert.False(result);
        }
    }
}