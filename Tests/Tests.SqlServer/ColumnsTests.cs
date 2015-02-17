﻿using System.Collections.Generic;
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
            var table = Database.Tables["TEST_TABLE_2"];
            if (table != null) table.Drop();

            table = Database.Tables["TEST_TABLE"];
            if (table != null) table.Drop();
        }

        private static readonly string[] InvalidColumnNames = Enums.GetDescriptions<Keyword>().Concat(new List<string> {
            "123abc", "abc abc", "%abc", "$abc", "ab*c", "", "  ", null, new string('a', 129)
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

        [Test]
        public void IfColumnExistsAndIsNotReferencedByAnyConstraint_RemoveMethosMustRemoveTheColumn() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL)");

            columns.Remove(new ColumnDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "description"
            });

            var column = sqlServerDatabase.GetColumn("dbo", "TEST_TABLE", "description");

            Assert.IsNull(column);
        }

        [Test]
        public void IfColumnExistsAndHasValuesInserted_RemoveMethosMustRemoveTheColumn() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL);
                INSERT INTO [dbo].[TEST_TABLE] VALUES (1,'test');");

            columns.Remove(new ColumnDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "description"
            });

            var column = sqlServerDatabase.GetColumn("dbo", "TEST_TABLE", "description");

            Assert.IsNull(column);
        }

        [Test]
        public void WhenTheColumnIsTheSingleColumnOfItsTable_RemoveMethodMustThrowException() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE]([id] [bigint] NOT NULL)");

            Assert.Throws<SingleColumnException>(() => columns.Remove(new ColumnDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "id"
            }));
        }


        [Test]
        public void IfTheColumnIsAPrimaryKey_RemoveMethodMustThrowException() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](400) NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id))");

            Assert.Throws<ReferencedColumnException>(() => columns.Remove(new ColumnDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "id"
            }));
        }

        [Test]
        public void IfTheColumnIsReferencedByAForeignKey_RemoveMethodMustThrowException() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [id2] [bigint] NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id))");

            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE_2](
                [id] [bigint] NOT NULL,
                [id2] [bigint] NOT NULL,
                CONSTRAINT FK_TEST FOREIGN KEY (id2) REFERENCES TEST_TABLE(id))");

            Assert.Throws<ReferencedColumnException>(() => columns.Remove(new ColumnDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE_2",
                Name = "id2"
            }));
        }

        [Test]
        public void IfTheColumnIsReferencedByAUniqueKey_RemoveMethodMustThrowException() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](400) NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id),
                CONSTRAINT UQ_TEST_description UNIQUE (description))");

            Assert.Throws<ReferencedColumnException>(() => columns.Remove(new ColumnDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "description"
            }));
        }

        [Test]
        public void IfTheColumnIsReferencedByADefaultConstraint_RemoveMethodMustThrowException() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id));
                ALTER TABLE [dbo].[TEST_TABLE] ADD CONSTRAINT [DEFAULT_TEST_TABLE_description] DEFAULT 'test' FOR [description]");

            Assert.Throws<ReferencedColumnException>(() => columns.Remove(new ColumnDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "description"
            }));
        }

        [Test]
        public void IfTheColumnIsReferencedByAnIndex_RemoveMethodMustThrowException() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [id2] [bigint] NOT NULL);
                CREATE INDEX idx_TEST_TABLE_id2 ON [dbo].[TEST_TABLE](id2)");

            Assert.Throws<ReferencedColumnException>(() => columns.Remove(new ColumnDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "id2"
            }));
        }

        [Test]
        public void IfTheColumnDoesNotExist_RemoveMethodMustThrowException() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [id2] [bigint] NOT NULL)");

            Assert.Throws<ColumnNotFoundException>(() => columns.Remove(new ColumnDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "description"
            }));
        }

        [Test]
        public void IfTheTableDoesNotExist_RemoveMethodMustThrowException() {
            Assert.Throws<TableNotFoundException>(() => columns.Remove(new ColumnDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "description"
            }));
        }

        [Test]
        public void WhenColumnsExistsAndIsNotReferencedByAnyConstraint_ChangeTypeMethodMustChangeTheDataType() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [id2] [bigint] NOT NULL)");

            columns.ChangeType(new ColumnDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "id2",
                Type = "varchar",
                MaximumSize = "100"
            });

            var column = sqlServerDatabase.GetColumn("dbo", "TEST_TABLE", "id2");

            Assert.IsNotNull(column);
            Assert.AreEqual("id2", column.Name);
            Assert.AreEqual("varchar", column.Type);
            Assert.AreEqual("100", column.MaximumSize);
        }

        [Test]
        public void WhenColumnsExistsAndHasValidValuesInserted_ChangeTypeMethodMustChangeTheDataType() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL);
                INSERT INTO [dbo].[TEST_TABLE] VALUES (1,'123');");

            columns.ChangeType(new ColumnDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "description",
                Type = "bigint"
            });

            var column = sqlServerDatabase.GetColumn("dbo", "TEST_TABLE", "description");

            Assert.IsNotNull(column);
            Assert.AreEqual("description", column.Name);
            Assert.AreEqual("bigint", column.Type);
        }

        [Test]
        public void WhenColumnsExistsAndHasInvalidValuesInserted_ChangeTypeMethodMustThrowException() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL);
                INSERT INTO [dbo].[TEST_TABLE] VALUES (1,'abc');");

            Assert.Throws<InvalidDataTypeException>(() => columns.ChangeType(new ColumnDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "description",
                Type = "bigint"
            }));
        }

        [Test]
        public void WhenColumnsExistsAndIsReferencedByAPrimaryKey_ChangeTypeMethodMustThrowException() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [int] NOT NULL,
                [description] [nvarchar](400) NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id))");

            Assert.Throws<ReferencedColumnException>(() => columns.ChangeType(new ColumnDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "id",
                Type = "bigint"
            }));
        }

        [Test]
        public void WhenColumnsExistsAndIsReferencedByAForeignKey_ChangeTypeMethodMustThrowException() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [int] NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id))");

            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE_2](
                [id] [bigint] NOT NULL,
                [id2] [int] NOT NULL,
                CONSTRAINT FK_TEST FOREIGN KEY (id2) REFERENCES TEST_TABLE(id))");

            Assert.Throws<ReferencedColumnException>(() => columns.ChangeType(new ColumnDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE_2",
                Name = "id2",
                Type = "bigint"
            }));
        }

        [Test]
        public void WhenColumnsExistsAndIsReferencedByAUniqueKey_ChangeTypeMethodMustThrowException() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](400) NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id),
                CONSTRAINT UQ_TEST_description UNIQUE (description))");

            Assert.Throws<ReferencedColumnException>(() => columns.ChangeType(new ColumnDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "description",
                Type = "bigint"
            }));
        }

        [Test]
        public void WhenColumnsExistsAndIsReferencedByADefaultConstraint_ChangeTypeMethodMustThrowException() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id));
                ALTER TABLE [dbo].[TEST_TABLE] ADD CONSTRAINT [DEFAULT_TEST_TABLE_description] DEFAULT 'test' FOR [description]");

            Assert.Throws<ReferencedColumnException>(() => columns.ChangeType(new ColumnDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "description",
                Type = "bigint"
            }));
        }

        [Test]
        public void WhenColumnsExistsAndIsReferencedByAnIndex_ChangeTypeMethodMustThrowException() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [id2] [int] NOT NULL);
                CREATE INDEX idx_TEST_TABLE_id2 ON [dbo].[TEST_TABLE](id2)");

            Assert.Throws<ReferencedColumnException>(() => columns.ChangeType(new ColumnDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "id2",
                Type = "bigint"
            }));
        }

        [Test]
        public void WhenColumnDoesNotExist_ChangeTypeMethodMustThrowException() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [id2] [int] NOT NULL);");

            Assert.Throws<ColumnNotFoundException>(() => columns.ChangeType(new ColumnDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "id3",
                Type = "bigint"
            }));
        }

        [Test]
        public void WhenTableDoesNotExist_ChangeTypeMethodMustThrowException() {
            Assert.Throws<TableNotFoundException>(() => columns.ChangeType(new ColumnDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "id",
                Type = "bigint"
            }));
        }

        [Test]
        public void WhenDataTypeIsInvalid_ChangeTypeMethodMustThrowException() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL)");

            Assert.Throws<InvalidDataTypeException>(() => columns.ChangeType(new ColumnDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "id",
                Type = "invalidtype"
            }));
        }

        [Test]
        public void WhenColumnAllowsNull_ChangeTypeMethodMustChangeDataTypeAllowingNullValues() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL)");

            columns.ChangeType(new ColumnDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "id",
                Type = "int",
                AllowsNull = true
            });

            var column = sqlServerDatabase.GetColumn("dbo", "TEST_TABLE", "id");

            Assert.IsNotNull(column);
            Assert.AreEqual("int", column.Type);
            Assert.IsTrue(column.AllowsNull);
        }

        [Test]
        public void WhenColumnDoesNotAllowNull_ChangeTypeMethodMustChangeDataTypeNotAllowingNullValues() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL)");

            columns.ChangeType(new ColumnDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "id",
                Type = "int",
                AllowsNull = false
            });

            var column = sqlServerDatabase.GetColumn("dbo", "TEST_TABLE", "id");

            Assert.IsNotNull(column);
            Assert.AreEqual("int", column.Type);
            Assert.IsFalse(column.AllowsNull);
        }

        [Test]
        public void WhenMaximumValueIsDefinedWithAnInvalidType_ChangeTypeMethodMustThrowException() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL)");

            Assert.Throws<InvalidDataTypeException>(() => columns.ChangeType(new ColumnDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "id",
                Type = "int",
                MaximumSize = "255"
            }));
        }

        [TestCase("varchar", "8000")]
        [TestCase("nvarchar", "4000")]
        [TestCase("varchar", "max")]
        [TestCase("nvarchar", "max")]
        public void WhenMaximumValueIsDefinedWithAValidType_ChangeTypeMethodMustChangeDataTypeWithTheMaximumSize(string dataType, string maximumValue) {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL)");

            columns.ChangeType(new ColumnDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "id",
                Type = dataType,
                AllowsNull = false,
                MaximumSize = maximumValue
            });

            var column = sqlServerDatabase.GetColumn("dbo", "TEST_TABLE", "id");

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
        public void WhenMaximumValueIsOutOfBounds_ChangeTypeMethodMustThrowException(string dataType, string maximumValue) {
            Database.ExecuteNonQuery(string.Format(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL)"));

            Assert.Throws<InvalidDataTypeException>(() => columns.ChangeType(new ColumnDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "id",
                Type = dataType,
                MaximumSize = maximumValue
            }));
        }
    }
}