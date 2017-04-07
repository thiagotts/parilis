using System.Collections.Generic;
using System.Linq;
using Core;
using Core.Descriptions;
using Core.Exceptions;
using Core.Interfaces;
using NUnit.Framework;
using SqlServer;

namespace Tests.SqlServer {
    [TestFixture]
    public class ConstraintsTests : DatabaseTest {
        private IConstraint constraints;
        private SqlServerDatabase sqlServerDatabase;
        private ColumnDescription columnId, columnDescription;

        [TestFixtureSetUp]
        public override void InitializeClass() {
            base.InitializeClass();
            constraints = Components.Instance.GetComponent<IConstraint>(ConnectionInfo);
            sqlServerDatabase = Components.Instance.GetComponent<IDatabase>(ConnectionInfo) as SqlServerDatabase;
            Database.ExecuteNonQuery(@"CREATE SCHEMA testschema");
            columnId = CreateColumnDescription("id", "bigint", allowsNull: false);
            columnDescription = CreateColumnDescription("description", "nvarchar", "400");
        }

        [SetUp]
        public void InitializeTest() {
            Database.Tables.Refresh();
            RemoveTable("TEST_TABLE_2");
            RemoveTable("TEST'TABLE_2");
            RemoveTable("TEST'TABLE");
            RemoveTable("TEST_TABLE");
            RemoveTable("TEST_TABLE_2", "testschema");
            RemoveTable("TEST_TABLE", "testschema");
        }

        [Test]
        public void WhenTargetTableDoesNotHaveAPrimaryKey_CreateMethodMustCreateThePrimaryKey() {
            Database.ExecuteNonQuery(@"CREATE TABLE [testschema].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL)");

            var primaryKey = new PrimaryKeyDescription {
                Schema = "testschema",
                TableName = "TEST_TABLE",
                Name = "PK_dbo_TEST_TABLE",
                Columns =  {columnId}
            };

            constraints.CreatePrimaryKey(primaryKey);

            var result = sqlServerDatabase.GetPrimaryKey(new TableDescription {Schema = "testschema", Name = "TEST_TABLE"});
            Assert.IsNotNull(result);
            Assert.AreEqual("PK_dbo_TEST_TABLE", result.Name);
        }

        [Test]
        public void WhenTargetTableHasQuotesInItsName_CreateMethodMustCreateThePrimaryKey() {
            Database.ExecuteNonQuery(@"CREATE TABLE [testschema].[TEST'TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL)");

            var primaryKey = new PrimaryKeyDescription {
                Schema = "testschema",
                TableName = "TEST'TABLE",
                Name = "PK_dbo_TEST_TABLE",
                Columns =  {columnId}
            };

            constraints.CreatePrimaryKey(primaryKey);

            var result = sqlServerDatabase.GetPrimaryKey(new TableDescription {Schema = "testschema", Name = "TEST'TABLE"});
            Assert.IsNotNull(result);
            Assert.AreEqual("PK_dbo_TEST_TABLE", result.Name);
        }

