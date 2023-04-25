using System.Collections.Generic;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace ZMap.Source
{
    public abstract class SpatialDatabaseSource : VectorSourceBase
    {
        /// <summary>
        /// 连接字符串
        /// todo: 要换成 db, port, user, password 的配置方式
        /// 字符串太 Tec 了，后面做 UI 不合适
        /// </summary>
        public string ConnectionString { get; }

        /// <summary>
        /// 数据库名
        /// </summary>
        public string Database { get; }

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
        public string Geometry { get; set; }
        
        /// <summary>
        /// SQL 查询的 WHERE
        /// </summary>
        public string Where { get; set; }

        /// <summary>
        /// 数据列
        /// </summary>
        public HashSet<string> Properties { get; set; }

        protected SpatialDatabaseSource(string connectionString, string database)
        {
            ConnectionString = connectionString;
            Database = database;
        }

        public override string ToString()
        {
            return
                $"ConnectionString: {ConnectionString}, Database: {Database}, Table: {Table}, SRID: {SRID}, Geometry: {Geometry}";
        }
    }
}