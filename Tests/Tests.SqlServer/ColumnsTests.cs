using System.Collections.Generic;
using System.Linq;
using Core;
using Core.Descriptions;
using Core.Exceptions;
using Core.Interfaces;
using NUnit.Framework;
using SqlServer.Enums;
using Tests.Core;

namespace Tests.SqlServer {
    [TestFixture]
    public class ColumnsTests : DatabaseTest {
        private IColumn columns;
        private IDatabase sqlServerDatabase;

        [TestFixtureSetUp]
        public override void InitializeClass() {
            base.InitializeClass();
            columns = Components.Instance.GetComponent<IColumn>(ConnectionInfo);
            sqlServerDatabase = Components.Instance.GetComponent<IDatabase>(ConnectionInfo);
        }

        [SetUp]
        public void InitializeTest() {
            Database.Tables.Refresh();
            DropTable("TEST_TABLE_2");
            DropTable("TEST_TABLE");
        }

        private void DropTable(string tableName) {
            var table = Database.Tables[tableName];
            if (table != null) {
                if (table.Indexes != null && table.Indexes.Count > 0) table.Indexes[0].Drop();
                table.Drop();
            }
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
        public void WhenColumnHasQuotesInItsName_CreateMethodMustCreateTheColumn() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL)");

            columns.Create(new ColumnDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "id'2",
                Type = "bigint"
            });

            var column = sqlServerDatabase.GetColumn("dbo", "TEST_TABLE", "id'2");

