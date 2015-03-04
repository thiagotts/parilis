using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Core.Descriptions;
using Core.Interfaces;
using NSubstitute;
using NUnit.Framework;

namespace Tests.Core{
    [TestFixture]
    public class FilterBySchemaTests : MockTest{
        
        [SetUp]
        public void InitializeTest(){
            IDatabase database = Components.Instance.GetComponent<IDatabase>(new ConnectionInfo());
            database.GetSchemas().Returns(new List<string>(), new List<string>());
            database.GetTables().Returns(new List<TableDescription>(), new List<TableDescription>());
            database.GetIndexes().Returns(new List<IndexDescription>(), new List<IndexDescription>());
            database.GetPrimaryKeys().Returns(new List<PrimaryKeyDescription>(), new List<PrimaryKeyDescription>());
            database.GetForeignKeys().Returns(new List<ForeignKeyDescription>(), new List<ForeignKeyDescription>());
            database.GetUniqueKeys().Returns(new List<UniqueDescription>(), new List<UniqueDescription>());
            database.GetDefaults().Returns(new List<DefaultDescription>(), new List<DefaultDescription>());
            actualDatabase = new DatabaseDescription(new ConnectionInfo());
        }

        private DatabaseDescription actualDatabase;

        [TestFixtureSetUp]
        public override void InitializeClass(){
            base.InitializeClass();
            Mock<ITable>();
            Mock<IConstraint>();
            Mock<IIndex>();
            Mock<IColumn>();
            Mock<ISchema>();            
        }
        
        [Test]
        public void WhenNoPassAnyArgument_MustReturnTheOwnDatabaseDescription(){
            var databaseDescriptionFiltered = actualDatabase.FilterBySchema();

            Assert.That(databaseDescriptionFiltered, Is.SameAs(actualDatabase));
            foreach (var schema in actualDatabase.Schemas){
                Assert.That(databaseDescriptionFiltered.Schemas, Contains.Item(schema));    
            }           
        }

        [Test]
        public void WhenPassAValueWithoutCorrespondency_MustReturnADatabaseDescriptionEmpty(){
            var databaseDescriptionFiltered = actualDatabase.FilterBySchema("aaa");

            Assert.That(databaseDescriptionFiltered.Defaults, Is.Empty);
            Assert.That(databaseDescriptionFiltered.ForeignKeys, Is.Empty);
            Assert.That(databaseDescriptionFiltered.Indexes, Is.Empty);
            Assert.That(databaseDescriptionFiltered.PrimaryKeys, Is.Empty);
            Assert.That(databaseDescriptionFiltered.Schemas, Is.Empty);
            Assert.That(databaseDescriptionFiltered.Tables, Is.Empty);
            Assert.That(databaseDescriptionFiltered.UniqueKeys, Is.Empty);            
        }
        
        [TestCase(null)]
        [TestCase("")]
        public void WhenPassNullOrEmptySchema_MustReturnADatabaseDescriptionEmpty(string nullOrEmptySchema){
            var databaseDescriptionFiltered = actualDatabase.FilterBySchema(nullOrEmptySchema);
            Assert.That(databaseDescriptionFiltered.IsEmpty);
        }

