using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InmobiliariaMVC.Controllers
{
    [Authorize(Roles = "Agente")]
    public class AgenteController : Controller
    {
        public IActionResult Index() { return View(); }
    }
}