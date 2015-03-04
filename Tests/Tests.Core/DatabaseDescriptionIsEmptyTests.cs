using System.Collections.Generic;
using Core;
using Core.Descriptions;
using Core.Interfaces;
using NSubstitute;
using NUnit.Framework;

namespace Tests.Core{
    [TestFixture]
    public class DatabaseDescriptionIsEmptyTests : MockTest{

        private DatabaseDescription databaseDescription;
        
        [TestFixtureSetUp]
        public override void InitializeClass(){
            base.InitializeClass();
            Mock<ITable>();
            Mock<IConstraint>();
            Mock<IIndex>();
            Mock<IColumn>();
            Mock<ISchema>();

        }

        [SetUp]
        public void InitializeTest(){
            var database = Components.Instance.GetComponent<IDatabase>(new ConnectionInfo());
            database.GetSchemas().Returns(new List<string>(), new List<string>());
            database.GetTables().Returns(new List<TableDescription>(), new List<TableDescription>());
            database.GetIndexes().Returns(new List<IndexDescription>(), new List<IndexDescription>());
            database.GetPrimaryKeys().Returns(new List<PrimaryKeyDescription>(), new List<PrimaryKeyDescription>());
            database.GetForeignKeys().Returns(new List<ForeignKeyDescription>(), new List<ForeignKeyDescription>());
            database.GetUniqueKeys().Returns(new List<UniqueDescription>(), new List<UniqueDescription>());
            database.GetDefaults().Returns(new List<DefaultDescription>(), new List<DefaultDescription>());

            databaseDescription = new DatabaseDescription(new ConnectionInfo());
        }

        [Test]
        public void WhenAllElementsAreNullOrEmpty_MustReturnTrue(){
            Assert.That(databaseDescription.IsEmpty);
        }
        
        [Test]
        public void WhenDefaultsHasAnyElement_MustReturnFalse(){
            databaseDescription.Defaults.Add(new DefaultDescription());
            Assert.That(databaseDescription.IsEmpty, Is.False);            
        }
        
        [Test]
        public void WhenSchemasHasAnyElement_MustReturnFalse(){
            databaseDescription.Schemas.Add("xpto");
            Assert.That(databaseDescription.IsEmpty, Is.False);            
        }
        
        [Test]
        public void WhenTablesHasAnyElement_MustReturnFalse(){
            databaseDescription.Tables.Add(new TableDescription ());
            Assert.That(databaseDescription.IsEmpty, Is.False);            
        }
        
        [Test]
        public void WhenIndexesHasAnyElement_MustReturnFalse(){
            databaseDescription.Indexes.Add(new IndexDescription ());
            Assert.That(databaseDescription.IsEmpty, Is.False);            
        }
        
        [Test]
        public void WhenPrimaryKeysHasAnyElement_MustReturnFalse(){
            databaseDescription.PrimaryKeys.Add(new PrimaryKeyDescription());
            Assert.That(databaseDescription.IsEmpty, Is.False);            
        }
        
        [Test]
        public void WhenForeignKeysHasAnyElement_MustReturnFalse(){
            databaseDescription.ForeignKeys.Add(new ForeignKeyDescription());
            Assert.That(databaseDescription.IsEmpty, Is.False);            
        }
        
        [Test]
        public void WhenUniqueKeysHasAnyElement_MustReturnFalse(){
            databaseDescription.UniqueKeys.Add(new UniqueDescription());
            Assert.That(databaseDescription.IsEmpty, Is.False);            
        }
        
    }
}