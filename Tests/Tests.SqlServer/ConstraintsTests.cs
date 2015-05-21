using System.Collections.Generic;
using System.Linq;
using Core;
using Core.Descriptions;
using Core.Exceptions;
using Core.Interfaces;
using NUnit.Framework;
using SqlServer;
using Tests.Core;

namespace Tests.SqlServer {
    [TestFixture]
    public class ConstraintsTests : DatabaseTest {
        private IConstraint constraints;
        private SqlServerDatabase sqlServerDatabase;

        [TestFixtureSetUp]
        public override void InitializeClass() {
            base.InitializeClass();
            constraints = Components.Instance.GetComponent<IConstraint>(ConnectionInfo);
            sqlServerDatabase = Components.Instance.GetComponent<IDatabase>(ConnectionInfo) as SqlServerDatabase;
            Database.ExecuteNonQuery(@"CREATE SCHEMA testschema");
        }

        [SetUp]
        public void InitializeTest() {
            Database.Tables.Refresh();
            var table = Database.Tables["TEST_TABLE_2"];
            if (table != null) table.Drop();

            table = Database.Tables["TEST_TABLE"];
            if (table != null) table.Drop();

            table = Database.Tables["TEST_TABLE_2", "testschema"];
            if (table != null) table.Drop();

            table = Database.Tables["TEST_TABLE", "testschema"];
            if (table != null) table.Drop();
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
                ColumnNames = new List<string> {"id"}
            };

            constraints.CreatePrimaryKey(primaryKey);

