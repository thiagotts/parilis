using System.Linq;
using Core;
using Core.Descriptions;
using Core.Interfaces;
using NUnit.Framework;
using SqlServer;
using Tests.Core;

namespace Tests.SqlServer {
    [TestFixture]
    public class SqlServerDatabaseTests : DatabaseTest {
        private SqlServerDatabase sqlServerDatabase;

        [TestFixtureSetUp]
        public override void InitializeClass() {
            base.InitializeClass();
            sqlServerDatabase = Components.Instance.GetComponent<IDatabase>(ConnectionInfo) as SqlServerDatabase;
        }

        [SetUp]
        public void FinishTest() {
            Database.Schemas.Refresh();
            if (Database.Schemas["testschema"] == null) {
                Database.ExecuteNonQuery(@"CREATE SCHEMA testschema");
            }

            Database.Tables.Refresh();
            RemoveTable("TEST_TABLE_3");
            RemoveTable("TEST_TABLE_2");
            RemoveTable("TEST'TABLE");
            RemoveTable("TEST_TABLE", "testschema");
            RemoveTable("TEST'TABLE_2");
            RemoveTable("TEST_TABLE");
        }

        private void RemoveTable(string tableName, string schema = null) {
            var table = string.IsNullOrWhiteSpace(schema) ? Database.Tables[tableName] : Database.Tables[tableName, schema];
            if (table != null) table.Drop();
        }

