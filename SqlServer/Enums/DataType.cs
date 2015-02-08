using System.ComponentModel;

namespace SqlServer.Enums {
    public enum DataType {
        [DefaultValue("bigint")]
        BigInt,

        [DefaultValue("numeric")]
        Numeric,

        [DefaultValue("bit")]
        Bit,

        [DefaultValue("smallint")]
        Smallint,

        [DefaultValue("decimal")]
        Decimal,

        [DefaultValue("smallmoney")]
        Smallmoney,

        [DefaultValue("int")]
        Int,

        [DefaultValue("tinyint")]
        Tinyint,

        [DefaultValue("money")]
        Money,

        [DefaultValue("float")]
        Float,

        [DefaultValue("real")]
        Real,

        [DefaultValue("date")]
        Date,

        [DefaultValue("datetimeoffset")]
        Datetimeoffset,

        [DefaultValue("datetime2")]
        Datetime2,

        [DefaultValue("smalldatetime")]
        Smalldatetime,

        [DefaultValue("datetime")]
        Datetime,

        [DefaultValue("time")]
        Time,

        [DefaultValue("char")]
        Char,

        [DefaultValue("varchar")]
        Varchar,

        [DefaultValue("text")]
        Text,

        [DefaultValue("nchar")]
        Nchar,

        [DefaultValue("nvarchar")]
        Nvarchar,

        [DefaultValue("ntext")]
        Ntext,

        [DefaultValue("binary")]
        Binary,

        [DefaultValue("varbinary")]
        Varbinary,

        [DefaultValue("image")]
        Image,

        [DefaultValue("cursor")]
        Cursor,

        [DefaultValue("timestamp")]
        Timestamp,

        [DefaultValue("hierarchyid")]
        Hierarchyid,

        [DefaultValue("uniqueidentifier")]
        Uniqueidentifier,

        [DefaultValue("sql_variant")]
        SqlVariant,

        [DefaultValue("xml")]
        Xml,

        [DefaultValue("table")]
        Table,

        [DefaultValue("geography")]
        Geography,

        [DefaultValue("geometry")]
        Geometry 
    }
}