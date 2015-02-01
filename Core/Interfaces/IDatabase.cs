using System.Collections.Generic;
using Core.Descriptions;

namespace Core.Interfaces {
    public interface IDatabase {
        PrimaryKeyDescription GetPrimaryKey(TableDescription table);
        IList<ForeignKeyDescription> GetForeignKeysReferencing(ConstraintDescription primaryKeyDescription);
    }
}
