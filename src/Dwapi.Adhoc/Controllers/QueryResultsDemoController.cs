using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using ActiveQueryBuilder.Core.QueryTransformer;
using ActiveQueryBuilder.Web.Server.Services;
using Dwapi.Adhoc.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Dwapi.Adhoc.Controllers
{
    [Authorize]
    public class QueryResultsDemoController : Controller
    {
        private string _instanceId = "QueryResults";

        private readonly IQueryBuilderService _aqbs;
        private readonly IQueryTransformerService _qts;
        private readonly IConfiguration _configuration;

        // Use IQueryBuilderService to get access to the server-side instances of Active Query Builder objects.
        // See the registration of this service in the Startup.cs.
        public QueryResultsDemoController(IQueryBuilderService aqbs, IQueryTransformerService qts,IConfiguration configuration)
        {
            _aqbs = aqbs;
            _qts = qts;
            _configuration = configuration;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetData([FromBody] GridModel m)
        {
            var qt = _qts.Get(m.InstanceId);

            qt.Skip((m.Pagenum * m.Pagesize).ToString());
            qt.Take(m.Pagesize == 0 ? "" : m.Pagesize.ToString());

            if (!string.IsNullOrEmpty(m.Sortdatafield))
            {
                qt.Sortings.Clear();

                if (!string.IsNullOrEmpty(m.Sortorder))
                {
                    var c = qt.Columns.FindColumnByResultName(m.Sortdatafield);

                    if (c != null)
                        qt.OrderBy(c, m.Sortorder.ToLower() == "asc");
                }
            }

            return GetData(qt, m.Params);
        }

        public ActionResult GetFlexData(string instanceId)
        {
            return GetData(new GridModel() {InstanceId = instanceId});
        }

        private ActionResult GetData(QueryTransformer qt, Param[] _params)
        {
            // var conn = qt.Query.SQLContext.MetadataProvider.Connection;
            var conn = new SqlConnection(_configuration.GetConnectionString("SourceConnection"));
            var sql = qt.SQL;

            if (_params != null)
                foreach (var p in _params)
                    p.DataType = qt.Query.QueryParameters.First(qp => qp.FullName == p.Name).DataType;

            try
            {
                var data = DataBaseHelper.GetData(conn, sql, _params);
                return Json(data);
            }
            catch (Exception e)
            {
                return StatusCode((int) HttpStatusCode.BadRequest, e.Message);
            }
        }

        public void LoadQuery(string query,string instanceId)
        {
            var qb = _aqbs.Get(instanceId);

            if (query == "artist")
                qb.SQL = "Select * From DimDate";
            else
                qb.SQL = "Select * From DimDate";

            _aqbs.Put(qb);
        }


    }

    public class GridModel
    {
        public int Pagenum { get; set; }
        public int Pagesize { get; set; }
        public string Sortdatafield { get; set; }
        public string Sortorder { get; set; }
        public Param[] Params { get; set; }
        public string InstanceId { get; set; } = "QueryResults";
    }

    public class Param
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public DbType DataType { get; set; }
    }
}