            Assert.IsNotNull(column);
            Assert.AreEqual("id'2", column.Name);
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
        public void WhenColumnIsNotIdentity_CreateMethodMustCreateColumnThatIsNotIdentity() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL)");

            columns.Create(new ColumnDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "id2",
                Type = "bigint",
                AllowsNull = false,
                IsIdentity = false
            });

            var column = sqlServerDatabase.GetColumn("dbo", "TEST_TABLE", "id2");

            Assert.IsNotNull(column);
            Assert.IsFalse(column.IsIdentity);
        }

        [Test]
        public void WhenColumnIsIdentityAndAllowsNullIsSetToTrue_CreateMethodMustThrowException() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL)");

            Assert.Throws<InvalidDescriptionException>(() => columns.Create(new ColumnDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "id2",
                Type = "bigint",
                AllowsNull = true,
                IsIdentity = true
            }));
        }

        [Test]
        public void WhenColumnIsIdentity_CreateMethodMustCreateColumnThatIsIdentity() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL)");

            columns.Create(new ColumnDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "id2",
                Type = "bigint",
                AllowsNull = false,
                IsIdentity = true
            });

            var column = sqlServerDatabase.GetColumn("dbo", "TEST_TABLE", "id2");

            Assert.IsNotNull(column);
            Assert.IsTrue(column.IsIdentity);
        }

        [TestCase("bigint", "255")]
        [TestCase("text", "255")]
        public void WhenLengthIsDefinedWithAnInvalidType_CreateMethodMustThrowException(string dataType, string length) {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL)");

            Assert.Throws<InvalidDataTypeException>(() => columns.Create(new ColumnDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "id2",
                Type = dataType,
                Length = length
            }));
        }

        [TestCase("varchar", "8000")]
        [TestCase("nvarchar", "4000")]
        [TestCase("varchar", "max")]
        [TestCase("nvarchar", "max")]
        [TestCase("char", "8000")]
        [TestCase("nchar", "4000")]
        [TestCase("binary", "8000")]
        [TestCase("varbinary", "8000")]
        [TestCase("varbinary", "max")]
        public void WhenLengthIsDefinedWithAValidType_CreateMethodMustCreateAColumnWithTheLength(string dataType, string length) {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL)");

            columns.Create(new ColumnDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "id2",
                Type = dataType,
                AllowsNull = false,
                Length = length
            });

            var column = sqlServerDatabase.GetColumn("dbo", "TEST_TABLE", "id2");

            Assert.IsNotNull(column);
            Assert.AreEqual(dataType, column.Type);
            Assert.AreEqual(length, column.Length);
        }

        [TestCase("varchar", "8001")]
        [TestCase("nvarchar", "4001")]
        [TestCase("varchar", "0")]
        [TestCase("nvarchar", "0")]
        [TestCase("varchar", "-1")]
        [TestCase("nvarchar", "-1")]
        [TestCase("varchar", "abc")]
        [TestCase("nvarchar", "abc")]
        [TestCase("char", "8001")]
        [TestCase("nchar", "4001")]
        [TestCase("char", "0")]
        [TestCase("nchar", "0")]
        [TestCase("char", "-1")]
        [TestCase("nchar", "-1")]
        [TestCase("char", "max")]
        [TestCase("nchar", "max")]
        [TestCase("float", "54")]
        [TestCase("real", "54")]
        [TestCase("float", "0")]
        [TestCase("real", "0")]
        [TestCase("float", "-1")]
        [TestCase("real", "-1")]
        [TestCase("float", "max")]
        [TestCase("real", "max")]
        [TestCase("binary", "8001")]
        [TestCase("binary", "0")]
        [TestCase("binary", "-1")]
        [TestCase("binary", "max")]
        [TestCase("varbinary", "8001")]
        [TestCase("varbinary", "0")]
        [TestCase("varbinary", "-1")]
        public void WhenLengthIsOutOfBounds_CreateMethodMustThrowException(string dataType, string length) {
            Database.ExecuteNonQuery(string.Format(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL)"));

            Assert.Throws<InvalidDataTypeException>(() => columns.Create(new ColumnDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "id2",
                Type = dataType,
                Length = length
            }));
        }

        [Test]
        public void IfColumnExistsAndIsNotReferencedByAnyConstraint_RemoveMethodMustRemoveTheColumn() {
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
        public void IfColumnhasQuotesInItsName_RemoveMethodMustRemoveTheColumn() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description's] [nvarchar](max) NULL)");

            columns.Remove(new ColumnDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "description's"
            });

            var column = sqlServerDatabase.GetColumn("dbo", "TEST_TABLE", "description's");

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
                Length = "100"
            });

            var column = sqlServerDatabase.GetColumn("dbo", "TEST_TABLE", "id2");

            Assert.IsNotNull(column);
            Assert.AreEqual("id2", column.Name);
            Assert.AreEqual("varchar", column.Type);
            Assert.AreEqual("100", column.Length);
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

        [TestCase("bigint", true)]
        [TestCase("nvarchar", false)]
        public void WhenColumnsExistsAndIsReferencedByAUniqueKey_ChangeTypeMethodMustThrowException(string newType, bool allowsNull) {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](400) NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id),
                CONSTRAINT UQ_TEST_description UNIQUE (description))");

            Assert.Throws<ReferencedColumnException>(() => columns.ChangeType(new ColumnDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "description",
                Type = newType,
                AllowsNull = allowsNull
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

        [TestCase("bigint", "255")]
        [TestCase("text", "255")]
        public void WhenLengthIsDefinedWithAnInvalidType_ChangeTypeMethodMustThrowException(string dataType, string length) {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL)");

            Assert.Throws<InvalidDataTypeException>(() => columns.ChangeType(new ColumnDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "id",
                Type = dataType,
                Length = length
            }));
        }

        [TestCase("varchar", "8000")]
        [TestCase("nvarchar", "4000")]
        [TestCase("varchar", "max")]
        [TestCase("nvarchar", "max")]
        [TestCase("char", "8000")]
        [TestCase("nchar", "4000")]
        [TestCase("binary", "8000")]
        [TestCase("varbinary", "8000")]
        [TestCase("varbinary", "max")]
        public void WhenLengthIsDefinedWithAValidType_ChangeTypeMethodMustChangeDataTypeWithTheLength(string dataType, string length) {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL)");

            columns.ChangeType(new ColumnDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "id",
                Type = dataType,
                AllowsNull = false,
                Length = length
            });

            var column = sqlServerDatabase.GetColumn("dbo", "TEST_TABLE", "id");

            Assert.IsNotNull(column);
            Assert.AreEqual(dataType, column.Type);
            Assert.AreEqual(length, column.Length);
        }

        [TestCase("varchar", "8001")]
        [TestCase("nvarchar", "4001")]
        [TestCase("varchar", "0")]
        [TestCase("nvarchar", "0")]
        [TestCase("varchar", "-1")]
        [TestCase("nvarchar", "-1")]
        [TestCase("varchar", "abc")]
        [TestCase("nvarchar", "abc")]
        [TestCase("char", "8001")]
        [TestCase("nchar", "4001")]
        [TestCase("char", "0")]
        [TestCase("nchar", "0")]
        [TestCase("char", "-1")]
        [TestCase("nchar", "-1")]
        [TestCase("char", "max")]
        [TestCase("nchar", "max")]
        [TestCase("binary", "8001")]
        [TestCase("binary", "0")]
        [TestCase("binary", "-1")]
        [TestCase("binary", "max")]
        [TestCase("varbinary", "8001")]
        [TestCase("varbinary", "0")]
        [TestCase("varbinary", "-1")]
        public void WhenLengthIsOutOfBounds_ChangeTypeMethodMustThrowException(string dataType, string length) {
            Database.ExecuteNonQuery(string.Format(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL)"));

            Assert.Throws<InvalidDataTypeException>(() => columns.ChangeType(new ColumnDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "id",
                Type = dataType,
                Length = length
            }));
        }
    }
}