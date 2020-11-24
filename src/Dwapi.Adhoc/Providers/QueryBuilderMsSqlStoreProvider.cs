using System;
using System.Data;
using System.IO;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.Web.Server;
using ActiveQueryBuilder.Web.Server.Infrastructure.Providers;
using Dwapi.Adhoc.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Dwapi.Adhoc.Providers
{
    /// QueryBuilder storage provider which saves the state in Sqlite database
    /// </summary>
    public class QueryBuilderMsSqlStoreProvider : IQueryBuilderProvider
    {
        public bool SaveState { get; private set; }

        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;

        public QueryBuilderMsSqlStoreProvider(IWebHostEnvironment env, IConfiguration config)
        {
            _env = env;
            _config = config;

            SaveState = true;

            var sql =@"

if not exists (select * from sysobjects where name='QueryBuilders' and xtype='U')
create table QueryBuilders
                        (
	                        id nvarchar(50)
		                        constraint QueryBuilders_pk
			                        primary key nonclustered,
	                        layout text
                        )
                   ";
            ExecuteNonQuery(sql);
        }

        /// <summary>
        /// Creates an instance of the QueryBuilder object and loads its state identified by the given id.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public QueryBuilder Get(string id)
        {
            var qb = new QueryBuilder(id)
            {
                SyntaxProvider = new MSSQLSyntaxProvider(),
                BehaviorOptions =
                {
                    AllowSleepMode = true
                }
                ,
                MetadataLoadingOptions =
                {
                    OfflineMode = true
                }
            };

            try
            {
                qb.MetadataContainer.ImportFromXML(GetMetaDataPath());
                qb.MetadataStructure.Refresh();
            }
            catch (Exception e)
            {
                Log.Error($"Instance error:{id}",e);
                throw;
            }

            var layout = GetLayout(id);

            try
            {
                if (layout != null)
                    qb.LayoutSQL = layout;
            }
            catch (Exception e)
            {
                Log.Error($"Instance error:{id}",e);
                throw;
            }
            return qb;
        }


        private string GetDatabasePath()
        {
            var con = _config.GetConnectionString("SourceConnection");
            return con;
        }

        private string GetMetaDataPath()
        {
            var  con= Path.Combine( _env.ContentRootPath,_config["XMLMetadata"]);
            return con;
        }

        /// <summary>
        /// Saves the state of QueryBuilder object identified by its Tag property.
        /// </summary>
        /// <param name="qb">The QueryBuilder object.</param>
        public void Put(QueryBuilder qb)
        {
            if (GetLayout(qb.Tag) == null)
                Insert(qb);
            else
                Update(qb);
        }

        /// <summary>
        /// Clears the state of QueryBuilder object identified by the given id.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public void Delete(string id)
        {
            var sql = string.Format("delete from QueryBuilders where id = {0}", id);
            ExecuteNonQuery(sql);
        }

        private void Insert(QueryBuilder qb)
        {
            var sql = string.Format("insert into QueryBuilders values ('{0}', '{1}')", qb.Tag, qb.LayoutSQL);
            ExecuteNonQuery(sql);
        }
        private void Update(QueryBuilder qb)
        {
            var sql = string.Format("update QueryBuilders set layout = '{1}' where id = '{0}'", qb.Tag, qb.LayoutSQL);
            ExecuteNonQuery(sql);
        }

        private void ExecuteNonQuery(string sql)
        {
            var _connection = DataBaseHelper.CreateMsSqlConnection(GetDatabasePath());

            try
            {
                if (_connection.State != ConnectionState.Open)
                    _connection.Open();

                using (var cmd = CreateCommand(_connection, sql))
                    cmd.ExecuteNonQuery();
            }
            finally
            {
                _connection.Close();
            }
        }

        private string GetLayout(string id)
        {
            var sql = string.Format("select layout from QueryBuilders where id = '{0}'", id);
            var _connection = DataBaseHelper.CreateMsSqlConnection(GetDatabasePath());

            try
            {
                if (_connection.State != ConnectionState.Open)
                    _connection.Open();

                using (var cmd = CreateCommand(_connection, sql))
                using (var reader = cmd.ExecuteReader())
                    if (reader.Read())
                        return reader["layout"].ToString();

                return null;
            }
            finally
            {
                _connection.Close();
            }
        }

        private IDbCommand CreateCommand(IDbConnection conn, string sql)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            return cmd;
        }
    }
}