        [Test]
        public void WhenTargetTableAlreadyHasAPrimaryKey_CreateMethodMustThrowException() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id))");

            var primaryKey = new PrimaryKeyDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "PK_dbo_TEST_TABLE",
                Columns =  {columnId}
            };

            Assert.Throws<MultiplePrimaryKeysException>(() => constraints.CreatePrimaryKey(primaryKey));
        }

        [Test]
        public void WhenAnotherTableWithinTheSameSchemaHasAPrimaryKeyWithTheSameName_CreateMethodMustThrowException() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL)");

            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE_2](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id))");

            var primaryKey = new PrimaryKeyDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "PK_dbo_TEST_TABLE_id",
                Columns =  {columnId}
            };

            Assert.Throws<InvalidConstraintNameException>(() => constraints.CreatePrimaryKey(primaryKey));
        }

        [Test]
        public void WhenTableFromAnotherSchemaHasAPrimaryKeyWithTheSameName_CreateMethodMustCreateThePrimaryKey() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL)");

            Database.ExecuteNonQuery(@"CREATE TABLE [testschema].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id))");

            var primaryKey = new PrimaryKeyDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "PK_dbo_TEST_TABLE_id",
                Columns =  {columnId}
            };

            constraints.CreatePrimaryKey(primaryKey);

            var result = sqlServerDatabase.GetPrimaryKey(new TableDescription {Schema = "dbo", Name = "TEST_TABLE"});
            Assert.IsNotNull(result);
            Assert.AreEqual("PK_dbo_TEST_TABLE_id", result.Name);
            Assert.AreEqual("dbo", result.Schema);
        }

        [Test]
        public void WhenPrimaryKeyDoesNotExist_RemoveMethodMustThrowException() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id))");

            var primaryKey = new PrimaryKeyDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "PK_TEST",
                Columns =  {columnId}
            };

            Assert.Throws<ConstraintNotFoundException>(() => constraints.RemovePrimaryKey(primaryKey));
        }

        [Test]
        public void WhenPrimaryKeyExistsAndNoOtherKeyReferencesIt_RemoveMethodMustRemoveThePrimaryKey() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id))");

            var primaryKey = new PrimaryKeyDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "PK_dbo_TEST_TABLE_id",
                Columns =  {columnId}
            };

            constraints.RemovePrimaryKey(primaryKey);

            Assert.IsNull(sqlServerDatabase.GetPrimaryKey(primaryKey.Name, primaryKey.Schema));
        }

        [Test]
        public void WhenTargetTableHasQuotesInItsName_RemoveMethodMustRemoveThePrimaryKey() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST'TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id))");

            var primaryKey = new PrimaryKeyDescription {
                Schema = "dbo",
                TableName = "TEST'TABLE",
                Name = "PK_dbo_TEST_TABLE_id",
                Columns =  { columnId }
            };

            constraints.RemovePrimaryKey(primaryKey);

            Assert.IsNull(sqlServerDatabase.GetPrimaryKey(primaryKey.Name, primaryKey.Schema));
        }

        [Test]
        public void WhenPrimaryKeyExistsAndOtherKeyReferencesIt_RemoveMethodMustThrowException() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id))");

            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE_2](
                [id] [bigint] NOT NULL,
                [id2] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_2_id PRIMARY KEY (id),
                CONSTRAINT FK_dbo_TEST_TABLE_id FOREIGN KEY (id2) REFERENCES TEST_TABLE(id))");

            var primaryKey = new PrimaryKeyDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "PK_dbo_TEST_TABLE_id",
                Columns =  {columnId}
            };

            Assert.Throws<ReferencedConstraintException>(() => constraints.RemovePrimaryKey(primaryKey));
        }

        [Test]
        public void WhenTargetTableDoesNotHaveAForeignKeyWithTheSameName_CreateMethodMustCreateTheForeignKey() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id))");

            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE_2](
                [id] [bigint] NOT NULL,
                [id_fk] [bigint] NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_2_id PRIMARY KEY (id))");

            constraints.CreateForeignKey(new ForeignKeyDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE_2",
                Name = "FK_TEST",
                Columns = new Dictionary<string, ColumnDescription> {
                    {
                        "id_fk",
                        new ColumnDescription {
                            Schema = "dbo",
                            TableName = "TEST_TABLE",
                            Name = "id"
                        }
                    }
                }
            });

            var foreignKeys = sqlServerDatabase.GetForeignKeys(new TableDescription {Schema = "dbo", Name = "TEST_TABLE_2"});

            Assert.AreEqual(1, foreignKeys.Count);
            Assert.AreEqual("FK_TEST", foreignKeys.Single().Name);
        }

        [Test]
        public void WhenTargetTableHasQuotesInItsName_CreateMethodMustCreateTheForeignKey() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST'TABLE](
                [id] [bigint] NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id))");

            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST'TABLE_2](
                [id] [bigint] NOT NULL,
                [id_fk] [bigint] NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_2_id PRIMARY KEY (id))");

            constraints.CreateForeignKey(new ForeignKeyDescription {
                Schema = "dbo",
                TableName = "TEST'TABLE_2",
                Name = "FK_TEST",
                Columns = new Dictionary<string, ColumnDescription> {
                    {
                        "id_fk",
                        new ColumnDescription {
                            Schema = "dbo",
                            TableName = "TEST'TABLE",
                            Name = "id"
                        }
                    }
                }
            });

            var foreignKeys = sqlServerDatabase.GetForeignKeys(new TableDescription { Schema = "dbo", Name = "TEST'TABLE_2" });

            Assert.AreEqual(1, foreignKeys.Count);
            Assert.AreEqual("FK_TEST", foreignKeys.Single().Name);
        }

        [Test]
        public void WhenTargetTableAlreadyHasAForeignKeyWithTheSameNameInTheSameSchema_CreateMethodMustThrowException() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id))");

            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE_2](
                [id] [bigint] NOT NULL,
                [id_fk] [bigint] NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_2_id PRIMARY KEY (id),
                CONSTRAINT FK_TEST FOREIGN KEY (id_fk) REFERENCES TEST_TABLE(id))");

            Assert.Throws<InvalidConstraintNameException>(() => constraints.CreateForeignKey(new ForeignKeyDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE_2",
                Name = "FK_TEST",
                Columns = new Dictionary<string, ColumnDescription> {
                    {
                        "id_fk",
                        new ColumnDescription {
                            Schema = "dbo",
                            TableName = "TEST_TABLE",
                            Name = "id"
                        }
                    }
                }
            }));
        }

        [Test]
        public void WhenTargetTableAlreadyHasAForeignKeyWithTheSameNameInAnotherSchema_CreateMethodMustCreateTheForeignKey() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id))");

            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE_2](
                [id] [bigint] NOT NULL,
                [id_fk] [bigint] NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_2_id PRIMARY KEY (id),
                CONSTRAINT FK_TEST FOREIGN KEY (id_fk) REFERENCES TEST_TABLE(id))");

            Database.ExecuteNonQuery(@"CREATE TABLE [testschema].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id))");

            Database.ExecuteNonQuery(@"CREATE TABLE [testschema].[TEST_TABLE_2](
                [id] [bigint] NOT NULL,
                [id_fk] [bigint] NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_2_id PRIMARY KEY (id))");

            constraints.CreateForeignKey(new ForeignKeyDescription {
                Schema = "testschema",
                TableName = "TEST_TABLE_2",
                Name = "FK_TEST",
                Columns = new Dictionary<string, ColumnDescription> {
                    {
                        "id_fk",
                        new ColumnDescription {
                            Schema = "testschema",
                            TableName = "TEST_TABLE",
                            Name = "id"
                        }
                    }
                }
            });

            var foreignKeys = sqlServerDatabase.GetForeignKeys(new TableDescription {Schema = "testschema", Name = "TEST_TABLE_2"});

            Assert.AreEqual(1, foreignKeys.Count);
            Assert.AreEqual("FK_TEST", foreignKeys.Single().Name);
        }

        [Test]
        public void WhenForeignKeysReferencesAnInvalidColumn_CreateMethodMustThrowException() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id))");

            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE_2](
                [id] [bigint] NOT NULL,
                [id_fk] [bigint] NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_2_id PRIMARY KEY (id))");

            Assert.Throws<InvalidReferenceColumnException>(() => constraints.CreateForeignKey(new ForeignKeyDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE_2",
                Name = "FK_TEST",
                Columns = new Dictionary<string, ColumnDescription> {
                    {
                        "id_fk",
                        new ColumnDescription {
                            Schema = "dbo",
                            TableName = "TEST_TABLE",
                            Name = "description"
                        }
                    }
                }
            }));
        }

        [Test]
        public void WhenForeignKeyReferencesAComposedPrimaryKey_CreateMethodMustCreateTheForeignKey() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [id2] [bigint] NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id, id2))");

            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE_2](
                [id] [bigint] NOT NULL,
                [id_fk] [bigint] NOT NULL,
                [id_fk2] [bigint] NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_2_id PRIMARY KEY (id))");

            constraints.CreateForeignKey(new ForeignKeyDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE_2",
                Name = "FK_TEST",
                Columns = new Dictionary<string, ColumnDescription> {
                    {
                        "id_fk",
                        new ColumnDescription {
                            Schema = "dbo",
                            TableName = "TEST_TABLE",
                            Name = "id"
                        }
                    }, {
                        "id_fk2",
                        new ColumnDescription {
                            Schema = "dbo",
                            TableName = "TEST_TABLE",
                            Name = "id2"
                        }
                    }
                }
            });

            var foreignKeys = sqlServerDatabase.GetForeignKeys(new TableDescription {Schema = "dbo", Name = "TEST_TABLE_2"});

            Assert.AreEqual(1, foreignKeys.Count);
            Assert.AreEqual("FK_TEST", foreignKeys.Single().Name);
        }

        [Test]
        public void WhenForeignKeyReferencesAColumnOfTheSameTableTwice_CreateMethodMustThrowException() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [id2] [bigint] NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id, id2))");

            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE_2](
                [id] [bigint] NOT NULL,
                [id_fk] [bigint] NOT NULL,
                [id_fk2] [bigint] NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_2_id PRIMARY KEY (id))");

            Assert.Throws<InvalidDescriptionException>(() => constraints.CreateForeignKey(new ForeignKeyDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE_2",
                Name = "FK_TEST",
                Columns = new Dictionary<string, ColumnDescription> {
                    {
                        "id_fk",
                        new ColumnDescription {
                            Schema = "dbo",
                            TableName = "TEST_TABLE",
                            Name = "id"
                        }
                    }, {
                        "id_fk2",
                        new ColumnDescription {
                            Schema = "dbo",
                            TableName = "TEST_TABLE",
                            Name = "id"
                        }
                    }
                }
            }));
        }

        [Test]
        public void WhenForeignKeysReferencesAColumnThatDoesNotExist_CreateMethodMustThrowException() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id))");

            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE_2](
                [id] [bigint] NOT NULL,
                [id_fk] [bigint] NOT NULL,
                [id_fk2] [bigint] NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_2_id PRIMARY KEY (id))");

            Assert.Throws<InvalidReferenceColumnException>(() => constraints.CreateForeignKey(new ForeignKeyDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE_2",
                Name = "FK_TEST",
                Columns = new Dictionary<string, ColumnDescription> {
                    {
                        "id_fk",
                        new ColumnDescription {
                            Schema = "dbo",
                            TableName = "TEST_TABLE",
                            Name = "id"
                        }
                    }, {
                        "id_fk2",
                        new ColumnDescription {
                            Schema = "dbo",
                            TableName = "TEST_TABLE",
                            Name = "id2"
                        }
                    }
                }
            }));
        }

        [Test]
        public void WhenForeignKeysReferencesATableThatDoesNotExist_CreateMethodMustThrowException() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id))");

            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE_2](
                [id] [bigint] NOT NULL,
                [id_fk] [bigint] NOT NULL,
                [id_fk2] [bigint] NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_2_id PRIMARY KEY (id))");

            Assert.Throws<InvalidReferenceColumnException>(() => constraints.CreateForeignKey(new ForeignKeyDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE_2",
                Name = "FK_TEST",
                Columns = new Dictionary<string, ColumnDescription> {
                    {
                        "id_fk",
                        new ColumnDescription {
                            Schema = "dbo",
                            TableName = "TEST_TABLE",
                            Name = "id"
                        }
                    }, {
                        "id_fk2",
                        new ColumnDescription {
                            Schema = "dbo",
                            TableName = "TEST_TABLE_3",
                            Name = "id"
                        }
                    }
                }
            }));
        }

        [Test]
        public void WhenForeignKeyExists_RemoveMethodMustRemoveTheKey() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id))");

            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE_2](
                [id] [bigint] NOT NULL,
                [id_fk] [bigint] NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_2_id PRIMARY KEY (id),
                CONSTRAINT FK_TEST FOREIGN KEY (id_fk) REFERENCES TEST_TABLE(id))");

            constraints.RemoveForeignKey(new ForeignKeyDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE_2",
                Name = "FK_TEST"
            });

            var foreignKeys = sqlServerDatabase.GetForeignKeys(new TableDescription {Schema = "dbo", Name = "TEST_TABLE_2"});

            Assert.AreEqual(0, foreignKeys.Count);
        }

        [Test]
        public void WhenTargetTableHasQuotesInItsName_RemoveMethodMustRemoveTheKey() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST'TABLE](
                [id] [bigint] NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id))");

            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST'TABLE_2](
                [id] [bigint] NOT NULL,
                [id_fk] [bigint] NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_2_id PRIMARY KEY (id),
                CONSTRAINT FK_TEST FOREIGN KEY (id_fk) REFERENCES [TEST'TABLE](id))");

            constraints.RemoveForeignKey(new ForeignKeyDescription {
                Schema = "dbo",
                TableName = "TEST'TABLE_2",
                Name = "FK_TEST"
            });

            var foreignKeys = sqlServerDatabase.GetForeignKeys(new TableDescription {Schema = "dbo", Name = "TEST'TABLE_2"});

            Assert.AreEqual(0, foreignKeys.Count);
        }

        [Test]
        public void WhenForeignKeysTableDoesNotExist_RemoveMethodMustThrowException() {
            Assert.Throws<ConstraintNotFoundException>(() => constraints.RemoveForeignKey(new ForeignKeyDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE_2",
                Name = "FK_TEST"
            }));
        }

        [Test]
        public void WhenForeignKeyDoesNotExist_RemoveMethodMustThrowException() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id))");

            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE_2](
                [id] [bigint] NOT NULL,
                [id_fk] [bigint] NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_2_id PRIMARY KEY (id),
                CONSTRAINT FK_TEST FOREIGN KEY (id_fk) REFERENCES TEST_TABLE(id))");

            Assert.Throws<ConstraintNotFoundException>(() => constraints.RemoveForeignKey(new ForeignKeyDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE_2",
                Name = "FK_TEST_2"
            }));
        }

        [Test]
        public void WhenTargetTableDoesNotHaveAUniqueKeyWithTheSameName_CreateMethodMustCreateTheUniqueKey() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](400) NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id))");

            constraints.CreateUnique(new UniqueDescription {
                Name = "UQ_TEST_description",
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Columns =  {columnDescription}
            });

            var uniqueKeys = sqlServerDatabase.GetUniqueKeys(new TableDescription {Schema = "dbo", Name = "TEST_TABLE"});

            Assert.AreEqual(1, uniqueKeys.Count);
            Assert.AreEqual("UQ_TEST_description", uniqueKeys.Single().Name);
        }

        [Test]
        public void WhenTargetTableHasQuotesInItsName_CreateMethodMustCreateTheUniqueKey() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST'TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](400) NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id))");

            constraints.CreateUnique(new UniqueDescription {
                Name = "UQ_TEST_description",
                Schema = "dbo",
                TableName = "TEST'TABLE",
                Columns =  { columnDescription }
            });

            var uniqueKeys = sqlServerDatabase.GetUniqueKeys(new TableDescription { Schema = "dbo", Name = "TEST'TABLE" });

            Assert.AreEqual(1, uniqueKeys.Count);
            Assert.AreEqual("UQ_TEST_description", uniqueKeys.Single().Name);
        }

        [Test]
        public void WhenThereIsAnotherUniqueKeyWithTheSameNameInTheSameSchema_CreateMethodMustThrowException() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](400) NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id))");

            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE_2](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](400) NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_2_id PRIMARY KEY (id),
                CONSTRAINT UQ_TEST_description UNIQUE (description))");

            Assert.Throws<InvalidConstraintNameException>(() => constraints.CreateUnique(new UniqueDescription {
                Name = "UQ_TEST_description",
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Columns =  {columnDescription}
            }));
        }

        [Test]
        public void WhenThereIsAnotherUniqueKeyWithTheSameNameInAnotherSchema_CreateMethodMustCreateTheUniqueKey() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](400) NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id))");

            Database.ExecuteNonQuery(@"CREATE TABLE [testschema].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](400) NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_2_id PRIMARY KEY (id),
                CONSTRAINT UQ_TEST_description UNIQUE (description))");

            constraints.CreateUnique(new UniqueDescription {
                Name = "UQ_TEST_description",
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Columns =  {columnDescription}
            });

            var uniqueKeys = sqlServerDatabase.GetUniqueKeys(new TableDescription {Schema = "dbo", Name = "TEST_TABLE"});

            Assert.AreEqual(1, uniqueKeys.Count);
            Assert.AreEqual("UQ_TEST_description", uniqueKeys.Single().Name);
        }

        [Test]
        public void WhenUniqueKeysReferencesAnInvalidColumn_CreateMethodMustThrowException() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](400) NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id))");

            var column = CreateColumnDescription("description2", "nvarchar", "400");

            Assert.Throws<InvalidReferenceColumnException>(() => constraints.CreateUnique(new UniqueDescription {
                Name = "UQ_TEST_description",
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Columns =  {column}
            }));
        }

        [Test]
        public void WhenUniqueKeyReferencesMultipleColumns_CreateMethodMustCreateTheUniqueKey() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](400) NULL,
                [description2] [nvarchar](400) NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id))");

            var columnDescription2 = CreateColumnDescription("description2", "nvarchar", "400");

            constraints.CreateUnique(new UniqueDescription {
                Name = "UQ_TEST_description",
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Columns =  {columnDescription, columnDescription2}
            });

            var uniqueKeys = sqlServerDatabase.GetUniqueKeys(new TableDescription {Schema = "dbo", Name = "TEST_TABLE"});

            Assert.AreEqual(1, uniqueKeys.Count);
            Assert.AreEqual("UQ_TEST_description", uniqueKeys.Single().Name);
            Assert.AreEqual(2, uniqueKeys.Single().Columns.Count);
            Assert.AreEqual("description", uniqueKeys.Single().Columns.First().Name);
            Assert.AreEqual("description2", uniqueKeys.Single().Columns.Last().Name);
        }

        [Test]
        public void WhenUniqueKeyReferencesTheSameColumnTwice_CreateMethodMustThrowException() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](400) NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id))");

            Assert.Throws<InvalidReferenceColumnException>(() => constraints.CreateUnique(new UniqueDescription {
                Name = "UQ_TEST_description",
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Columns =  {columnDescription, columnDescription}
            }));
        }

        [Test]
        public void WhenUniqueKeysReferencesATableThatDoesNotExist_CreateMethodMustThrowException() {
            Assert.Throws<InvalidReferenceColumnException>(() => constraints.CreateUnique(new UniqueDescription {
                Name = "UQ_TEST_description",
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Columns =  {columnDescription}
            }));
        }

        [TestCase("text", null)]
        [TestCase("ntext", null)]
        [TestCase("image", null)]
        [TestCase("xml", null)]
        [TestCase("geography", null)]
        [TestCase("geometry", null)]
        [TestCase("nvarchar", "401")]
        [TestCase("varchar", "901")]
        public void WhenDataTypeOfUniqueKeyIsInvalid_CreateMethodMustThrowException(string type, string length) {
            Database.ExecuteNonQuery(string.Format(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [{0}]{1} NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id))",
                type, string.IsNullOrWhiteSpace(length) ? string.Empty : string.Format("({0})", length)));

            var column = CreateColumnDescription("description", type, length);

            Assert.Throws<InvalidReferenceColumnException>(() => constraints.CreateUnique(new UniqueDescription {
                Name = "UQ_TEST_description",
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Columns =  {column}
            }));
        }

        [Test]
        public void IfTargetTableHasDuplicatedValuesForAColumnReferrencedByTheUniqueKey_CreateMethodMustThrowException() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](400) NULL,
                [description2] [nvarchar](400) NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id))");

            Database.ExecuteNonQuery(@"INSERT INTO [TESTS_PARILIS].[dbo].[TEST_TABLE] ([id], [description], [description2]) VALUES (1, 'test', 'test2');
                                       INSERT INTO [TESTS_PARILIS].[dbo].[TEST_TABLE] ([id], [description], [description2]) VALUES (2, 'test', 'test2');");

            var columnDescription2 = CreateColumnDescription("description2", "nvarchar", "400");

            Assert.Throws<InvalidReferenceColumnException>(() => constraints.CreateUnique(new UniqueDescription {
                Name = "UQ_TEST_description",
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Columns =  {columnDescription, columnDescription2}
            }));
        }

        [Test]
        public void IfTargetTableDoesNotHaveDuplicatedValuesForAColumnReferrencedByTheUniqueKey_CreateMethodMustCreateTheUniqueKey() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](400) NULL,
                [description2] [nvarchar](400) NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id))");

            Database.ExecuteNonQuery(@"INSERT INTO [TESTS_PARILIS].[dbo].[TEST_TABLE] ([id], [description], [description2]) VALUES (1, 'test', 'test2');
                                       INSERT INTO [TESTS_PARILIS].[dbo].[TEST_TABLE] ([id], [description], [description2]) VALUES (2, 'test', 'test3');");

            var columnDescription2 = CreateColumnDescription("description2", "nvarchar", "400");

            constraints.CreateUnique(new UniqueDescription {
                Name = "UQ_TEST_description",
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Columns =  {columnDescription, columnDescription2}
            });

            var uniqueKeys = sqlServerDatabase.GetUniqueKeys(new TableDescription {Schema = "dbo", Name = "TEST_TABLE"});

            Assert.AreEqual(1, uniqueKeys.Count);
            Assert.AreEqual("UQ_TEST_description", uniqueKeys.Single().Name);
            Assert.AreEqual(2, uniqueKeys.Single().Columns.Count);
            Assert.AreEqual("description", uniqueKeys.Single().Columns.First().Name);
            Assert.AreEqual("description2", uniqueKeys.Single().Columns.Last().Name);
        }

        [Test]
        public void IfUniqueKeyDoesNotExist_RemoveMethodMustThrowException() {
            Assert.Throws<ConstraintNotFoundException>(() => constraints.RemoveUnique(new UniqueDescription {
                Name = "UQ_TEST_description",
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Columns =  {columnDescription}
            }));
        }

        [Test]
        public void IfUniqueKeyExistsAndIstReferencedByAnyOtherKey_RemoveMethodMustThrowException() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [bigint] NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id),
                CONSTRAINT UQ_TEST_description UNIQUE (description))");

            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE_2](
                [id] [bigint] NOT NULL,
                [id_fk] [bigint] NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_2_id PRIMARY KEY (id),
                CONSTRAINT FK_TEST FOREIGN KEY (id_fk) REFERENCES TEST_TABLE(description))");

            Assert.Throws<ReferencedConstraintException>(() => constraints.RemoveUnique(new UniqueDescription {
                Name = "UQ_TEST_description",
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Columns =  {columnDescription}
            }));
        }

        [Test]
        public void IfUniqueKeyExistsAndIsNotReferencedByAnyOtherKey_RemoveMethodRemoveTheUniqueKey() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [bigint] NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id),
                CONSTRAINT UQ_TEST_description UNIQUE (description))");

            constraints.RemoveUnique(new UniqueDescription {
                Name = "UQ_TEST_description",
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Columns =  {columnDescription}
            });

            var uniqueKey = sqlServerDatabase.GetUniqueKey("UQ_TEST_description", "dbo");

            Assert.IsNull(uniqueKey);
        }

        [Test]
        public void IfTargetTableHasQuotesInItsName_RemoveMethodRemoveTheUniqueKey() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST'TABLE](
                [id] [bigint] NOT NULL,
                [description] [bigint] NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id),
                CONSTRAINT UQ_TEST_description UNIQUE (description))");

            constraints.RemoveUnique(new UniqueDescription {
                Name = "UQ_TEST_description",
                Schema = "dbo",
                TableName = "TEST'TABLE",
                Columns =  { columnDescription }
            });

            var uniqueKey = sqlServerDatabase.GetUniqueKey("UQ_TEST_description", "dbo");

            Assert.IsNull(uniqueKey);
        }

        [Test]
        public void WhenDefaultValueDoesNotExist_CreateMethodMustCreateTheDefaultValue() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id));");

            var column = CreateColumnDescription("description");

            constraints.CreateDefault(new DefaultDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "DEFAULT_TEST_TABLE_description",
                Column = column,
                DefaultValue = "'test'"
            });

            var defaultValue = sqlServerDatabase.GetDefault("DEFAULT_TEST_TABLE_description", "dbo");

            Assert.IsNotNull(defaultValue);
        }

        [Test]
        public void WhenTargetTableHasQuotesInItsName_CreateMethodMustCreateTheDefaultValue() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST'TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id));");

            var column = CreateColumnDescription("description");

            constraints.CreateDefault(new DefaultDescription {
                Schema = "dbo",
                TableName = "TEST'TABLE",
                Name = "DEFAULT_TEST_TABLE_description",
                Column = column,
                DefaultValue = "'test'"
            });

            var defaultValue = sqlServerDatabase.GetDefault("DEFAULT_TEST_TABLE_description", "dbo");

            Assert.IsNotNull(defaultValue);
        }

        [Test]
        public void WhenTargetTableAlreadyHasADefaultValueSetForTheColumn_CreateMethodMustThrowException() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id));
                ALTER TABLE [dbo].[TEST_TABLE] ADD CONSTRAINT [DEFAULT_TEST_TABLE_description] DEFAULT 'test' FOR [description]");

            var column = CreateColumnDescription("description", "nvarchar", "max", false);

            Assert.Throws<InvalidReferenceColumnException>(() => constraints.CreateDefault(new DefaultDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "DEFAULT_TEST_TABLE_description_2",
                Column = column,
                DefaultValue = "'test'"
            }));
        }

        [Test]
        public void WhenThereIsAnotherDefaultValueWithTheSameNameInTheSameSchema_CreateMethodMustThrowException() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id));
                ALTER TABLE [dbo].[TEST_TABLE] ADD CONSTRAINT [DEFAULT_TEST_TABLE_description] DEFAULT 'test' FOR [description]");

            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE_2](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_2_id PRIMARY KEY (id));");

            Assert.Throws<InvalidConstraintNameException>(() => constraints.CreateDefault(new DefaultDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "DEFAULT_TEST_TABLE_description",
                Column = CreateColumnDescription(),
                DefaultValue = "'test'"
            }));
        }

        [Test]
        public void WhenThereIsAnotherDefaultValueWithTheSameNameInAnotherSchema_CreateMethodMustCreateTheDefaultValue() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id));
                ALTER TABLE [dbo].[TEST_TABLE] ADD CONSTRAINT [DEFAULT_TEST_TABLE_description] DEFAULT 'test' FOR [description]");

            Database.ExecuteNonQuery(@"CREATE TABLE [testschema].[TEST_TABLE_2](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_2_id PRIMARY KEY (id));");

            var column = CreateColumnDescription("description");

            constraints.CreateDefault(new DefaultDescription {
                Schema = "testschema",
                TableName = "TEST_TABLE_2",
                Name = "DEFAULT_TEST_TABLE_description",
                Column = column,
                DefaultValue = "'test'"
            });

            var defaultValue = sqlServerDatabase.GetDefault("DEFAULT_TEST_TABLE_description", "testschema");

            Assert.IsNotNull(defaultValue);
        }

        [Test]
        public void IfTargetColumnAlreadyHasValues_CreateMethodMustCreateTheDefaultValue() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id));");

            Database.ExecuteNonQuery(@"INSERT INTO [dbo].[TEST_TABLE] ([id], [description]) VALUES (1, 'test 1')");
            Database.ExecuteNonQuery(@"INSERT INTO [dbo].[TEST_TABLE] ([id], [description]) VALUES (2, 'test 2')");

            var column = CreateColumnDescription("description");

            constraints.CreateDefault(new DefaultDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "DEFAULT_TEST_TABLE_description",
                Column = column,
                DefaultValue = "'test 3'"
            });

            var defaultValue = sqlServerDatabase.GetDefault("DEFAULT_TEST_TABLE_description", "dbo");

            Assert.IsNotNull(defaultValue);
        }

        [Test]
        public void WhenDeafultValueExists_RemoveMethodMustRemoveTheDefaultValue() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id));
                ALTER TABLE [dbo].[TEST_TABLE] ADD CONSTRAINT [DEFAULT_TEST_TABLE_description] DEFAULT 'test' FOR [description]");

            constraints.RemoveDefault(new DefaultDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "DEFAULT_TEST_TABLE_description"
            });

            var defaultValue = sqlServerDatabase.GetDefault("DEFAULT_TEST_TABLE_description", "dbo");

            Assert.IsNull(defaultValue);
        }

        [Test]
        public void WhenTargetTableHasQuotesInItsName_RemoveMethodMustRemoveTheDefaultValue() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST'TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id));
                ALTER TABLE [dbo].[TEST'TABLE] ADD CONSTRAINT [DEFAULT_TEST_TABLE_description] DEFAULT 'test' FOR [description]");

            constraints.RemoveDefault(new DefaultDescription {
                Schema = "dbo",
                TableName = "TEST'TABLE",
                Name = "DEFAULT_TEST_TABLE_description"
            });

            var defaultValue = sqlServerDatabase.GetDefault("DEFAULT_TEST_TABLE_description", "dbo");

            Assert.IsNull(defaultValue);
        }

        [Test]
        public void WhenDeafultValueDoesNotExist_RemoveMethodMustThrowException() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id));");

            Assert.Throws<ConstraintNotFoundException>(() => constraints.RemoveDefault(new DefaultDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "DEFAULT_TEST_TABLE_description"
            }));
        }
    }
}