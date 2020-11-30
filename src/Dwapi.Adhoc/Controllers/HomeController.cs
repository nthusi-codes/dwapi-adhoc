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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Dwapi.Adhoc.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IAdhocManager _adhocManager;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;



        public HomeController(IAdhocManager adhocManager, IWebHostEnvironment env, IConfiguration configuration)
        {
            _adhocManager = adhocManager;
            _env = env;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Index", "QueryResultsDemo");
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }

        public IActionResult RefreshMeta()
        {
            _adhocManager.RefreshMetadata(GetDatabasePath(), GetMetaDataPath());
            return Ok(new {Status = "MetasRefreshed"});
        }

        public IActionResult ShowDwh()
        {
            return Redirect(_configuration["HomeUri"]);
        }

        private string GetDatabasePath()
        {
            var con = _configuration.GetConnectionString("SourceConnection");
            return con;
        }

        private string GetMetaDataPath()
        {
            var con = Path.Combine(_env.ContentRootPath,_configuration["XMLMetadata"]);
            return con;
        }
    }
}