            var result = sqlServerDatabase.GetPrimaryKey(new TableDescription {Schema = "testschema", Name = "TEST_TABLE"});
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
                ColumnNames = new List<string> {"id"}
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
                ColumnNames = new List<string> {"id"}
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
                ColumnNames = new List<string> {"id"}
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
                ColumnNames = new List<string> {"id"}
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
                ColumnNames = new List<string> {"id"}
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
                ColumnNames = new List<string> {"id"}
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
                ColumnNames = new List<string> {"description"}
            });

            var uniqueKeys = sqlServerDatabase.GetUniqueKeys(new TableDescription {Schema = "dbo", Name = "TEST_TABLE"});

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
                ColumnNames = new List<string> {"description"}
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
                ColumnNames = new List<string> {"description"}
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

            Assert.Throws<InvalidReferenceColumnException>(() => constraints.CreateUnique(new UniqueDescription {
                Name = "UQ_TEST_description",
                Schema = "dbo",
                TableName = "TEST_TABLE",
                ColumnNames = new List<string> {"description2"}
            }));
        }

        [Test]
        public void WhenUniqueKeyReferencesMultipleColumns_CreateMethodMustCreateTheUniqueKey() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](400) NULL,
                [description2] [nvarchar](400) NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id))");

            constraints.CreateUnique(new UniqueDescription {
                Name = "UQ_TEST_description",
                Schema = "dbo",
                TableName = "TEST_TABLE",
                ColumnNames = new List<string> {"description", "description2"}
            });

            var uniqueKeys = sqlServerDatabase.GetUniqueKeys(new TableDescription {Schema = "dbo", Name = "TEST_TABLE"});

            Assert.AreEqual(1, uniqueKeys.Count);
            Assert.AreEqual("UQ_TEST_description", uniqueKeys.Single().Name);
            Assert.AreEqual(2, uniqueKeys.Single().ColumnNames.Count);
            Assert.AreEqual("description", uniqueKeys.Single().ColumnNames.First());
            Assert.AreEqual("description2", uniqueKeys.Single().ColumnNames.Last());
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
                ColumnNames = new List<string> {"description", "description"}
            }));
        }

        [Test]
        public void WhenUniqueKeysReferencesATableThatDoesNotExist_CreateMethodMustThrowException() {
            Assert.Throws<InvalidReferenceColumnException>(() => constraints.CreateUnique(new UniqueDescription {
                Name = "UQ_TEST_description",
                Schema = "dbo",
                TableName = "TEST_TABLE",
                ColumnNames = new List<string> {"description"}
            }));
        }

        [TestCase("[text]")]
        [TestCase("[ntext]")]
        [TestCase("[image]")]
        [TestCase("[xml]")]
        [TestCase("[geography]")]
        [TestCase("[geometry]")]
        [TestCase("[nvarchar](401)")]
        [TestCase("[varchar](901)")]
        public void WhenDataTypeOfUniqueKeyIsInvalid_CreateMethodMustThrowException(string type) {
            Database.ExecuteNonQuery(string.Format(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] {0} NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id))", type));

            Assert.Throws<InvalidReferenceColumnException>(() => constraints.CreateUnique(new UniqueDescription {
                Name = "UQ_TEST_description",
                Schema = "dbo",
                TableName = "TEST_TABLE",
                ColumnNames = new List<string> {"description"}
            }));
        }

        [Test]
        public void IfTargetTableHasDuplicatedValuesForAColumnReferrencedByTheUniqueKey_CreateMethodMustThrowException() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description1] [nvarchar](400) NULL,
                [description2] [nvarchar](400) NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id))");

            Database.ExecuteNonQuery(@"INSERT INTO [TESTS_PARILIS].[dbo].[TEST_TABLE] ([id], [description1], [description2]) VALUES (1, 'test', 'test2');
                                       INSERT INTO [TESTS_PARILIS].[dbo].[TEST_TABLE] ([id], [description1], [description2]) VALUES (2, 'test', 'test2');");

            Assert.Throws<InvalidReferenceColumnException>(() => constraints.CreateUnique(new UniqueDescription {
                Name = "UQ_TEST_description",
                Schema = "dbo",
                TableName = "TEST_TABLE",
                ColumnNames = new List<string> {"description1", "description2"}
            }));
        }

        [Test]
        public void IfTargetTableDoesNotHaveDuplicatedValuesForAColumnReferrencedByTheUniqueKey_CreateMethodMustCreateTheUniqueKey() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description1] [nvarchar](400) NULL,
                [description2] [nvarchar](400) NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id))");

            Database.ExecuteNonQuery(@"INSERT INTO [TESTS_PARILIS].[dbo].[TEST_TABLE] ([id], [description1], [description2]) VALUES (1, 'test', 'test2');
                                       INSERT INTO [TESTS_PARILIS].[dbo].[TEST_TABLE] ([id], [description1], [description2]) VALUES (2, 'test', 'test3');");

            constraints.CreateUnique(new UniqueDescription {
                Name = "UQ_TEST_description",
                Schema = "dbo",
                TableName = "TEST_TABLE",
                ColumnNames = new List<string> {"description1", "description2"}
            });

            var uniqueKeys = sqlServerDatabase.GetUniqueKeys(new TableDescription {Schema = "dbo", Name = "TEST_TABLE"});

            Assert.AreEqual(1, uniqueKeys.Count);
            Assert.AreEqual("UQ_TEST_description", uniqueKeys.Single().Name);
            Assert.AreEqual(2, uniqueKeys.Single().ColumnNames.Count);
            Assert.AreEqual("description1", uniqueKeys.Single().ColumnNames.First());
            Assert.AreEqual("description2", uniqueKeys.Single().ColumnNames.Last());
        }

        [Test]
        public void IfUniqueKeyDoesNotExist_RemoveMethodMustThrowException() {
            Assert.Throws<ConstraintNotFoundException>(() => constraints.RemoveUnique(new UniqueDescription {
                Name = "UQ_TEST_description",
                Schema = "dbo",
                TableName = "TEST_TABLE",
                ColumnNames = new List<string> {"description"}
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
                ColumnNames = new List<string> {"description"}
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
                ColumnNames = new List<string> {"description"}
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