using ActiveQueryBuilder.Web.Server.Infrastructure.Providers;
using Dwapi.Adhoc.Helpers;

namespace Dwapi.Adhoc.Providers
{
    public interface IAdhocManager
    {
        void RefreshMetadata(string connectionString, string path);
        void RefreshMetadataHts(string connectionString, string path);
    }
}