        [Test]
        public void WhenPassAnExistentSchema_MustReturnOnlyElementsThatMatchesWith(){
            string dboSchema = "dbo";
            string otherSchema = "other";

            actualDatabase.Schemas.Add(dboSchema);
            actualDatabase.Tables.Add(new TableDescription { Schema = dboSchema });
            actualDatabase.Defaults.Add(new DefaultDescription { Schema = dboSchema });
            actualDatabase.PrimaryKeys.Add(new PrimaryKeyDescription { Schema = dboSchema });
            actualDatabase.Indexes.Add(new IndexDescription { Schema = dboSchema });
            actualDatabase.UniqueKeys.Add(new UniqueDescription { Schema = dboSchema });
            actualDatabase.ForeignKeys.Add(new ForeignKeyDescription { Schema = dboSchema });

            actualDatabase.Schemas.Add(otherSchema);
            actualDatabase.Tables.Add(new TableDescription { Schema = otherSchema });
            actualDatabase.Defaults.Add(new DefaultDescription { Schema = otherSchema });
            actualDatabase.PrimaryKeys.Add(new PrimaryKeyDescription { Schema = otherSchema });
            actualDatabase.Indexes.Add(new IndexDescription { Schema = otherSchema });
            actualDatabase.UniqueKeys.Add(new UniqueDescription { Schema = otherSchema });
            actualDatabase.ForeignKeys.Add(new ForeignKeyDescription { Schema = otherSchema });

            var databaseDescriptionFiltered = actualDatabase.FilterBySchema(dboSchema);

            Assert.That(databaseDescriptionFiltered.IsEmpty, Is.False);
            
            Assert.That(databaseDescriptionFiltered.Schemas, Contains.Item(dboSchema));            
            Assert.That(databaseDescriptionFiltered.Tables.All(table=>table.Schema.Equals(dboSchema, StringComparison.InvariantCultureIgnoreCase)));            
            Assert.That(databaseDescriptionFiltered.Defaults.All(@default=>@default.Schema.Equals(dboSchema, StringComparison.InvariantCultureIgnoreCase)));            
            Assert.That(databaseDescriptionFiltered.Indexes.All(index=>index.Schema.Equals(dboSchema, StringComparison.InvariantCultureIgnoreCase)));            
            Assert.That(databaseDescriptionFiltered.PrimaryKeys.All(pk=>pk.Schema.Equals(dboSchema, StringComparison.InvariantCultureIgnoreCase)));            
            Assert.That(databaseDescriptionFiltered.ForeignKeys.All(fk=>fk.Schema.Equals(dboSchema, StringComparison.InvariantCultureIgnoreCase)));            
            Assert.That(databaseDescriptionFiltered.UniqueKeys.All(unique=>unique.Schema.Equals(dboSchema, StringComparison.InvariantCultureIgnoreCase)));          
        }
        
