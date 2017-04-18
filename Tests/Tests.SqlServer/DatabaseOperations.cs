using System;
using System.IO;
using Microsoft.SqlServer.Management.Smo;

namespace Tests.SqlServer {
    public class DatabaseOperations {
        private Database database;

        public DatabaseOperations(Database database) {
            this.database = database;
            database.Tables.Refresh(true);
        }

        public void CreateSchema(string schemaName) {
            if (SchemaExists(schemaName))
                return;

            string query = $"CREATE SCHEMA {schemaName.ToLower()} AUTHORIZATION dbo;";
            database.ExecuteNonQuery(query);
        }

        public bool SchemaExists(string schemaName) {
            database.Schemas.Refresh(true);
            var schema = database.Schemas[schemaName.ToLower()];
            return schema != null;
        }

        public void CreateSchemaTablesFrom(string schemaName, string scriptsDirectory) {
            CreateSchema(schemaName);
            var pathsScripts = Directory.GetFiles(scriptsDirectory, $"{schemaName}.*.sql");

            var scriptForeignKeys = string.Empty;
            var scriptIndexes = string.Empty;

            foreach (var path in pathsScripts) {
                var script = new FileInfo(path);
                if (!script.Name.StartsWith(schemaName, StringComparison.InvariantCulture)) continue;
                if (script.Name.Contains("foreignkeys")) {
                    scriptForeignKeys = script.FullName;
                    continue;
                }
                if (script.Name.Contains("indexes")) {
                    scriptIndexes = script.FullName;
                    continue;
                }

                CreateTable(schemaName, script.Name.Split('.')[1], script.FullName);
            }

            if (!string.IsNullOrWhiteSpace(scriptForeignKeys)) {
                var query = File.ReadAllText(scriptForeignKeys);
                database.ExecuteNonQuery(query);
            }

            if (!string.IsNullOrWhiteSpace(scriptIndexes)) {
                var query = File.ReadAllText(scriptIndexes);
                database.ExecuteNonQuery(query);
            }
        }

        public void CreateFromFile(string scriptFilePath) {                       
            var script = File.ReadAllText(scriptFilePath);
            database.ExecuteNonQuery(script);
        }

        internal Table FindTable(string schemaName, string tableName) {            
            return database.Tables[tableName, schemaName];
        }

        public void CreateTable(string schemaName, string tableName, string scriptFilesPath) {
            var table = FindTable(schemaName, tableName);
            if (table != null)
                return;

            var query = string.Empty;

            if (File.Exists(scriptFilesPath))
                query = File.ReadAllText(scriptFilesPath);
            else if (File.Exists(Path.Combine(scriptFilesPath, $"{tableName}.sql")))
                query = File.ReadAllText(Path.Combine(scriptFilesPath, $"{tableName}.sql"));
            else if (File.Exists(Path.Combine(scriptFilesPath, $"{schemaName}.{tableName}.sql")))
                query = File.ReadAllText(Path.Combine(scriptFilesPath, $"{schemaName}.{tableName}.sql"));
            else
                query = $"CREATE TABLE [{schemaName}].[{tableName}]";

            database.ExecuteNonQuery(query);
        }

        public static Database Create(Server server, string databaseName) {
            server.Databases.Refresh(true);
            var database = server.Databases[databaseName];
            if (database != null)
                return database;

            database = new Database(server, databaseName) {Collation = "Latin1_General_CI_AI"};
            database.Create();
            new DatabaseOperations(database).CreateSchema(@"dbo");
            return database;            
        }

        public static void Drop(Server server, string databaseName) {
            var exists = server?.Databases[databaseName] != null;
            if(exists)
                server.KillDatabase(databaseName);
        }
    }
}