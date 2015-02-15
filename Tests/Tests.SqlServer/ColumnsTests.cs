using System;
using System.Collections.Generic;
using System.Linq;
using Core.Descriptions;
using Core.Exceptions;
using NUnit.Framework;
using SqlServer;
using SqlServer.Enums;
using Tests.Core;

namespace Tests.SqlServer {
    [TestFixture]
    public class ColumnsTests : DatabaseTest {
        private Columns columns;
        private SqlServerDatabase sqlServerDatabase;

        [TestFixtureSetUp]
        public override void InitializeClass() {
            base.InitializeClass();
            columns = new Columns(Database);
            sqlServerDatabase = new SqlServerDatabase(Database);
        }

        [TearDown]
        public void FinishTest() {
            var table = Database.Tables["TEST_TABLE"];
            if (table != null) table.Drop();
        }

        private static readonly string[] InvalidColumnNames = Enums.GetDescriptions<Keyword>().Concat(new List<string> {
            "123abc", "abc abc", "%abc", "$abc", "ab*c", "", "  ", null
        }).ToArray();

        [Test]
        public void WhenColumnDoesNotExist_CreateMethodMustCreateTheColumn() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL)");

            columns.Create(new ColumnDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "id2",
                Type = "bigint"
            });

            var column = sqlServerDatabase.GetColumn("dbo", "TEST_TABLE", "id2");

            Assert.IsNotNull(column);
            Assert.AreEqual("id2", column.Name);
            Assert.AreEqual("bigint", column.Type);
        }

        [Test]
        public void WhenColumnWithTheSameNameAlreadyExists_CreateMethodMustThrowException() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL)");

            Assert.Throws<InvalidColumnNameException>(() => columns.Create(new ColumnDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "id",
                Type = "bigint"
            }));
        }

        [Test]
        public void WhenTargetTableDoesNotExist_CreateMethodMustThrowException() {
            Assert.Throws<TableNotFoundException>(() => columns.Create(new ColumnDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "id",
                Type = "bigint"
            }));
        }

        [Test, TestCaseSource("InvalidColumnNames")]
        public void WhenColumnNameIsInvalid_CreateMethodMustThrowException(string columnName) {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL)");

            Assert.Throws<InvalidColumnNameException>(() => columns.Create(new ColumnDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = columnName,
                Type = "bigint"
            }));
        }

        [Test]
        public void WhenDataTypeIsInvalid_CreateMethodMustThrowException() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL)");

            Assert.Throws<InvalidDataTypeException>(() => columns.Create(new ColumnDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "id2",
                Type = "invalidtype"
            }));
        }

        [Test]
        public void WhenColumnAllowsNull_CreateMethodMustCreateColumnsThatAllowsNull() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL)");

            columns.Create(new ColumnDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "id2",
                Type = "bigint",
                AllowsNull = true
            });

            var column = sqlServerDatabase.GetColumn("dbo", "TEST_TABLE", "id2");

            Assert.IsNotNull(column);
            Assert.IsTrue(column.AllowsNull);
        }

        [Test]
        public void WhenColumnDoesNotAllowNull_CreateMethodMustCreateColumnsThatDoesNotAllowNull() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL)");

            columns.Create(new ColumnDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "id2",
                Type = "bigint",
                AllowsNull = false
            });

            var column = sqlServerDatabase.GetColumn("dbo", "TEST_TABLE", "id2");

            Assert.IsNotNull(column);
            Assert.IsFalse(column.AllowsNull);
        }

        [Test]
        public void WhenMaximumValueIsDefinedWithAnInvalidType_CreateMethodMustThrowException() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL)");

            Assert.Throws<InvalidDataTypeException>(() => columns.Create(new ColumnDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "id2",
                Type = "bigint",
                MaximumSize = "255"
            }));
        }

        [TestCase("varchar", "8000")]
        [TestCase("nvarchar", "4000")]
        [TestCase("varchar", "max")]
        [TestCase("nvarchar", "max")]
        public void WhenMaximumValueIsDefinedWithAValidType_CreateMethodMustCreateAColumnWithTheMaximumSize(string dataType, string maximumValue) {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL)");

            columns.Create(new ColumnDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "id2",
                Type = dataType,
                AllowsNull = false,
                MaximumSize = maximumValue
            });

            var column = sqlServerDatabase.GetColumn("dbo", "TEST_TABLE", "id2");

            Assert.IsNotNull(column);
            Assert.AreEqual(dataType, column.Type);
            Assert.AreEqual(maximumValue, column.MaximumSize);
        }

        [TestCase("varchar", "8001")]
        [TestCase("nvarchar", "4001")]
        [TestCase("varchar", "0")]
        [TestCase("nvarchar", "0")]
        [TestCase("varchar", "-1")]
        [TestCase("nvarchar", "-1")]
        [TestCase("varchar", "abc")]
        [TestCase("nvarchar", "abc")]
        public void WhenMaximumValueIsOutOfBounds_CreateMethodMustThrowException(string dataType, string maximumValue) {
            Database.ExecuteNonQuery(string.Format(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL)"));

            Assert.Throws<InvalidDataTypeException>(() => columns.Create(new ColumnDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "id2",
                Type = dataType,
                MaximumSize = maximumValue
            }));
        }
    }
}