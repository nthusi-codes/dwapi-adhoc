using Microsoft.AspNetCore.Mvc;

namespace Dwapi.Adhoc.Controllers
{
    public class FlexController : Controller
    {
        // GET
        public IActionResult Index()
        {
            return View();
        }
    }
}
