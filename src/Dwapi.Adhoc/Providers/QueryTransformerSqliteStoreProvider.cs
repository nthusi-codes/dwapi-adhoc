using System.Data;
using System.IO;
using ActiveQueryBuilder.Core.QueryTransformer;
using ActiveQueryBuilder.Web.Server.Infrastructure.Providers;
using ActiveQueryBuilder.Web.Server.Services;
using Dwapi.Adhoc.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Dwapi.Adhoc.Providers
{
    public class QueryTransformerSqliteStoreProvider : IQueryTransformerProvider
    {
        public bool SaveState { get; private set; }

        private readonly IDbConnection _connection;

        private readonly IQueryBuilderService _aqbs;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration _config;

        public QueryTransformerSqliteStoreProvider(IQueryBuilderService aqbs, IHostingEnvironment env, IConfiguration config)
        {
            _aqbs = aqbs;
            _env = env;
            _config = config;

            SaveState = true;
            _connection = DataBaseHelper.CreateSqLiteConnection(GetDatabasePath());

            var sql = "create table if not exists QueryTransformers(id text primary key, state TEXT)";
            ExecuteNonQuery(sql);
        }

        private string GetDatabasePath()
        {
            var con = Path.Combine(_env.ContentRootPath, _config.GetConnectionString("SourceConnection"));
            return con;
        }

        public QueryTransformer Get(string id)
        {
            var qt = new QueryTransformer { Tag = id, QueryProvider = _aqbs.Get(id).SQLQuery };
            var state = GetState(id);

            if (state != null)
                qt.XML = state;

            return qt;
        }

        public void Put(QueryTransformer qt)
        {
            if (GetState(qt.Tag.ToString()) == null)
                Insert(qt);
            else
                Update(qt);
        }

        public void Delete(string id)
        {
            var sql = string.Format("delete from QueryTransformers where id = {0}", id);
            ExecuteNonQuery(sql);
        }

        private void Insert(QueryTransformer qt)
        {
            var sql = string.Format("insert into QueryTransformers values ('{0}', '{1}')", qt.Tag, qt.XML);
            ExecuteNonQuery(sql);
        }
        private void Update(QueryTransformer qt)
        {
            var sql = string.Format("update QueryTransformers set state = '{1}' where id = '{0}'", qt.Tag, qt.XML);
            ExecuteNonQuery(sql);
        }

        private void ExecuteNonQuery(string sql)
        {
            try
            {
                if (_connection.State != ConnectionState.Open)
                    _connection.Open();

                using (var cmd = CreateCommand(sql))
                    cmd.ExecuteNonQuery();
            }
            finally
            {
                _connection.Close();
            }
        }

        private string GetState(string id)
        {
            var sql = string.Format("select state from QueryTransformers where id = '{0}'", id);

            try
            {
                if (_connection.State != ConnectionState.Open)
                    _connection.Open();

                using (var cmd = CreateCommand(sql))
                using (var reader = cmd.ExecuteReader())
                    if (reader.Read())
                        return reader["state"].ToString();

                return null;
            }
            finally
            {
                _connection.Close();
            }
        }

        private IDbCommand CreateCommand(string sql)
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = sql;
            return cmd;
        }
    }
}
