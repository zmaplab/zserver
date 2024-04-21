using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeSql.Internal.Model;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using Npgsql;
using ZMap.Infrastructure;

namespace ZMap.Source.Postgre;

public sealed class PostgreSource(string connectionString) : SpatialDatabaseSource(connectionString)
{
    private static readonly ConcurrentDictionary<string, IFreeSql> FreeSqlCache = new();

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
            foreach (var property in Properties)
            {
                if (property == Geometry)
                {
                    continue;
                }

                sqlBuilder.Append(' ').Append(property).Append(',');
            }

            sqlBuilder.Append(' ').Append(Geometry).Append(" WHERE ");
        }

        sqlBuilder.Append(Geometry).Append(" && ST_MakeEnvelope(@MinX, @MinY, @MaxX, @MaxY, @Srid)");

        if (!string.IsNullOrEmpty(Where))
        {
            sqlBuilder.Append(" AND ").Append(Where);
        }

        var freeSql = GetFreeSql();
        var select = freeSql.Select<object>()
            .WithSql(sqlBuilder.ToString(), new { bbox.MinX, bbox.MaxX, bbox.MinY, bbox.MaxY, Srid });
        if (!string.IsNullOrEmpty(Filter))
        {
            var filterInfo = JsonConvert.DeserializeObject<DynamicFilterInfo>(Filter);
            select = select.WhereDynamicFilter(filterInfo);
        }

        if (EnvironmentVariables.EnableSensitiveDataLogging)
        {
            var sql = select.ToSql();
            Log.Logger.LogInformation("{Sql} {MinX}, {MaxX}, {MinY}, {MaxY}, {SRID}", sql, bbox.MinX, bbox.MaxX,
                bbox.MinY, bbox.MaxY, Srid);
        }

        var items = await select.ToListAsync();
        return items.Select(x =>
        {
            var f = new Feature(Geometry, x as IDictionary<string, object>);
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

    private IFreeSql GetFreeSql()
    {
        return FreeSqlCache.GetOrAdd(ConnectionString, _ =>
        {
            return new FreeSql.FreeSqlBuilder()
                .UseAdoConnectionPool(true)
                .UseConnectionFactory(FreeSql.DataType.PostgreSQL, () =>
                {
                    var builder = new NpgsqlConnectionStringBuilder(ConnectionString);
                    var connectionString = builder.ToString();
                    var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
                    dataSourceBuilder.UseNetTopologySuite();
                    var dataSource = dataSourceBuilder.Build();
                    return dataSource.CreateConnection();
                })
                .Build();
        });
    }
}