// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace ZMap.Source;

public abstract class SpatialDatabaseSource(string connectionString) : VectorSourceBase
{
    /// <summary>
    /// 连接字符串
    /// todo: 要换成 db, port, user, password 的配置方式
    /// 字符串太 Tec 了，后面做 UI 不合适
    /// </summary>
    public string ConnectionString { get; } = connectionString;

    // /// <summary>
    // /// 数据库名
    // /// </summary>
    // public string Database { get; } = database;

    /// <summary>
    /// 表名
    /// </summary>
    public string Table { get; set; }

    /// <summary>
    /// ID 列名
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// 图形列名
    /// </summary>
    public string Geometry { get; set; } = "geom";

    /// <summary>
    /// SQL 查询的 WHERE
    /// </summary>
    public string Where { get; set; }

    /// <summary>
    /// 数据列
    /// </summary>
    public HashSet<string> Properties { get; set; }

    public override string ToString()
    {
        return
            $"ConnectionString: {ConnectionString}, Table: {Table}, SRID: {Srid}, Geometry: {Geometry}";
    }
}