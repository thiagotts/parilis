using System.Collections.Generic;
using Core.Descriptions;
using NUnit.Framework;

namespace Tests.Core.Descriptions {
    [TestFixture]
    public class IndexDescriptionTests : Test {
        private ColumnDescription column1, column2;

        [TestFixtureSetUp]
        public void InitializeClass() {
            column1 = CreateColumnDescription("c1");
            column2 = CreateColumnDescription("c2");
        }

        [Test]
        public void WhenIndexesHaveDifferentFullNames_EqualsMustReturnFalse() {
            var index1 = new IndexDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "index1",
                Columns = new List<ColumnDescription> {column1, column2},
                Unique = true
            };

            var index2 = new IndexDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "index2",
                Columns = new List<ColumnDescription> {column1, column2},
                Unique = true
            };

            var result = index1.Equals(index2);

            Assert.IsFalse(result);
        }

        [Test]
        public void WhenIndexesHaveADifferentAmountOfColumnNames_EqualsMustReturnFalse() {
            var index1 = new IndexDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "index1",
                Columns = new List<ColumnDescription> {column1, column2},
                Unique = true
            };

            var index2 = new IndexDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "index1",
                Columns = new List<ColumnDescription> {column1},
                Unique = true
            };

            var result = index1.Equals(index2);

            Assert.IsFalse(result);
        }

        [Test]
        public void WhenIndexesHaveDifferentColumns_EqualsMustReturnFalse() {
            var index1 = new IndexDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "index1",
                Columns = new List<ColumnDescription> {column1, column2},
                Unique = true
            };

            var column3 = CreateColumnDescription(allowsNull: !column2.AllowsNull);
            var index2 = new IndexDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "index1",
                Columns = new List<ColumnDescription> {column1, column3},
                Unique = true
            };

            var result = index1.Equals(index2);

            Assert.IsFalse(result);
        }

        [Test]
        public void WhenIndexesHaveDifferentUniqueFlags_EqualsMustReturnFalse() {
            var index1 = new IndexDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "index1",
                Columns = new List<ColumnDescription> {column1, column2},
                Unique = true
            };

            var index2 = new IndexDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "index1",
                Columns = new List<ColumnDescription> {column1, column2},
                Unique = false
            };

            var result = index1.Equals(index2);

            Assert.IsFalse(result);
        }

        [Test]
        public void WhenIndexesTheSameFullNameAndColumnNamesAndUniqueFlag_EqualsMustReturnTrue() {
            var index1 = new IndexDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "index1",
                Columns = new List<ColumnDescription> {column1, column2},
                Unique = true
            };

            var index2 = new IndexDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "index1",
                Columns = new List<ColumnDescription> {column1, column2},
                Unique = true
            };

            var result = index1.Equals(index2);

            Assert.IsTrue(result);
        }

        [Test]
        public void WhenOneOfTheIndexesIsNull_EqualsMustReturnFalse() {
            var index1 = new IndexDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "index1",
                Columns = new List<ColumnDescription> {column1, column2},
                Unique = true
            };

            var result = index1.Equals(null);

            Assert.False(result);
        }

        [Test]
        public void WhenOneOfTheObjectsIsNotAIndexDescription_EqualsMustReturnFalse() {
            var index1 = new IndexDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "index1",
                Columns = new List<ColumnDescription> {column1, column2},
                Unique = true
            };

            var unique = new UniqueDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "Default1"
            };

            var result = index1.Equals(unique);

            Assert.False(result);
        }
    }
}