using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using Npgsql;
using ZMap.Infrastructure;

namespace ZMap.Source.Postgre
{
    public sealed class PostgreSource : SpatialDatabaseSource
    {
        // private static readonly ConcurrentDictionary<string, DbContext> DbContexts =
        //     new ConcurrentDictionary<string, DbContext>();

        public PostgreSource(string connectionString, string database) : base(connectionString,
            database)
        {
            // DbContexts.GetOrAdd($"{database}_{Table}", (key) => new BridgeDbContext(connectionString, database, Table));
        }

        public override async Task<IEnumerable<Feature>> GetFeaturesInExtentAsync(Envelope bbox)
        {
            if (string.IsNullOrWhiteSpace(Geometry))
            {
                throw new ArgumentException("未设置图形在数据库中的列名");
            }

            // todo: Filter 怎么才能保证没有 SQL 注入: 不真接暴露地图接口，由后端服务构造请求
            var filter = Filter?.ToQuerySql();
            var where = string.IsNullOrWhiteSpace(filter) ? string.Empty : $"{filter} AND ";

            // todo: 使用 PG 的 Simplify 达不到效果, 需要继续研究
            // sql =
            //     $"SELECT CASE WHEN ST_HasArc({Geometry}) THEN {Geometry} ELSE ST_Simplify(ST_Force2D({Geometry}), 0.00001, true) END as geom{columnSql} from (SELECT {Geometry} as geom{columnSql} FROM {Table} WHERE {@where} {Geometry} && ST_MakeEnvelope({bbox.MinX}, {bbox.MinY},{bbox.MaxX},{bbox.MaxY}, {SRID})) t";

            string sql;
            if (Properties == null || Properties.Count == 0)
            {
                sql =
                    $"SELECT * FROM {Table} WHERE {where} {Geometry} && ST_MakeEnvelope({bbox.MinX}, {bbox.MinY}, {bbox.MaxX}, {bbox.MaxY}, {SRID}) AND {(string.IsNullOrWhiteSpace(Where) ? "1 = 1" : Where)}";
            }
            else
            {
                var columnSql = Properties == null || Properties.Count == 0
                    ? string.Empty
                    : $" , {string.Join(", ", Properties.Where(x => x != "geom"))}";

                sql =
                    $"SELECT {Geometry} as geom{columnSql} FROM {Table} WHERE {where} {Geometry} && ST_MakeEnvelope({bbox.MinX}, {bbox.MinY}, {bbox.MaxX}, {bbox.MaxY}, {SRID}) AND {(string.IsNullOrWhiteSpace(Where) ? "1 = 1" : Where)}";
            }

            if (EnvironmentVariables.EnableSensitiveDataLogging)
            {
                Log.Logger.LogInformation("{Sql}", sql);
            }

            using var conn = CreateDbConnection();

            return (await conn.QueryAsync(sql, null, null, 10)).Select(x =>
            {
                var f = new Feature(Geometry, x);
                if (f.Geometry.SRID != -1)
                {
                    return f;
                }

                if (f.Geometry is GeometryCollection geometryCollection)
                {
                    foreach (var geom in geometryCollection)
                    {
                        geom.SRID = SRID;
                    }
                }
                else
                {
                    f.Geometry.SRID = SRID;
                }

                return f;
            });
        }

        public override Envelope GetEnvelope()
        {
            return null;
        }

        public override void Dispose()
        {
        }

        private IDbConnection CreateDbConnection()
        {
            var builder = new NpgsqlConnectionStringBuilder(ConnectionString)
                { Database = Database };
            var connectionString = builder.ToString();
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
            dataSourceBuilder.UseNetTopologySuite();
            var dataSource = dataSourceBuilder.Build();
            return dataSource.CreateConnection();
        }
    }
}