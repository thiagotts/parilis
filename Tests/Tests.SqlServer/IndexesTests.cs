using System.Collections.Generic;
using Core;
using Core.Descriptions;
using Core.Exceptions;
using Core.Interfaces;
using NUnit.Framework;
using Tests.Core;

namespace Tests.SqlServer {
    [TestFixture]
    public class IndexesTests : DatabaseTest {
        private IIndex indexes;
        private IDatabase sqlServerDatabase;

        [TestFixtureSetUp]
        public override void InitializeClass() {
            base.InitializeClass();
            indexes = Components.Instance.GetComponent<IIndex>(ConnectionInfo);
            sqlServerDatabase = Components.Instance.GetComponent<IDatabase>(ConnectionInfo);
            Database.ExecuteNonQuery(@"CREATE SCHEMA testschema");
        }

        [TearDown]
        public void FinishTest() {
            var table = Database.Tables["TEST_TABLE_2"];
            if (table != null) table.Drop();

            table = Database.Tables["TEST_TABLE"];
            if (table != null) table.Drop();
        }

        [Test]
        public void WhenThereIsNotAnotherIndexWithTheSameNameInTheSameSchema_CreateMethodMustCreateTheIndex() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [id2] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL)");

            indexes.Create(new IndexDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "idx_TEST_TABLE_id_id2",
                ColumnNames = new List<string> {"id", "id2"}
            });

            var index = sqlServerDatabase.GetIndex("dbo", "TEST_TABLE", "idx_TEST_TABLE_id_id2");

            Assert.IsNotNull(index);
            Assert.AreEqual("idx_TEST_TABLE_id_id2", index.Name);
            Assert.IsFalse(index.Unique);
        }

        [TestCase("[text]")]
        [TestCase("[ntext]")]
        [TestCase("[image]")]
        [TestCase("[xml]")]
        [TestCase("[geography]")]
        [TestCase("[geometry]")]
        [TestCase("[nvarchar](401)")]
        [TestCase("[varchar](901)")]
        public void WhenDataTypeOfAColumnIsInvalid_CreateMethodMustThrowException(string dataType) {
            Database.ExecuteNonQuery(string.Format(@"CREATE TABLE [dbo].[TEST_TABLE](
                        [id] [bigint] NOT NULL,
                        [id2] {0} NOT NULL,
                        [description] [nvarchar](max) NULL)", dataType));

            Assert.Throws<InvalidReferenceColumnException>(() => indexes.Create(new IndexDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "idx_TEST_TABLE_id_id2",
                ColumnNames = new List<string> {"id", "id2"}
            }));
        }

        [Test]
        public void WhenIndexHasRepeatedColoumns_CreateMethodMustThrowException() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                        [id] [bigint] NOT NULL,
                        [id2] [bigint] NOT NULL,
                        [description] [nvarchar](max) NULL)");

            Assert.Throws<InvalidReferenceColumnException>(() => indexes.Create(new IndexDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "idx_TEST_TABLE_id_id2",
                ColumnNames = new List<string> {"id", "id"}
            }));
        }

        [Test]
        public void WhenIndexReferencesAColumnThatDoesNotExist_CreateMethodMustThrowException() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                        [id] [bigint] NOT NULL,
                        [id2] [bigint] NOT NULL,
                        [description] [nvarchar](max) NULL)");

            Assert.Throws<InvalidReferenceColumnException>(() => indexes.Create(new IndexDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "idx_TEST_TABLE_id_id2",
                ColumnNames = new List<string> {"id", "id3"}
            }));
        }

        [Test]
        public void WhenThereIsAnotherIndexWithTheSameNameInTheSameTable_CreateMethodMustThrowException() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                        [id] [bigint] NOT NULL,
                        [id2] [bigint] NOT NULL,
                        [description] [nvarchar](max) NULL);
                        CREATE INDEX idx_TEST_TABLE_id_id2 ON [dbo].[TEST_TABLE](id)");

            Assert.Throws<InvalidIndexNameException>(() => indexes.Create(new IndexDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "idx_TEST_TABLE_id_id2",
                ColumnNames = new List<string> {"id2"}
            }));
        }

        [Test]
        public void WhenThereIsAnotherIndexWithAnotherNameInTheSameTable_CreateMethodMustCreateTheIndex() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [id2] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL);
                CREATE INDEX idx_TEST_TABLE_id ON [dbo].[TEST_TABLE](id)");

            indexes.Create(new IndexDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "idx_TEST_TABLE_id2",
                ColumnNames = new List<string> {"id2"}
            });

            var index = sqlServerDatabase.GetIndex("dbo", "TEST_TABLE", "idx_TEST_TABLE_id2");

            Assert.IsNotNull(index);
            Assert.AreEqual("idx_TEST_TABLE_id2", index.Name);
        }

        [Test]
        public void WhenThereIsAnotherIndexWithTheSameNameInAnotherTableInTheSameSchema_CreateMethodMustCreateTheIndex() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL);
                CREATE INDEX idx_TEST_TABLE_id ON [dbo].[TEST_TABLE](id)");

            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE_2](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL);");

            indexes.Create(new IndexDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE_2",
                Name = "idx_TEST_TABLE_id",
                ColumnNames = new List<string> {"id"}
            });

            var index = sqlServerDatabase.GetIndex("dbo", "TEST_TABLE_2", "idx_TEST_TABLE_id");

            Assert.IsNotNull(index);
            Assert.AreEqual("idx_TEST_TABLE_id", index.Name);
            Assert.AreEqual("TEST_TABLE_2", index.TableName);
        }

        [Test]
        public void WhenThereIsAnotherIndexWithTheSameColumns_CreateMethodMustCreateTheIndex() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                        [id] [bigint] NOT NULL,
                        [description] [nvarchar](max) NULL);
                        CREATE INDEX idx_TEST_TABLE_id ON [dbo].[TEST_TABLE](id)");

            indexes.Create(new IndexDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "idx_TEST_TABLE_id_2",
                ColumnNames = new List<string> {"id"}
            });

            var index = sqlServerDatabase.GetIndex("dbo", "TEST_TABLE", "idx_TEST_TABLE_id_2");

            Assert.IsNotNull(index);
            Assert.AreEqual("idx_TEST_TABLE_id_2", index.Name);
        }

        [Test]
        public void WhenThereIsAnotherIndexWithAColumnInCommon_CreateMethodMustCreateTheIndex() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                        [id] [bigint] NOT NULL,
                        [id2] [bigint] NOT NULL,
                        [id3] [bigint] NOT NULL);
                        CREATE INDEX idx_TEST_TABLE_id ON [dbo].[TEST_TABLE](id, id2)");

            indexes.Create(new IndexDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "idx_TEST_TABLE_id_2",
                ColumnNames = new List<string> {"id2", "id3"}
            });

            var index = sqlServerDatabase.GetIndex("dbo", "TEST_TABLE", "idx_TEST_TABLE_id_2");

            Assert.IsNotNull(index);
            Assert.AreEqual("idx_TEST_TABLE_id_2", index.Name);
        }

        [Test]
        public void WhenThereIsAnotherIndexWithTheSameNameInAnotherSchema_CreateMethodMustCreateTheIndex() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                        [id] [bigint] NOT NULL,
                        [id2] [bigint] NOT NULL,
                        [id3] [bigint] NOT NULL);
                        CREATE INDEX idx_TEST_TABLE_id ON [dbo].[TEST_TABLE](id, id2)");

            indexes.Create(new IndexDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "idx_TEST_TABLE_id_2",
                ColumnNames = new List<string> {"id2", "id3"}
            });

            var index = sqlServerDatabase.GetIndex("dbo", "TEST_TABLE", "idx_TEST_TABLE_id_2");

            Assert.IsNotNull(index);
            Assert.AreEqual("idx_TEST_TABLE_id_2", index.Name);
        }

        [Test]
        public void WhenTheIndexIsUnique_CreateMethodMustCreateAUniqueIndex() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                        [id] [bigint] NOT NULL,
                        [id2] [bigint] NOT NULL);");

            indexes.Create(new IndexDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "idx_TEST_TABLE_id_2",
                ColumnNames = new List<string> {"id"},
                Unique = true
            });

            var index = sqlServerDatabase.GetIndex("dbo", "TEST_TABLE", "idx_TEST_TABLE_id_2");

            Assert.IsNotNull(index);
            Assert.IsTrue(index.Unique);
        }

        [Test]
        public void WhenIndexDoesNotExist_RemoveMethodMustThrowException() {
            Assert.Throws<IndexNotFoundException>(() => indexes.Remove(new IndexDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "idx_TEST_TABLE_id",
                ColumnNames = new List<string> {"id2"}
            }));
        }

        [Test]
        public void WhenIndexExist_RemoveMethosMustRemoveTheCorrespondentIndex() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                        [id] [bigint] NOT NULL,
                        [id2] [bigint] NOT NULL);
                        CREATE INDEX idx_TEST_TABLE_id ON [dbo].[TEST_TABLE](id, id2)");

            var index = sqlServerDatabase.GetIndex("dbo", "TEST_TABLE", "idx_TEST_TABLE_id");
            Assert.IsNotNull(index);

            indexes.Remove(new IndexDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "idx_TEST_TABLE_id"
            });

            index = sqlServerDatabase.GetIndex("dbo", "TEST_TABLE", "idx_TEST_TABLE_id");
            Assert.IsNull(index);
        }
    }
}