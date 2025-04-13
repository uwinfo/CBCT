using Dapper;
using Npgsql;

namespace Core.Helpers
{
    public class DapperHelper
    {
        private static string GetConnectionString(string? dbc)
        {
            return dbc ?? Su.PgSql.GetDbc(Su.PgSql.DefaultDbId);
        }

        private static NpgsqlConnection CreateConnection(string dbc)
        {
            var connection = new NpgsqlConnection(dbc);
            connection.Open();
            return connection;
        }

        public static int ExecuteSQL(string sql, object param, string? dbc = null, int? timeout = null)
        {
            using var connection = CreateConnection(GetConnectionString(dbc));
            return connection.Execute(sql, param, commandTimeout: timeout);
        }

        //public static async Task<int> ExecuteSQLAsync(string sql, object param, string? dbc = null, int? timeout = null)
        //{
        //    using var connection = CreateConnection(GetConnectionString(dbc));
        //    return await connection.ExecuteAsync(sql, param, commandTimeout: timeout);
        //}

        //public static DataTable ExecuteReader(string sql, object param, string? dbc = null, int? timeout = null)
        //{
        //    using var connection = CreateConnection(GetConnectionString(dbc));
        //    using var reader = connection.ExecuteReader(sql, param, commandTimeout: timeout);
        //    var dt = new DataTable();
        //    dt.Load(reader);
        //    return dt;
        //}

        //public static async Task<DataTable> ExecuteReaderAsync(string sql, object param, string? dbc = null, int? timeout = null)
        //{
        //    using var connection = CreateConnection(GetConnectionString(dbc));
        //    using var reader = await connection.ExecuteReaderAsync(sql, param, commandTimeout: timeout);
        //    var dt = new DataTable();
        //    dt.Load(reader);
        //    return dt;
        //}

        //public static DataTable Query(string sql, object param, string? dbc = null, int? timeout = null)
        //{
        //    using var connection = CreateConnection(GetConnectionString(dbc));
        //    var res = connection.Query(sql, param, commandTimeout: timeout);
        //    var json = JsonConvert.SerializeObject(res);
        //    return JsonConvert.DeserializeObject<DataTable>(json);
        //}

        //public static async Task<DataTable> QueryAsync(string sql, object param, string? dbc = null, int? timeout = null)
        //{
        //    using var connection = CreateConnection(GetConnectionString(dbc));
        //    var res = await connection.QueryAsync(sql, param, commandTimeout: timeout);
        //    var json = JsonConvert.SerializeObject(res);
        //    return JsonConvert.DeserializeObject<DataTable>(json);
        //}

        //public static async Task<object?> ExecuteScalarAsync(string sql, object param, string? dbc = null, int? timeout = null)
        //{
        //    using var connection = CreateConnection(GetConnectionString(dbc));
        //    return await connection.ExecuteScalarAsync(sql, param, commandTimeout: timeout);
        //}

        //public static object? ExecuteScalar(string sql, object param, string? dbc = null, int? timeout = null)
        //{
        //    using var connection = CreateConnection(GetConnectionString(dbc));
        //    return connection.ExecuteScalar(sql, param, commandTimeout: timeout);
        //}

        //public static string ReplaceListCri(string sql, string nameCri, List<string> ltCri)
        //{
        //    if (ltCri.Count > 0)
        //    {
        //        sql = sql.Replace("#" + nameCri + "#", " where " + string.Join(" and ", ltCri));
        //        sql = sql.Replace("#" + nameCri + "2#", " and " + string.Join(" and ", ltCri));
        //    }
        //    sql = sql.Replace("#" + nameCri + "#", "");
        //    sql = sql.Replace("#" + nameCri + "2#", "");
        //    return sql;
        //}
    }
}