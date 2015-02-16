using System;
using Core.Descriptions;
using Core.Interfaces;

namespace SqlServer {
    public class Tables : ITable {
        public void Create(TableDescription tableDescription) {
            throw new NotImplementedException();
        }

        public void Remove(string schema, string tableName) {
            throw new NotImplementedException();
        }
    }
}