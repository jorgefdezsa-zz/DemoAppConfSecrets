namespace MvcEntornos.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using MvcEntornos.Models;
    using System.Diagnostics;

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> logger;
        private readonly Settings settings;

        public HomeController(ILogger<HomeController> logger, IOptionsSnapshot<Settings> settings)
        {
            this.logger = logger;
            this.settings = settings.Value;
        }

        public IActionResult Index()
        {
            ViewData["BackgroundColor"] = settings.BackgroundColor;
            ViewData["FontSize"] = settings.FontSize;
            ViewData["FontColor"] = settings.FontColor;
            ViewData["Message"] = settings.Message;
            ViewData["FromKV"] = settings.FromKV;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
