using System.Linq;
using Core.Descriptions;
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
            Database.ExecuteNonQuery(@"CREATE SCHEMA testschema");
            sqlServerDatabase = new SqlServerDatabase(Database);
        }

        [TearDown]
        public void FinishTest() {
            var table = Database.Tables["TEST_TABLE_3"];
            if (table != null) table.Drop();

            table = Database.Tables["TEST_TABLE_2"];
            if (table != null) table.Drop();

            table = Database.Tables["TEST_TABLE"];
            if (table != null) table.Drop();
        }

        [Test]
        public void WhenTableHasAPrimaryKeyReferringASingleColumn_MustReturnThePrimaryKeyDescriptionWithTheColumnName() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id))");

            var result = sqlServerDatabase.GetPrimaryKey(new TableDescription {Schema = "dbo", Name = "TEST_TABLE"});

            Assert.IsNotNull(result);
            Assert.AreEqual("dbo", result.Schema);
            Assert.AreEqual("TEST_TABLE", result.TableName);
            Assert.AreEqual("PK_dbo_TEST_TABLE_id", result.Name);
            Assert.AreEqual(1, result.ColumnNames.Count());
            Assert.AreEqual("id", result.ColumnNames.Single());
        }

        [Test]
        public void WhenTableHasAPrimaryKeyReferringMultipleColumns_MustReturnThePrimaryKeyDescriptionWithAllColumnNames() {
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
            Assert.AreEqual(2, result.ColumnNames.Count());
            Assert.AreEqual("id", result.ColumnNames.First());
            Assert.AreEqual("id2", result.ColumnNames.Last());
        }

        [Test]
        public void WhenTableDowsNotHaveAPrimaryKey_MustReturnNull() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL)");

            var result = sqlServerDatabase.GetPrimaryKey(new TableDescription {Schema = "dbo", Name = "TEST_TABLE"});

            Assert.IsNull(result);
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
            Assert.AreEqual(2, result.Single().ColumnNames.Count());
            Assert.AreEqual("description", result.Single().ColumnNames.First());
            Assert.AreEqual("description2", result.Single().ColumnNames.Last());
        }

        [Test]
        public void WhenTableDoesNotHaveUniqueKeys_GetMethodMustReturnAnEmptyList() {
            var result = sqlServerDatabase.GetUniqueKeys(new TableDescription {Schema = "dbo", Name = "TEST_TABLE"});

            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void WhenUniqueKeyExists_GetMethodMustReturnTheCorrespondingKey() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](400) NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id),
                CONSTRAINT UQ_TEST_description UNIQUE (description))");

            var result = sqlServerDatabase.GetUniqueKey("UQ_TEST_description");

            Assert.IsNotNull(result);
            Assert.AreEqual("UQ_TEST_description", result.Name);
        }

        [Test]
        public void WhenUniqueKeyDoesNotExist_GetMethodMustReturnNull() {
            var result = sqlServerDatabase.GetUniqueKey("UQ_TEST_description");

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
            Assert.AreEqual(2, index.ColumnNames.Count);
            Assert.IsTrue(index.ColumnNames.Any(c => c.Equals("id")));
            Assert.IsTrue(index.ColumnNames.Any(c => c.Equals("id2")));
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
    }
}