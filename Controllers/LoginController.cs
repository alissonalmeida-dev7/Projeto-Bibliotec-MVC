using Microsoft.AspNetCore.Mvc;

namespace Projeto_Bibliotec_MVC;

public class LoginController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}