        [Test]
        public void WhenColumnIsNotIdentity_GetColumnMustReturnColumnWithIsIdentityPropertySetToFalse() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id))");

            var column = sqlServerDatabase.GetColumn("dbo", "TEST_TABLE", "id");

            Assert.IsFalse(column.IsIdentity);
        }

        [Test]
        public void WhenColumnIsIdentity_GetColumnMustReturnColumnWithIsIdentityPropertySetToTrue() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] IDENTITY(1,1) NOT NULL,
                [description] [nvarchar](max) NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id))");

            var column = sqlServerDatabase.GetColumn("dbo", "TEST_TABLE", "id");

            Assert.True(column.IsIdentity);
        }

        [Test]
        public void WhenColumnHasQuotesInItsName_GetColumnMustReturnColumnDescription() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] IDENTITY(1,1) NOT NULL,
                [description's] [nvarchar](max) NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id))");

            var column = sqlServerDatabase.GetColumn("dbo", "TEST_TABLE", "description's");

            Assert.IsNotNull(column);
        }

        [Test]
        public void WhenTableHasAPrimaryKeyReferringASingleColumn_MustReturnThePrimaryKeyDescriptionWithTheColumn() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id))");

            var result = sqlServerDatabase.GetPrimaryKey(new TableDescription {Schema = "dbo", Name = "TEST_TABLE"});

            Assert.IsNotNull(result);
            Assert.AreEqual("dbo", result.Schema);
            Assert.AreEqual("TEST_TABLE", result.TableName);
            Assert.AreEqual("PK_dbo_TEST_TABLE_id", result.Name);
            Assert.AreEqual(1, result.Columns.Count());
            Assert.AreEqual("id", result.Columns.Single().Name);
        }

        [Test]
        public void WhenTableHasAPrimaryKeyReferringMultipleColumns_MustReturnThePrimaryKeyDescriptionWithAllColumns() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [id2] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id, id2))");

            var result = sqlServerDatabase.GetPrimaryKey(new TableDescription {Schema = "dbo", Name = "TEST_TABLE"});

            Assert.IsNotNull(result);
            Assert.AreEqual("dbo", result.Schema);
            Assert.AreEqual("TEST_TABLE", result.TableName);
            Assert.AreEqual("PK_dbo_TEST_TABLE_id", result.Name);
            Assert.AreEqual(2, result.Columns.Count());
            Assert.AreEqual("id", result.Columns.First().Name);
            Assert.AreEqual("id2", result.Columns.Last().Name);
        }

        [Test]
        public void WhenTableDoesNotHaveAPrimaryKey_MustReturnNull() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL)");

            var result = sqlServerDatabase.GetPrimaryKey(new TableDescription {Schema = "dbo", Name = "TEST_TABLE"});

            Assert.IsNull(result);
        }

        [Test]
        public void WhenTableNameHasQuotes_MustReturnThePrimaryKeyDescription() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST'TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL,
                CONSTRAINT PK_dbo_TESTTABLE_id PRIMARY KEY (id))");

            var result = sqlServerDatabase.GetPrimaryKey(new TableDescription {Schema = "dbo", Name = "TEST'TABLE"});

            Assert.IsNotNull(result);
        }

        [Test]
        public void WhenThereIsNotAPrimaryKeyWithTheSpecifiedName_MustReturnNull() {
            var result = sqlServerDatabase.GetPrimaryKey("PK_TEST");

            Assert.IsNull(result);
        }

        [Test]
        public void WhenThereIsAPrimaryKeyWithTheSpecifiedName_MustReturnThePrimaryKeyDescription() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                CONSTRAINT PK_TEST PRIMARY KEY (id))");

            var result = sqlServerDatabase.GetPrimaryKey("PK_TEST");

            Assert.IsNotNull(result);
            Assert.AreEqual("PK_TEST", result.Name);
        }

        [Test]
        public void WhenThereIsAPrimaryKeyWithTheSpecifiedNameInAnotherSchema_MustReturnThePrimaryKeyDescription() {
            Database.ExecuteNonQuery(@"CREATE TABLE [testschema].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                CONSTRAINT PK_TEST PRIMARY KEY (id))");

            var result = sqlServerDatabase.GetPrimaryKey("PK_TEST", "testschema");

            Assert.IsNotNull(result);
            Assert.AreEqual("PK_TEST", result.Name);
            Assert.AreEqual("testschema", result.Schema);
        }

        [Test]
        public void WhenThereIsNoForeignKeysReferencingThePrimaryKey_GetForeignKeysReferencingThePrimaryKeyMustReturnAnEmptyList() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                CONSTRAINT PK_TEST PRIMARY KEY (id))");

            var result = sqlServerDatabase.GetForeignKeysReferencing(new PrimaryKeyDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "PK_TEST"
            });

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Any());
        }

        [Test]
        public void WhenThereAreForeignKeysReferencingThePrimaryKey_GetForeignKeysReferencingThePrimaryKeyMustReturnAListWithTheCorrespondingItens() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL,
                CONSTRAINT PK_TEST PRIMARY KEY (id))");

            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE_2](
                [id] [bigint] NOT NULL,
                [id2] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_2_id PRIMARY KEY (id),
                CONSTRAINT FK_TEST_1 FOREIGN KEY (id2) REFERENCES TEST_TABLE(id))");

            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE_3](
                [id] [bigint] NOT NULL,
                [id2] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_3_id PRIMARY KEY (id),
                CONSTRAINT FK_TEST_2 FOREIGN KEY (id2) REFERENCES TEST_TABLE(id))");

            var result = sqlServerDatabase.GetForeignKeysReferencing(new PrimaryKeyDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "PK_TEST"
            });

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Any(key => key.Name.Equals("FK_TEST_1")));
            Assert.IsTrue(result.Any(key => key.Name.Equals("FK_TEST_2")));
        }

        [Test]
        public void WhenTheReferencedTableHasQuotesInItsName_GetForeignKeysMustReturnTheCorrespondingItem() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST'TABLE](
                [id] [bigint] NOT NULL,
                CONSTRAINT PK_TEST PRIMARY KEY (id))");

            var result = sqlServerDatabase.GetForeignKeysReferencing(new PrimaryKeyDescription {
                Schema = "dbo",
                TableName = "TEST'TABLE",
                Name = "PK_TEST"
            });

            Assert.IsNotNull(result);
        }

        [Test]
        public void WhenThereIsNoForeignKeysReferencingTheColumn_GetForeignKeysReferencingTheColumnMustReturnAnEmptyList() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [id_fk] [bigint] NOT NULL,
                CONSTRAINT PK_TEST PRIMARY KEY (id))");

            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE_2](
                [id] [bigint] NOT NULL,
                [id2] [bigint] NOT NULL,
                [id3] [bigint] NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_2_id PRIMARY KEY (id),
                CONSTRAINT FK_TEST_1 FOREIGN KEY (id2) REFERENCES TEST_TABLE(id))");


            var result = sqlServerDatabase.GetForeignKeysReferencing(new ColumnDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE_2",
                Name = "id3"
            });

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Any());
        }

        [Test]
        public void WhenThereAreForeignKeysReferencingTheColumn_GetForeignKeysReferencingTheColumnMustReturnAListWithTheCorrespondingItens() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                CONSTRAINT PK_TEST PRIMARY KEY (id))");

            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE_2](
                [id] [bigint] NOT NULL,
                [id2] [bigint] NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_2_id PRIMARY KEY (id),
                CONSTRAINT FK_TEST_1 FOREIGN KEY (id2) REFERENCES TEST_TABLE(id))");

            var result = sqlServerDatabase.GetForeignKeysReferencing(new ColumnDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE_2",
                Name = "id2"
            });

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.Any(key => key.Name.Equals("FK_TEST_1")));
        }

        [Test]
        public void WhenTheConstraintsTableHasQuotesInItsname_GetForeignKeysReferencingTheColumnMustReturnTheCorrespondingKeys() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                CONSTRAINT PK_TEST PRIMARY KEY (id))");

            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST'TABLE_2](
                [id] [bigint] NOT NULL,
                [id2] [bigint] NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_2_id PRIMARY KEY (id),
                CONSTRAINT FK_TEST_1 FOREIGN KEY (id2) REFERENCES TEST_TABLE(id))");

            var result = sqlServerDatabase.GetForeignKeysReferencing(new ColumnDescription {
                Schema = "dbo",
                TableName = "TEST'TABLE_2",
                Name = "id2"
            });

            Assert.IsNotNull(result);
        }

        [Test]
        public void WhenTableHasOneForeignKey_GetMethodMustReturnTheCorrespondingKey() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                CONSTRAINT PK_TEST PRIMARY KEY (id))");

            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE_2](
                [id] [bigint] NOT NULL,
                [id2] [bigint] NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_2_id PRIMARY KEY (id),
                CONSTRAINT FK_TEST FOREIGN KEY (id2) REFERENCES TEST_TABLE(id))");

            var result = sqlServerDatabase.GetForeignKeys(new TableDescription {Schema = "dbo", Name = "TEST_TABLE_2"});

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("FK_TEST", result.Single().Name);
        }

        [Test]
        public void WhenTableHasMoreThanOneForeignKeys_GetMethodMustReturnTheCorrespondingKeys() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                CONSTRAINT PK_TEST_1 PRIMARY KEY (id))");

            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE_2](
                [id] [bigint] NOT NULL,
                CONSTRAINT PK_TEST_2 PRIMARY KEY (id))");

            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE_3](
                [id] [bigint] NOT NULL,
                [id2] [bigint] NOT NULL,
                [id3] [bigint] NOT NULL,
                CONSTRAINT PK_TEST_3 PRIMARY KEY (id),
                CONSTRAINT FK_TEST_1 FOREIGN KEY (id2) REFERENCES TEST_TABLE(id),
                CONSTRAINT FK_TEST_2 FOREIGN KEY (id3) REFERENCES TEST_TABLE_2(id))");

            var result = sqlServerDatabase.GetForeignKeys(new TableDescription {Schema = "dbo", Name = "TEST_TABLE_3"});

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("FK_TEST_1", result.First().Name);
            Assert.AreEqual("id2", result.First().Columns.Keys.Single());
            Assert.AreEqual("FK_TEST_2", result.Last().Name);
            Assert.AreEqual("id3", result.Last().Columns.Keys.Single());
        }

        [Test]
        public void WhenTableDoesNotHaveForeignKeys_GetMethodMustReturnAnEmptyList() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                CONSTRAINT PK_TEST PRIMARY KEY (id))");

            var result = sqlServerDatabase.GetForeignKeys(new TableDescription {Schema = "dbo", Name = "TEST_TABLE"});

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void WhenTableHasQuotesInItsName_GetMethodMustReturnTheCorrespondingKey() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                CONSTRAINT PK_TEST_TABLE PRIMARY KEY (id))");

            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST'TABLE_2](
                [id] [bigint] NOT NULL,
                [id2] [bigint] NOT NULL,
                CONSTRAINT PK_dbo_TESTTABLE_2_id PRIMARY KEY (id),
                CONSTRAINT FK_TEST FOREIGN KEY (id2) REFERENCES TEST_TABLE(id))");

            var result = sqlServerDatabase.GetForeignKeys(new TableDescription {Schema = "dbo", Name = "TEST'TABLE_2"});

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("FK_TEST", result.Single().Name);
        }

        [Test]
        public void WhenTableHasOneUniqueKey_GetMethodMustReturnTheCorrespondingKey() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](400) NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id),
                CONSTRAINT UQ_TEST_description UNIQUE (description))");

            var result = sqlServerDatabase.GetUniqueKeys(new TableDescription {Schema = "dbo", Name = "TEST_TABLE"});

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("UQ_TEST_description", result.Single().Name);
        }

        [Test]
        public void WhenTableHasMoreThanOneUniqueKey_GetMethodMustReturnTheCorrespondingKeys() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](400) NULL,
                [description2] [nvarchar](400) NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id),
                CONSTRAINT UQ_TEST_description UNIQUE (description),
                CONSTRAINT UQ_TEST_description2 UNIQUE (description2))");

            var result = sqlServerDatabase.GetUniqueKeys(new TableDescription {Schema = "dbo", Name = "TEST_TABLE"});

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("UQ_TEST_description", result.First().Name);
            Assert.AreEqual("UQ_TEST_description2", result.Last().Name);
        }

        [Test]
        public void WhenTableHasAUniqueKeyReferrecingMultipleColumns_GetMethodMustReturnTheCorrespondingKeyWithAllColumnsNames() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](400) NULL,
                [description2] [nvarchar](400) NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id),
                CONSTRAINT UQ_TEST_description UNIQUE (description, description2))");

            var result = sqlServerDatabase.GetUniqueKeys(new TableDescription {Schema = "dbo", Name = "TEST_TABLE"});

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("UQ_TEST_description", result.Single().Name);
            Assert.AreEqual(2, result.Single().Columns.Count());
            Assert.AreEqual("description", result.Single().Columns.First().Name);
            Assert.AreEqual("description2", result.Single().Columns.Last().Name);
        }

        [Test]
        public void WhenTableDoesNotHaveUniqueKeys_GetMethodMustReturnAnEmptyList() {
            var result = sqlServerDatabase.GetUniqueKeys(new TableDescription {Schema = "dbo", Name = "TEST_TABLE"});

            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void WhenTableHasQuotesInItsName_GetMethodMustReturnTheCorrespondingUniquey() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST'TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](400) NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id),
                CONSTRAINT UQ_TEST_description UNIQUE (description))");

            var result = sqlServerDatabase.GetUniqueKeys(new TableDescription {Schema = "dbo", Name = "TEST'TABLE"});

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("UQ_TEST_description", result.Single().Name);
        }

        [Test]
        public void WhenUniqueKeyExists_GetMethodMustReturnTheCorrespondingKey() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](400) NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id),
                CONSTRAINT UQ_TEST_description UNIQUE (description))");

            var result = sqlServerDatabase.GetUniqueKey("UQ_TEST_description", "dbo");

            Assert.IsNotNull(result);
            Assert.AreEqual("UQ_TEST_description", result.Name);
        }

        [Test]
        public void WhenUniqueKeyDoesNotExist_GetMethodMustReturnNull() {
            var result = sqlServerDatabase.GetUniqueKey("UQ_TEST_description", "dbo");

            Assert.IsNull(result);
        }

        [Test]
        public void WhenIndexExists_GetIndexMustReturnTheCorrespondentIndex() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [id2] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL);
                CREATE INDEX index_name ON [dbo].[TEST_TABLE](id, id2)");

            var index = sqlServerDatabase.GetIndex("dbo", "TEST_TABLE", "index_name");

            Assert.IsNotNull(index);
            Assert.AreEqual("index_name", index.Name);
            Assert.AreEqual(2, index.Columns.Count);
            Assert.IsTrue(index.Columns.Any(c => c.Name.Equals("id")));
            Assert.IsTrue(index.Columns.Any(c => c.Name.Equals("id2")));
        }

        [Test]
        public void WhenIndexDoesNotExist_GetIndexMustReturnNull() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [id2] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL)");

            var index = sqlServerDatabase.GetIndex("dbo", "TEST_TABLE", "index_name");

            Assert.IsNull(index);
        }

        [Test]
        public void WhenDatabaseHasNoTables_GetTablesMustReturnAnEmptyList() {
            var result = sqlServerDatabase.GetTables();

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void WhenDatabaseHasTablesOnASingleSchema_GetTablesMustReturnAllTables() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [nvarchar](100) NOT NULL)");

            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE_2](
                [id] [bigint] NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id))");

            var result = sqlServerDatabase.GetTables();

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("dbo", result.First().Schema);
            Assert.AreEqual("TEST_TABLE", result.First().Name);
            Assert.AreEqual(1, result.First().Columns.Count);
            Assert.AreEqual("dbo", result.First().Columns.Single().Schema);
            Assert.AreEqual("TEST_TABLE", result.First().Columns.Single().TableName);
            Assert.AreEqual("id", result.First().Columns.Single().Name);
            Assert.AreEqual("nvarchar", result.First().Columns.Single().Type);
            Assert.AreEqual("100", result.First().Columns.Single().Length);
            Assert.IsFalse(result.First().Columns.Single().AllowsNull);
        }

        [Test]
        public void WhenDatabaseHasTablesOnMultipleSchemas_GetTablesMustReturnAllTables() {
            Database.ExecuteNonQuery(@"CREATE TABLE [testschema].[TEST_TABLE](
                [id] [nvarchar](100) NOT NULL)");

            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE_2](
                [id] [bigint] NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id))");

            var result = sqlServerDatabase.GetTables();

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            var table = result.First(t => t.Schema.Equals("testschema"));
            Assert.AreEqual("testschema", table.Schema);
            Assert.AreEqual("TEST_TABLE", table.Name);
            Assert.AreEqual(1, table.Columns.Count);
            Assert.AreEqual("testschema", table.Columns.Single().Schema);
            Assert.AreEqual("TEST_TABLE", table.Columns.Single().TableName);
            Assert.AreEqual("id", table.Columns.Single().Name);
            Assert.AreEqual("nvarchar", table.Columns.Single().Type);
            Assert.AreEqual("100", table.Columns.Single().Length);
            Assert.IsFalse(table.Columns.Single().AllowsNull);
        }

        [Test]
        public void WhenDatabaseHasNoIndexes_GetIndexesMustReturnAnEmptyList() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](400) NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id),
                CONSTRAINT UQ_TEST_description UNIQUE (description))");

            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE_3](
                [id] [bigint] NOT NULL,
                [id2] [bigint] NOT NULL,
                CONSTRAINT PK_TEST_1 PRIMARY KEY (id),
                CONSTRAINT FK_TEST_1 FOREIGN KEY (id2) REFERENCES TEST_TABLE(id))");

            var result = sqlServerDatabase.GetIndexes();

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void WhenDatabaseHasIndexesOnASingleSchema_GetIndexesMustReturnAllIndexes() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL);
                CREATE INDEX index_name ON [dbo].[TEST_TABLE](id)");

            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE_2](
                [id] [bigint] NOT NULL);
                CREATE INDEX index_name2 ON [dbo].[TEST_TABLE_2](id)");

            var result = sqlServerDatabase.GetIndexes();

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("dbo", result.First().Schema);
            Assert.AreEqual("TEST_TABLE", result.First().TableName);
            Assert.AreEqual("index_name", result.First().Name);
            Assert.AreEqual(1, result.First().Columns.Count);
            Assert.AreEqual("id", result.First().Columns.Single().Name);
            Assert.IsFalse(result.First().Unique);
        }

        [Test]
        public void WhenDatabaseHasIndexesOnMultiplesSchema_GetIndexesMustReturnAllIndexes() {
            Database.ExecuteNonQuery(@"CREATE TABLE [testschema].[TEST_TABLE](
                [id] [bigint] NOT NULL);
                CREATE INDEX index_name ON [testschema].[TEST_TABLE](id)");

            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE_2](
                [id] [bigint] NOT NULL);
                CREATE INDEX index_name2 ON [dbo].[TEST_TABLE_2](id)");

            var result = sqlServerDatabase.GetIndexes();

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            var index = result.First(t => t.Schema.Equals("testschema"));
            Assert.AreEqual("testschema", index.Schema);
            Assert.AreEqual("TEST_TABLE", index.TableName);
            Assert.AreEqual("index_name", index.Name);
            Assert.AreEqual(1, index.Columns.Count);
            Assert.AreEqual("id", index.Columns.Single().Name);
            Assert.IsFalse(index.Unique);
        }

        [Test]
        public void WhenDatabaseHasNoPrimaryKeys_GetPrimaryKeysMustReturnAnEmptyList() {
            Database.ExecuteNonQuery(@"CREATE TABLE [testschema].[TEST_TABLE](
                [id] [bigint] NOT NULL);");

            var result = sqlServerDatabase.GetPrimaryKeys();

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void WhenDatabaseHasPrimaryKeysOnASingleSchema_GetPrimaryKeysMustReturnAllPrimaryKeys() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id))");

            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE_2](
                [id] [bigint] NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_2_id PRIMARY KEY (id))");

            var result = sqlServerDatabase.GetPrimaryKeys();

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            var primaryKey = result.First(t => t.Schema.Equals("dbo"));
            Assert.AreEqual("dbo", primaryKey.Schema);
            Assert.AreEqual("TEST_TABLE", primaryKey.TableName);
            Assert.AreEqual("PK_dbo_TEST_TABLE_id", primaryKey.Name);
            Assert.AreEqual(1, primaryKey.Columns.Count);
            Assert.AreEqual("id", primaryKey.Columns.Single().Name);
        }

        [Test]
        public void WhenDatabaseHasPrimaryKeysOnMultipleSchemas_GetPrimaryKeysMustReturnAllPrimaryKeys() {
            Database.ExecuteNonQuery(@"CREATE TABLE [testschema].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id))");

            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE_2](
                [id] [bigint] NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_2_id PRIMARY KEY (id))");

            var result = sqlServerDatabase.GetPrimaryKeys();

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            var primaryKey = result.First(t => t.Schema.Equals("testschema"));
            Assert.AreEqual("testschema", primaryKey.Schema);
            Assert.AreEqual("TEST_TABLE", primaryKey.TableName);
            Assert.AreEqual("PK_dbo_TEST_TABLE_id", primaryKey.Name);
            Assert.AreEqual(1, primaryKey.Columns.Count);
            Assert.AreEqual("id", primaryKey.Columns.Single().Name);
        }

        [Test]
        public void WhenDatabasehasNoForeignKeys_GetForeignsKeysMustReturnAnEmptyList() {
            var result = sqlServerDatabase.GetForeignKeys();

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void WhenDatabasehasForeignKeysOnASingleSchema_GetForeignsKeysMustReturnAllForeignKeys() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [id2] [bigint] NOT NULL,
                CONSTRAINT PK_TEST PRIMARY KEY (id))");

            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE_2](
                [id] [bigint] NOT NULL,
                [id2] [bigint] NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_2_id PRIMARY KEY (id),
                CONSTRAINT FK_TEST_2 FOREIGN KEY (id2) REFERENCES TEST_TABLE(id))");

            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE_3](
                [id] [bigint] NOT NULL,
                [id2] [bigint] NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_3_id PRIMARY KEY (id),
                CONSTRAINT FK_TEST_3 FOREIGN KEY (id2) REFERENCES TEST_TABLE_2(id))");

            var result = sqlServerDatabase.GetForeignKeys();

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            var foreignKey = result.First(t => t.Schema.Equals("dbo"));
            Assert.AreEqual("dbo", foreignKey.Schema);
            Assert.AreEqual("TEST_TABLE_2", foreignKey.TableName);
            Assert.AreEqual("FK_TEST_2", foreignKey.Name);
            Assert.AreEqual(1, foreignKey.Columns.Count);
            Assert.AreEqual("id2", foreignKey.Columns.Single().Key);
            Assert.AreEqual("dbo", foreignKey.Columns.Single().Value.Schema);
            Assert.AreEqual("TEST_TABLE", foreignKey.Columns.Single().Value.TableName);
            Assert.AreEqual("id", foreignKey.Columns.Single().Value.Name);
        }

        [Test]
        public void WhenDatabasehasForeignKeysOnMultipleSchemas_GetForeignsKeysMustReturnAllForeignKeys() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [id2] [bigint] NOT NULL,
                CONSTRAINT PK_TEST PRIMARY KEY (id))");

            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE_2](
                [id] [bigint] NOT NULL,
                [id2] [bigint] NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_2_id PRIMARY KEY (id),
                CONSTRAINT FK_TEST_2 FOREIGN KEY (id2) REFERENCES TEST_TABLE(id))");

            Database.ExecuteNonQuery(@"CREATE TABLE [testschema].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [id2] [bigint] NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_3_id PRIMARY KEY (id),
                CONSTRAINT FK_TEST_3 FOREIGN KEY (id2) REFERENCES [dbo].[TEST_TABLE](id))");

            var result = sqlServerDatabase.GetForeignKeys();

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            var foreignKey = result.First(t => t.Schema.Equals("testschema"));
            Assert.AreEqual("testschema", foreignKey.Schema);
            Assert.AreEqual("TEST_TABLE", foreignKey.TableName);
            Assert.AreEqual("FK_TEST_3", foreignKey.Name);
            Assert.AreEqual(1, foreignKey.Columns.Count);
            Assert.AreEqual("id2", foreignKey.Columns.Single().Key);
            Assert.AreEqual("dbo", foreignKey.Columns.Single().Value.Schema);
            Assert.AreEqual("TEST_TABLE", foreignKey.Columns.Single().Value.TableName);
            Assert.AreEqual("id", foreignKey.Columns.Single().Value.Name);
        }

        [Test]
        public void WhenDatabaseHasNoUniqueKeys_GetUniqueKeysMustReturnAnEmptyList() {
            var result = sqlServerDatabase.GetUniqueKeys();

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void WhenDatabaseHasUniqueKeysOnASingleSchema_GetUniqueKeysMustReturnAllUniqueKeys() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](400) NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id),
                CONSTRAINT UQ_TEST_description UNIQUE (description))");

            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE_2](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](400) NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_2_id PRIMARY KEY (id),
                CONSTRAINT UQ_TEST_2_description UNIQUE (description))");

            var result = sqlServerDatabase.GetUniqueKeys();

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            var foreignKey = result.First(t => t.Schema.Equals("dbo"));
            Assert.AreEqual("dbo", foreignKey.Schema);
            Assert.AreEqual("TEST_TABLE", foreignKey.TableName);
            Assert.AreEqual("UQ_TEST_description", foreignKey.Name);
            Assert.AreEqual(1, foreignKey.Columns.Count);
            Assert.AreEqual("description", foreignKey.Columns.Single().Name);
        }

        [Test]
        public void WhenDatabaseHasUniqueKeysOnMultipleSchemas_GetUniqueKeysMustReturnAllUniqueKeys() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](400) NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id),
                CONSTRAINT UQ_TEST_description UNIQUE (description))");

            Database.ExecuteNonQuery(@"CREATE TABLE [testschema].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](400) NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_2_id PRIMARY KEY (id),
                CONSTRAINT UQ_TEST_2_description UNIQUE (description))");

            var result = sqlServerDatabase.GetUniqueKeys();

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            var foreignKey = result.First(t => t.Schema.Equals("testschema"));
            Assert.AreEqual("testschema", foreignKey.Schema);
            Assert.AreEqual("TEST_TABLE", foreignKey.TableName);
            Assert.AreEqual("UQ_TEST_2_description", foreignKey.Name);
            Assert.AreEqual(1, foreignKey.Columns.Count);
            Assert.AreEqual("description", foreignKey.Columns.Single().Name);
        }

        [Test]
        public void WhenDatabaseHasASingleSchema_GetSchemasMustReturnTheSchemaName() {
            Database.ExecuteNonQuery(@"DROP SCHEMA testschema");

            var schemas = sqlServerDatabase.GetSchemas();

            Assert.IsNotNull(schemas);
            Assert.AreEqual(1, schemas.Count);
            Assert.AreEqual("dbo", schemas.Single());
        }

        [Test]
        public void WhenDatabaseHasMultipleSchemas_GetSchemasMustReturnAllSchemaNames() {
            var schemas = sqlServerDatabase.GetSchemas();

            Assert.IsNotNull(schemas);
            Assert.AreEqual(2, schemas.Count);
            Assert.IsTrue(schemas.Any(s => s.Equals("dbo")));
            Assert.IsTrue(schemas.Any(s => s.Equals("testschema")));
        }

        [Test]
        public void WhenDefaultHasQuotesInItsName_GetDefaultMethodMustReturnTheDefaultDescription() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id));
                ALTER TABLE [dbo].[TEST_TABLE] ADD CONSTRAINT [DEFAULT'TEST_TABLE_description] DEFAULT 'test' FOR [description]");

            var defaultValue = sqlServerDatabase.GetDefault("DEFAULT'TEST_TABLE_description", "dbo");

            Assert.IsNotNull(defaultValue);
        }
    }
}