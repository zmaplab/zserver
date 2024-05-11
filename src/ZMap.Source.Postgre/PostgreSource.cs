using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using FreeSql.Internal.Model;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using Npgsql;
using ZMap.Infrastructure;
using DataType = FreeSql.DataType;

namespace ZMap.Source.Postgre;

public sealed class PostgreSource(string connectionString) : SpatialDatabaseSource(connectionString)
{
    private static readonly ILogger Logger = Log.CreateLogger<PostgreSource>();

    private static readonly Lazy<IFreeSql> FreeSql = new(() =>
    {
        return new FreeSql.FreeSqlBuilder()
            .UseConnectionFactory(DataType.PostgreSQL, () =>
            {
                var dataSourceBuilder = new NpgsqlDataSourceBuilder(
                    "User ID=postgres;Password=11111;Host=127.0.0.1;Port=8888;Database=zserver_dev;Pooling=true;");
                dataSourceBuilder.UseNetTopologySuite();
                var dataSource = dataSourceBuilder.Build();
                return dataSource.CreateConnection();
            })
            .Build();
    });

    private static ConcurrentDictionary<string, string> BaseSql = new ConcurrentDictionary<string, string>();

    public override async Task<IEnumerable<Feature>> GetFeaturesInExtentAsync(Envelope bbox)
    {
        if (string.IsNullOrEmpty(Geometry))
        {
            throw new ArgumentException("未设置图形在数据库中的列名");
        }

        // todo: 使用 PG 的 Simplify 达不到效果, 需要继续研究
        // sql =
        //     $"SELECT CASE WHEN ST_HasArc({Geometry}) THEN {Geometry} ELSE ST_Simplify(ST_Force2D({Geometry}), 0.00001, true) END as geom{columnSql} from (SELECT {Geometry} as geom{columnSql} FROM {Table} WHERE {@where} {Geometry} && ST_MakeEnvelope({bbox.MinX}, {bbox.MinY},{bbox.MaxX},{bbox.MaxY}, {SRID})) t";

        var sqlBuilder = new StringBuilder();
        if (Properties == null || Properties.Count == 0)
        {
            sqlBuilder.Append("SELECT * ").Append("FROM ").Append(Table).Append(" WHERE ");
        }
        else
        {
            sqlBuilder.Append("SELECT");
            var containsId = false;
            foreach (var property in Properties)
            {
                if (property == Geometry)
                {
                    continue;
                }

                if (containsId == false && property == Id)
                {
                    containsId = true;
                }

                sqlBuilder.Append(' ').Append(property).Append(',');
            }

            if (!containsId)
            {
                sqlBuilder.Append(' ').Append(Id).Append(',');
            }

            sqlBuilder.Append(' ').Append(Geometry).Append(" WHERE ");
        }

        sqlBuilder.Append(Geometry).Append(" && ST_MakeEnvelope(@MinX, @MinY, @MaxX, @MaxY, @Srid)");

        if (!string.IsNullOrEmpty(Where))
        {
            sqlBuilder.Append(" AND ").Append(Where);
        }

        var baseSql = sqlBuilder.ToString();

        string sql;
        if (!string.IsNullOrEmpty(Filter))
        {
            var select = FreeSql.Value.Select<object>().WithSql(baseSql);
            var filterInfo = JsonConvert.DeserializeObject<DynamicFilterInfo>(Filter);
            select = select.WhereDynamicFilter(filterInfo);
            sql = select.ToSql();
        }
        else
        {
            sql = baseSql;
        }

        if (EnvironmentVariables.EnableSensitiveDataLogging)
        {
            Logger.LogInformation("{Sql} {MinX}, {MaxX}, {MinY}, {MaxY}, {SRID}", sql, bbox.MinX, bbox.MaxX,
                bbox.MinY, bbox.MaxY, Srid);
        }

        await using var conn = CreateDbConnection();

        return (await conn.QueryAsync(sql, new { bbox.MinX, bbox.MaxX, bbox.MinY, bbox.MaxY, Srid }, null, 30)).Select(
            x =>
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
                        geom.SRID = Srid;
                    }
                }
                else
                {
                    f.Geometry.SRID = Srid;
                }

                return f;
            });
    }

    public override Envelope GetEnvelope()
    {
        return null;
    }

    public override ISource Clone()
    {
        // return new PostgreSource(ConnectionString, Database)
        // {
        //     Table= Table,
        //     Id= Id,
        //     Geometry= Geometry,
        //     Where= Where,
        //     Name= Name,
        //     Properties= Properties.ToHashSet(),
        //     Srid= Srid,
        // };
        return (ISource)MemberwiseClone();
    }

    public override void Dispose()
    {
    }

    private DbConnection CreateDbConnection()
    {
        var dataSource = Cache.GetOrCreate(Name, entry =>
        {
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(ConnectionString);
            dataSourceBuilder.UseNetTopologySuite();
            var dataSource = dataSourceBuilder.Build();
            entry.SetValue(dataSource);
            entry.SetSlidingExpiration(TimeSpan.FromMinutes(15));
            return dataSource;
        });

        return dataSource.CreateConnection();
    }
}