        [Test]
        public void WhenPassManyExistentSchemas_MustReturnOnlyElementsThatMatchesWith(){
            string dboSchema = "dbo";
            string otherSchema = "other";
            string otherSchema1 = "other1";

            actualDatabase.Schemas.Add(dboSchema);
            actualDatabase.Tables.Add(new TableDescription { Schema = dboSchema });
            actualDatabase.Defaults.Add(new DefaultDescription { Schema = dboSchema });
            actualDatabase.PrimaryKeys.Add(new PrimaryKeyDescription { Schema = dboSchema });
            actualDatabase.Indexes.Add(new IndexDescription { Schema = dboSchema });
            actualDatabase.UniqueKeys.Add(new UniqueDescription { Schema = dboSchema });
            actualDatabase.ForeignKeys.Add(new ForeignKeyDescription { Schema = dboSchema });

            actualDatabase.Schemas.Add(otherSchema);
            actualDatabase.Tables.Add(new TableDescription { Schema = otherSchema });
            actualDatabase.Defaults.Add(new DefaultDescription { Schema = otherSchema });
            actualDatabase.PrimaryKeys.Add(new PrimaryKeyDescription { Schema = otherSchema });
            actualDatabase.Indexes.Add(new IndexDescription { Schema = otherSchema });
            actualDatabase.UniqueKeys.Add(new UniqueDescription { Schema = otherSchema });
            actualDatabase.ForeignKeys.Add(new ForeignKeyDescription { Schema = otherSchema });

            actualDatabase.Schemas.Add(otherSchema1);
            actualDatabase.Tables.Add(new TableDescription { Schema = otherSchema1 });
            actualDatabase.Defaults.Add(new DefaultDescription { Schema = otherSchema1 });
            actualDatabase.PrimaryKeys.Add(new PrimaryKeyDescription { Schema = otherSchema1 });
            actualDatabase.Indexes.Add(new IndexDescription { Schema = otherSchema1 });
            actualDatabase.UniqueKeys.Add(new UniqueDescription { Schema = otherSchema1 });
            actualDatabase.ForeignKeys.Add(new ForeignKeyDescription { Schema = otherSchema1 });

            var databaseDescriptionFiltered = actualDatabase.FilterBySchema(dboSchema, otherSchema);

            Assert.That(databaseDescriptionFiltered.IsEmpty, Is.False);
            
            Assert.That(databaseDescriptionFiltered.Schemas, Contains.Item(dboSchema));            
            Assert.That(databaseDescriptionFiltered.Tables.Any(table=>table.Schema.Equals(dboSchema, StringComparison.InvariantCultureIgnoreCase)));
            Assert.That(databaseDescriptionFiltered.Defaults.Any(@default => @default.Schema.Equals(dboSchema, StringComparison.InvariantCultureIgnoreCase)));
            Assert.That(databaseDescriptionFiltered.Indexes.Any(index => index.Schema.Equals(dboSchema, StringComparison.InvariantCultureIgnoreCase)));
            Assert.That(databaseDescriptionFiltered.PrimaryKeys.Any(pk => pk.Schema.Equals(dboSchema, StringComparison.InvariantCultureIgnoreCase)));
            Assert.That(databaseDescriptionFiltered.ForeignKeys.Any(fk => fk.Schema.Equals(dboSchema, StringComparison.InvariantCultureIgnoreCase)));
            Assert.That(databaseDescriptionFiltered.UniqueKeys.Any(unique => unique.Schema.Equals(dboSchema, StringComparison.InvariantCultureIgnoreCase)));            
            
            Assert.That(databaseDescriptionFiltered.Schemas, Contains.Item(otherSchema));
            Assert.That(databaseDescriptionFiltered.Tables.Any(table => table.Schema.Equals(otherSchema, StringComparison.InvariantCultureIgnoreCase)));
            Assert.That(databaseDescriptionFiltered.Defaults.Any(@default => @default.Schema.Equals(otherSchema, StringComparison.InvariantCultureIgnoreCase)));
            Assert.That(databaseDescriptionFiltered.Indexes.Any(index => index.Schema.Equals(otherSchema, StringComparison.InvariantCultureIgnoreCase)));
            Assert.That(databaseDescriptionFiltered.PrimaryKeys.Any(pk => pk.Schema.Equals(otherSchema, StringComparison.InvariantCultureIgnoreCase)));
            Assert.That(databaseDescriptionFiltered.ForeignKeys.Any(fk => fk.Schema.Equals(otherSchema, StringComparison.InvariantCultureIgnoreCase)));
            Assert.That(databaseDescriptionFiltered.UniqueKeys.Any(unique => unique.Schema.Equals(otherSchema, StringComparison.InvariantCultureIgnoreCase)));

            Assert.That(databaseDescriptionFiltered.Schemas.Contains(otherSchema1), Is.False);
            Assert.That(databaseDescriptionFiltered.Tables.Any(table => table.Schema.Equals(otherSchema1, StringComparison.InvariantCultureIgnoreCase)), Is.False);
            Assert.That(databaseDescriptionFiltered.Defaults.Any(@default => @default.Schema.Equals(otherSchema1, StringComparison.InvariantCultureIgnoreCase)), Is.False);
            Assert.That(databaseDescriptionFiltered.Indexes.Any(index => index.Schema.Equals(otherSchema1, StringComparison.InvariantCultureIgnoreCase)), Is.False);
            Assert.That(databaseDescriptionFiltered.PrimaryKeys.Any(pk => pk.Schema.Equals(otherSchema1, StringComparison.InvariantCultureIgnoreCase)), Is.False);
            Assert.That(databaseDescriptionFiltered.ForeignKeys.Any(fk => fk.Schema.Equals(otherSchema1, StringComparison.InvariantCultureIgnoreCase)), Is.False);
            Assert.That(databaseDescriptionFiltered.UniqueKeys.Any(unique => unique.Schema.Equals(otherSchema1, StringComparison.InvariantCultureIgnoreCase)), Is.False);            
        }
    }
}