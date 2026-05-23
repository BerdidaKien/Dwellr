using Microsoft.AspNetCore.Mvc;

namespace Dwellr.Controllers
{
    public class OrderController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
