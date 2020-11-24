using System;
using System.Data.SqlClient;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.Web.Server;
using Serilog;

namespace Dwapi.Adhoc.Providers
{
    public class AdhocManager : IAdhocManager
    {
        public void RefreshMetadata(string connectionString, string path)
        {
            var qb = new QueryBuilder()
            {
                SyntaxProvider = new MSSQLSyntaxProvider(),
                BehaviorOptions = {AllowSleepMode = true},
                MetadataLoadingOptions = {OfflineMode = true},
                MetadataProvider = new MSSQLMetadataProvider() {Connection = new SqlConnection(connectionString)}
            };

            try
            {
                qb.MetadataContainer.LoadAll(true);
                qb.MetadataStructure.Refresh();
                qb.MetadataContainer.ExportToXML(path);
            }
            catch (Exception e)
            {
                Log.Error($"Refersh metadata error {connectionString}", e);
                throw;
            }
        }
    }
}