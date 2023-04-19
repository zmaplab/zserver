// using System;
// using System.Text;
// using Dapper;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.EntityFrameworkCore.Metadata.Builders;
// using Npgsql;
// using ZMap.Infrastructure;
//
// namespace ZMap.Source.Postgre;
//
// public class PgDbContext : DbContext
// {
//     private readonly string _connectionString;
//     private readonly string _table;
//     private readonly string _database;
//
//     public PgDbContext(string connectionString, string database, string table)
//     {
//         _connectionString = connectionString;
//         _table = table;
//         _database = database;
//     }
//
//     protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//     {
//         optionsBuilder.UseNpgsql(
//             "User ID=postgres;Password=1qazZAQ!;Host=localhost;Port=5432;Database=zserver_dev;Pooling=true;");
//     }
//
//     protected override void OnModelCreating(ModelBuilder modelBuilder)
//     {
//         using var conn = new NpgsqlConnection(_connectionString);
//         if (conn.Database != _database)
//         {
//             conn.ChangeDatabase(_database);
//         }
//
//         using var reader = conn.ExecuteReader($"SELECT * FROM {_table} LIMIT 1");
//
//         var sb = new StringBuilder("public class ");
//         sb.AppendLine(_table);
//         sb.AppendLine("{");
//
//         for (var i = 0; i < reader.FieldCount; ++i)
//         {
//             var field = reader.GetFieldType(i);
//             var name = reader.GetName(i);
//             sb.Append("public ").Append(field.Name).Append(" ").Append(name).AppendLine(" { get; set; }");
//         }
//
//         sb.AppendLine("}");
//         DynamicCompilationUtilities.Compiler.BuildType(sb.ToString());
//         var entityTypeConfigurationBuilder = new StringBuilder();
//         entityTypeConfigurationBuilder.Append(
//                 "public class EntityConfiguration<").Append(_table).Append("> : IEntityTypeConfiguration<")
//             .Append(_table)
//             .Append(">").AppendLine();
//         entityTypeConfigurationBuilder.AppendLine("{");
//         entityTypeConfigurationBuilder.Append("    public void Configure(EntityTypeBuilder<").Append(_table)
//             .Append("> builder)").AppendLine();
//         entityTypeConfigurationBuilder.AppendLine("    {");
//         entityTypeConfigurationBuilder.AppendLine("    }");
//         entityTypeConfigurationBuilder.AppendLine("}");
//         var type = DynamicCompilationUtilities.Compiler.BuildType(entityTypeConfigurationBuilder.ToString());
//
//         modelBuilder.ApplyConfigurationsFromAssembly(type.Assembly);
//     }
//
//     // public string BuildQuerySql(string filter)
//     // {
//     //     // "x.xiao_ban.Contains(\"001\")"
//     //     
//     // }
// }
//
// public class EntityConfiguration<T> : IEntityTypeConfiguration<T> where T : class
// {
//     public void Configure(EntityTypeBuilder<T> builder)
//     {
//     }
// }