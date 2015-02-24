using System.ComponentModel;
using SqlServer.Attributes;

namespace SqlServer.Enums {
    public enum DataType {
        [DefaultValue("bigint")]
        [AllowsLength(false)]
        BigInt,

        [DefaultValue("numeric")]
        [AllowsLength(false)]
        Numeric,

        [DefaultValue("bit")]
        [AllowsLength(false)]
        Bit,

        [DefaultValue("smallint")]
        [AllowsLength(false)]
        Smallint,

        [DefaultValue("decimal")]
        [AllowsLength(false)]
        Decimal,

        [DefaultValue("smallmoney")]
        [AllowsLength(false)]
        Smallmoney,

        [DefaultValue("int")]
        [AllowsLength(false)]
        Int,

        [DefaultValue("tinyint")]
        [AllowsLength(false)]
        Tinyint,

        [DefaultValue("money")]
        [AllowsLength(false)]
        Money,

        [DefaultValue("float")]
        [AllowsLength(true, 1, 53, false)]
        Float,

        [DefaultValue("real")]
        [AllowsLength(true, 1, 53, false)]
        Real,

        [DefaultValue("date")]
        [AllowsLength(false)]
        Date,

        [DefaultValue("datetimeoffset")]
        [AllowsLength(false)]
        Datetimeoffset,

        [DefaultValue("datetime2")]
        [AllowsLength(false)]
        Datetime2,

        [DefaultValue("smalldatetime")]
        [AllowsLength(false)]
        Smalldatetime,

        [DefaultValue("datetime")]
        [AllowsLength(false)]
        Datetime,

        [DefaultValue("time")]
        [AllowsLength(false)]
        Time,

        [DefaultValue("char")]
        [AllowsLength(true, 1, 8000, false)]
        Char,

        [DefaultValue("varchar")]
        [AllowsLength(true, 1, 8000, true)]
        Varchar,

        [DefaultValue("text")]
        [AllowsLength(false)]
        Text,

        [DefaultValue("nchar")]
        [AllowsLength(true, 1, 4000, false)]
        Nchar,

        [DefaultValue("nvarchar")]
        [AllowsLength(true, 1, 4000, true)]
        Nvarchar,

        [DefaultValue("ntext")]
        [AllowsLength(false)]
        Ntext,

        [DefaultValue("binary")]
        [AllowsLength(true, 1, 8000, false)]
        Binary,

        [DefaultValue("varbinary")]
        [AllowsLength(true, 1, 8000, true)]
        Varbinary,

        [DefaultValue("image")]
        [AllowsLength(false)]
        Image,

        [DefaultValue("cursor")]
        [AllowsLength(false)]
        Cursor,

        [DefaultValue("timestamp")]
        [AllowsLength(false)]
        Timestamp,

        [DefaultValue("hierarchyid")]
        [AllowsLength(false)]
        Hierarchyid,

        [DefaultValue("uniqueidentifier")]
        [AllowsLength(false)]
        Uniqueidentifier,

        [DefaultValue("sql_variant")]
        [AllowsLength(false)]
        SqlVariant,

        [DefaultValue("xml")]
        [AllowsLength(false)]
        Xml,

        [DefaultValue("table")]
        [AllowsLength(false)]
        Table,

        [DefaultValue("geography")]
        [AllowsLength(false)]
        Geography,

        [DefaultValue("geometry")]
        [AllowsLength(false)]
        Geometry
    }
}