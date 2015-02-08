using System.Collections.Generic;
using Core.Descriptions;

namespace Core.Interfaces {
    public interface IDatabase {
        PrimaryKeyDescription GetPrimaryKey(TableDescription table);
        IList<ForeignKeyDescription> GetForeignKeysReferencing(ConstraintDescription primaryKeyDescription);
        IList<ForeignKeyDescription> GetForeignKeys(TableDescription tableDescription);
        IList<UniqueDescription> GetUniqueKeys(TableDescription tableDescription);
        UniqueDescription GetUniqueKey(string uniqueKeyName);
    }
}
