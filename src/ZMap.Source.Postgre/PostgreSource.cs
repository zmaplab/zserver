using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
    private static readonly Lazy<ILogger> Logger = new(Log.CreateLogger<PostgreSource>);
    private static readonly ConcurrentDictionary<string, string> BaseSql = new();

    /// <summary>
    /// 
    /// </summary>
    public string ExtendedVersion { get; set; }

    private static readonly Lazy<IFreeSql> FreeSql = new(() =>
    {
        return new FreeSql.FreeSqlBuilder()
            .UseConnectionFactory(DataType.PostgreSQL, () =>
            {
                var dataSourceBuilder = new NpgsqlDataSourceBuilder(
                    "User ID=nobody;Password=jv9@4cLh#cMuEgSSUc2b;Host=127.0.0.1;Port=55432;Database=__db;Pooling=true;");
                dataSourceBuilder.UseNetTopologySuite();
                var dataSource = dataSourceBuilder.Build();
                return dataSource.CreateConnection();
            })
            .Build();
    });

    public override async Task<IEnumerable<Feature>> GetFeaturesAsync(Envelope bbox, string fitler)
    {
        if (string.IsNullOrEmpty(Geometry))
        {
            throw new ArgumentException("未设置图形在数据库中的列名");
        }

        // todo: 使用 PG 的 Simplify 达不到效果, 需要继续研究
        // sql =
        //     $"SELECT CASE WHEN ST_HasArc({Geometry}) THEN {Geometry} ELSE ST_Simplify(ST_Force2D({Geometry}), 0.00001, true) END as geom{columnSql} from (SELECT {Geometry} as geom{columnSql} FROM {Table} WHERE {@where} {Geometry} && ST_MakeEnvelope({bbox.MinX}, {bbox.MinY},{bbox.MaxX},{bbox.MaxY}, {SRID})) t";

        var baseSql = BaseSql.GetOrAdd(Key, (_) =>
        {
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

            if ("KingbaseES".Equals(ExtendedVersion, StringComparison.OrdinalIgnoreCase))
            {
                sqlBuilder.Append("ST_Intersects(").Append(Geometry)
                    .Append(", ST_MakeEnvelope(@MinX, @MinY, @MaxX, @MaxY, @Srid))");
            }
            else
            {
                sqlBuilder.Append(Geometry).Append(" && ST_MakeEnvelope(@MinX, @MinY, @MaxX, @MaxY, @Srid)");
            }

            if (!string.IsNullOrEmpty(Where))
            {
                sqlBuilder.Append(" AND ").Append(Where);
            }

            return sqlBuilder.ToString();
        });

        var sql = !string.IsNullOrEmpty(fitler) ? CombineFilter(baseSql, fitler) : baseSql;

        if (EnvironmentVariables.EnableSensitiveDataLogging)
        {
            Logger.Value.LogInformation("{Sql} {MinX}, {MaxX}, {MinY}, {MaxY}, {SRID}", sql, bbox.MinX, bbox.MaxX,
                bbox.MinY, bbox.MaxY, Srid);
        }

        await using var dataSource = GetNpgSqlDataSource();
        await using var conn = dataSource.CreateConnection();

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

    internal string CombineFilter(string baseSql, string filter)
    {
        var select = FreeSql.Value.Select<object>().WithSql(baseSql);
        var filterInfo = JsonConvert.DeserializeObject<DynamicFilterInfo>(filter);
        select = select.WhereDynamicFilter(filterInfo);
        return select.ToSql();
    }

    public override Envelope Envelope => null;

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

    private NpgsqlDataSource GetNpgSqlDataSource()
    {
        var dataSourceBuilder = Cache.GetOrCreate(ConnectionString, entry =>
        {
            var builder = new NpgsqlDataSourceBuilder(ConnectionString);
            builder.UseNetTopologySuite();

            entry.SetValue(builder);
            entry.SetSlidingExpiration(TimeSpan.FromMinutes(5));
            return builder;
        });
        return dataSourceBuilder.Build();
    }
}