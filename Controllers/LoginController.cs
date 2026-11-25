using Microsoft.AspNetCore.Mvc;
using Projeto_Bibliotec_MVC.ViewModels;

namespace Projeto_Bibliotec_MVC;

public class LoginController : Controller
{
    // Exibe a tela de login
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    // Recebe os dados enviados pelo formulario
    [HttpPost]
    public IActionResult Index(LoginViewModel model)
    {
        // Verifica se os dados recebidos atendem as regras de validação
        if(!ModelState.IsValid)
        {
            return View(model);
        }

        // Mensagem temporaria para testar o recebimento dos dados.
        ModelState.AddModelError("", "Autenticação ainda não implementada");
                                
        return View(model);
    }
}

