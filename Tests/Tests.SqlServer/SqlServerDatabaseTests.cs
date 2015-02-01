using System.Linq;
using Core.Descriptions;
using NUnit.Framework;
using SqlServer;
using Tests.Core;

namespace Tests.SqlServer {
    [TestFixture]
    public class SqlServerDatabaseTests : DatabaseTest {
        [TestFixtureSetUp]
        public override void InitializeClass() {
            base.InitializeClass();
            Database.ExecuteNonQuery(@"CREATE SCHEMA testschema");
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

            var sqlServerDatabase = new SqlServerDatabase(Database);
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

            var sqlServerDatabase = new SqlServerDatabase(Database);
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

            var sqlServerDatabase = new SqlServerDatabase(Database);
            var result = sqlServerDatabase.GetPrimaryKey(new TableDescription {Schema = "dbo", Name = "TEST_TABLE"});

            Assert.IsNull(result);
        }

        [Test]
        public void WhenThereIsNotAPrimaryKeyWithTheSpecifiedName_MustReturnNull() {
            var sqlServerDatabase = new SqlServerDatabase(Database);
            var result = sqlServerDatabase.GetPrimaryKey("PK_TEST");

            Assert.IsNull(result);
        }

        [Test]
        public void WhenThereIsAPrimaryKeyWithTheSpecifiedName_MustReturnThePrimaryKeyDescription() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                CONSTRAINT PK_TEST PRIMARY KEY (id))");

            var sqlServerDatabase = new SqlServerDatabase(Database);
            var result = sqlServerDatabase.GetPrimaryKey("PK_TEST");

            Assert.IsNotNull(result);
            Assert.AreEqual("PK_TEST", result.Name);
        }

        [Test]
        public void WhenThereIsAPrimaryKeyWithTheSpecifiedNameInAnotherSchema_MustReturnThePrimaryKeyDescription() {
            Database.ExecuteNonQuery(@"CREATE TABLE [testschema].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                CONSTRAINT PK_TEST PRIMARY KEY (id))");

            var sqlServerDatabase = new SqlServerDatabase(Database);
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

            var sqlServerDatabase = new SqlServerDatabase(Database);
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

            var sqlServerDatabase = new SqlServerDatabase(Database);
            var result = sqlServerDatabase.GetForeignKeysReferencing(new PrimaryKeyDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "PK_TEST"
            });

            Assert.IsNotNull(result);
            Assert.AreEqual(2 , result.Count);
            Assert.IsTrue(result.Any(key => key.Name.Equals("FK_TEST_1")));
            Assert.IsTrue(result.Any(key => key.Name.Equals("FK_TEST_2")));
        }
    }
}