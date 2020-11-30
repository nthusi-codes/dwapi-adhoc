using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ActiveQueryBuilder.Web.Server;
using ActiveQueryBuilder.Web.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Dwapi.Adhoc.Models;
using Dwapi.Adhoc.Providers;
using Flexmonster.DataServer.Core;
using Flexmonster.DataServer.Core.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Dwapi.Adhoc.Controllers
{
    [Route("[controller]")]
    public class BigDataController : Controller
    {
        private const string API_VERSION = "2.8.5";
        private readonly IQueryBuilderService _aqbs;
        private readonly IQueryTransformerService _qts;
        private readonly IConfiguration _configuration;

        private static Dictionary<string, List<object>> _userPermissions = new Dictionary<string, List<object>>()
        {
            {"AAAA", null },
            {"BBBB",  new List<object>(){ "Germany","France" } },
            {"CCCC",  new List<object>(){ "USA","Canada" } },
            {"DDDD", new List<object>(){ "Australia" } },
        };

        private readonly IApiService _apiService;

        public BigDataController(IApiService apiService,IQueryBuilderService aqbs, IQueryTransformerService qts,IConfiguration configuration)
        {
            _apiService = apiService;
            _aqbs = aqbs;
            _qts = qts;
            _configuration = configuration;
        }

        public IActionResult Get()
        {
            return Ok(new {Status = "Connected !"});
        }

        [Route("handshake")]
        public IActionResult Handshake([FromBody] HandshakeRequst request)
        {
            object response = null;
            if (request.Type == RequestType.Handshake)
            {
                response = new { version = API_VERSION };
            }
            return new JsonResult(response);
        }


        [Route("fields")]
        public async Task<IActionResult> PostFields([FromBody]FieldsRequest request)
        {

            var response = await _apiService.GetFieldsAsync(request);
            return new JsonResult(response);
        }

        [HttpPost("members")]
        public async Task<IActionResult> PostMembers([FromBody]MembersRequest request)
        {
            var response = await _apiService.GetMembersAsync(request, GetServerFilter());
            return new JsonResult(response);
        }

        [HttpPost("select")]
        public async Task<IActionResult> PostSelect([FromBody]SelectRequest request)
        {
            var response = await _apiService.GetAggregatedDataAsync(request, GetServerFilter());
            return new JsonResult(response);
        }

        //server side filter to disable some data for user
        private ServerFilter GetServerFilter()
        {
            return null;

            HttpContext.Request.Headers.TryGetValue("UserToken", out StringValues userRole);
            if (userRole.Count == 1)
            {
                ServerFilter serverFilter = new ServerFilter();
                serverFilter.Field = new Field() { UniqueName = "Country", Type = ColumnType.StringType };
                serverFilter.Include = _userPermissions[userRole[0]];
                return serverFilter;
            }
            return null;
        }
    }
}
