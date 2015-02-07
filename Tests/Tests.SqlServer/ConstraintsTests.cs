﻿using System;
using System.Collections.Generic;
using System.Linq;
using Core.Descriptions;
using Core.Exceptions;
using NUnit.Framework;
using SqlServer;
using Tests.Core;

namespace Tests.SqlServer {
    [TestFixture]
    public class ConstraintsTests : DatabaseTest {
        [TestFixtureSetUp]
        public override void InitializeClass() {
            base.InitializeClass();
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
        public void WhenTargetTableDoesNotHaveAPrimaryKey_CreateMethodMustCreateThePrimaryKey() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NULL)");

            var primaryKey = new PrimaryKeyDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE",
                Name = "PK_dbo_TEST_TABLE",
                ColumnNames = new List<string> {"id"}
            };

            var constraints = new Constraints(Database);
            constraints.CreatePrimaryKey(primaryKey);

            var result = new SqlServerDatabase(Database).GetPrimaryKey(new TableDescription {Schema = "dbo", Name = "TEST_TABLE"});
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

            var constraints = new Constraints(Database);
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

            var constraints = new Constraints(Database);
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

            var constraints = new Constraints(Database);
            constraints.CreatePrimaryKey(primaryKey);

            var result = new SqlServerDatabase(Database).GetPrimaryKey(new TableDescription {Schema = "dbo", Name = "TEST_TABLE"});
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

            var constraints = new Constraints(Database);
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

            var constraints = new Constraints(Database);
            constraints.RemovePrimaryKey(primaryKey);

            var sqlServerDatabase = new SqlServerDatabase(Database);
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

            var constraints = new Constraints(Database);
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

            var constraints = new Constraints(Database);
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

            var sqlServerDatabase = new SqlServerDatabase(Database);
            var foreignKeys = sqlServerDatabase.GetForeignKeys(new TableDescription {Schema = "dbo", Name = "TEST_TABLE_2"});

            Assert.AreEqual(1, foreignKeys.Count);
            Assert.AreEqual("FK_TEST", foreignKeys.Single().Name);
        }

        [Test]
        public void WhenTargetTableAlreadyHasAForeignKeyWithTheSameName_CreateMethodMustThrowException() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id))");

            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE_2](
                [id] [bigint] NOT NULL,
                [id_fk] [bigint] NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_2_id PRIMARY KEY (id),
                CONSTRAINT FK_TEST FOREIGN KEY (id_fk) REFERENCES TEST_TABLE(id))");

            var constraints = new Constraints(Database);
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
        public void WhenForeignKeysReferencesAnInvalidColumn_CreateMethodMustThrowException() {
            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE](
                [id] [bigint] NOT NULL,
                [description] [nvarchar](max) NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id))");

            Database.ExecuteNonQuery(@"CREATE TABLE [dbo].[TEST_TABLE_2](
                [id] [bigint] NOT NULL,
                [id_fk] [bigint] NOT NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_2_id PRIMARY KEY (id))");

            var constraints = new Constraints(Database);
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

            var constraints = new Constraints(Database);
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

            var sqlServerDatabase = new SqlServerDatabase(Database);
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

            var constraints = new Constraints(Database);
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

            var constraints = new Constraints(Database);
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

            var constraints = new Constraints(Database);
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

            var constraints = new Constraints(Database);
            constraints.RemoveForeignKey(new ForeignKeyDescription {
                Schema = "dbo",
                TableName = "TEST_TABLE_2",
                Name = "FK_TEST"
            });

            var sqlServerDatabase = new SqlServerDatabase(Database);
            var foreignKeys = sqlServerDatabase.GetForeignKeys(new TableDescription { Schema = "dbo", Name = "TEST_TABLE_2" });

            Assert.AreEqual(0, foreignKeys.Count);
        }

        [Test]
        public void WhenForeignKeysTableDoesNotExist_RemoveMethodMustThrowException() {
            var constraints = new Constraints(Database);
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

            var constraints = new Constraints(Database);
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
                [description] [nvarchar](4000) NULL,
                CONSTRAINT PK_dbo_TEST_TABLE_id PRIMARY KEY (id),
                CONSTRAINT UQ_TEST_description UNIQUE (description))");

            var constraints = new Constraints(Database);
            constraints.CreateUnique(new UniqueDescription {
                Name = "UQ_TEST_description",
                Schema = "dbo",
                TableName = "TEST_TABLE",
                ColumnNames = new List<string> {"description"}
            });

            var sqlServerDatabase = new SqlServerDatabase(Database);
            var uniqueKeys = sqlServerDatabase.GetUniqueKeys(new TableDescription {Schema = "dbo", Name = "TEST_TABLE"});

            Assert.AreEqual(1, uniqueKeys.Count);
            Assert.AreEqual("UQ_TEST_description", uniqueKeys.Single().Name);
        }

        [Test]
        public void WhenDataTypeOfUniqueKeyIsInvalid_CreateMethodMustThrowException() {
            Assert.Inconclusive("Escrever teste.");
        }

        [Test]
        public void WhenThereIsAnotherUniqueKeyWithTheSameNameInTheSameSchemaHas_CreateMethodMustThrowException() {
            Assert.Inconclusive("Escrever teste.");
        }

        [Test]
        public void WhenUniqueKeysReferencesAnInvalidColumn_CreateMethodMustThrowException() {
            Assert.Inconclusive("Escrever teste.");
        }

        [Test]
        public void WhenUniqueKeyReferencesMultipleColumns_CreateMethodMustCreateTheUniqueKey() {
            Assert.Inconclusive("Escrever teste.");
        }

        [Test]
        public void WhenUniqueKeyReferencesTheSameColumnTwice_CreateMethodMustThrowException() {
            Assert.Inconclusive("Escrever teste.");
        }

        [Test]
        public void WhenUniqueKeysReferencesAColumnThatDoesNotExist_CreateMethodMustThrowException() {
            Assert.Inconclusive("Escrever teste.");
        }

        [Test]
        public void WhenUniqueKeysReferencesATableThatDoesNotExist_CreateMethodMustThrowException() {
            Assert.Inconclusive("Escrever teste.");
        }

        //TODO: If a UNIQUE constraint is added to a column that has duplicated values, the Database Engine returns an error and does not add the constraint.

        //TODO: Testar remoção: A UNIQUE constraint can be referenced by a FOREIGN KEY constraint.
    }
}