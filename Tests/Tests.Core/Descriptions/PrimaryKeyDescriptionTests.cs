using System.Collections.Generic;
using Core.Descriptions;
using NUnit.Framework;

namespace Tests.Core.Descriptions {
    [TestFixture]
    public class PrimaryKeyDescriptionTests : Test {
        private ColumnDescription column1, column2a, column2b;

        [TestFixtureSetUp]
        public void InitializeClass() {
            column1 = CreateColumnDescription("id1");
            column2a = CreateColumnDescription("id2", allowsNull: true);
            column2b = CreateColumnDescription("id2", allowsNull: false);
        }

        [Test]
        public void WhenPrimaryKeysHaveDifferentFullNames_EqualsMustReturnFalse() {
            var primaryKey1 = new PrimaryKeyDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "primary1",
                Columns = new List<ColumnDescription> {column1, column2a}
            };

            var primaryKey2 = new PrimaryKeyDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "primary2",
                Columns = new List<ColumnDescription> {column1, column2a}
            };

            var result = primaryKey1.Equals(primaryKey2);

            Assert.IsFalse(result);
        }

        [Test]
        public void WhenPrimaryKeysHaveADifferentAmountOfColumns_EqualsMustReturnFalse() {
            var primaryKey1 = new PrimaryKeyDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "primary1",
                Columns = new List<ColumnDescription> {column1, column2a}
            };

            var primaryKey2 = new PrimaryKeyDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "primary1",
                Columns = new List<ColumnDescription> {column1}
            };

            var result = primaryKey1.Equals(primaryKey2);

            Assert.IsFalse(result);
        }

        [Test]
        public void WhenPrimaryKeysHaveDifferentColumns_EqualsMustReturnFalse() {
            var primaryKey1 = new PrimaryKeyDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "primary1",
                Columns = new List<ColumnDescription> {column1, column2a}
            };

            var primaryKey2 = new PrimaryKeyDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "primary1",
                Columns = new List<ColumnDescription> {column1, column2b}
            };

            var result = primaryKey1.Equals(primaryKey2);

            Assert.IsFalse(result);
        }

        [Test]
        public void WhenPrimaryKeysHaveTheSameFullNameAndColumns_EqualsMustReturnTrue() {
            var primaryKey1 = new PrimaryKeyDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "primary1",
                Columns = new List<ColumnDescription> {column1, column2a}
            };

            var primaryKey2 = new PrimaryKeyDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "primary1",
                Columns = new List<ColumnDescription> {column1, column2a}
            };

            var result = primaryKey1.Equals(primaryKey2);

            Assert.IsTrue(result);
        }

        [Test]
        public void WhenOneOfThePrimaryKeysIsNull_EqualsMustReturnFalse() {
            var primaryKey1 = new PrimaryKeyDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "primary1",
                Columns = new List<ColumnDescription> {column1, column2a}
            };

            var result = primaryKey1.Equals(null);

            Assert.False(result);
        }

        [Test]
        public void WhenOneOfTheObjectsIsNotAPrimaryKeyDescription_EqualsMustReturnFalse() {
            var primaryKey1 = new PrimaryKeyDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "primary1",
                Columns = new List<ColumnDescription> {column1, column2a}
            };

            var unique = new UniqueDescription {
                Schema = "dbo",
                TableName = "Table1",
                Name = "Default1"
            };

            var result = primaryKey1.Equals(unique);

            Assert.False(result);
        }
    }
}