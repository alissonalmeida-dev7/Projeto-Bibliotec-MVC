using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projeto_Bibliotec_MVC.Data;
using Projeto_Bibliotec_MVC.Models;
using Projeto_Bibliotec_MVC.ViewModels;

namespace Projeto_Bibliotec_MVC;

public class LoginController : Controller
{
    // RECEBENDO DEPENDENCIAS (registradas na program)
    // Permite consultar e manipular os dados do banco
    private readonly AppDbContext _context;
    // Permite gerar e validar o hash das senhas
    private readonly IPasswordHasher<Usuario> _passwordHasher;

    public LoginController(AppDbContext context, IPasswordHasher<Usuario> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    // Exibe a tela de login
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    // Recebe os dados enviados pelo formulario
    [HttpPost]
    public async Task<IActionResult> Index(LoginViewModel model)
    {
        // Verifica se os dados recebidos atendem as regras de validação
        if(!ModelState.IsValid)
        {
            return View(model);
        }

        //* O Async seve para utilizar coisas externas da aplicação sem alterar a ordem de rodagem do codigo (utilizado sempre quando se tratar de banco de dados).
        //* Ela busca um usuario no banco pelo e-mail e verifica se ele não esta excluido, se não encontrar um usuario ou se estiver excluido retorna null.
        Usuario usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Email == model.Email && u.Excluido == false); 

        if(usuario == null)
        {
            ModelState.AddModelError("", "E-mail ou senha inválidos.");
            return View(model);
        }

        //* Verifica se a senha informada corresponde ao hash armazenado.
        //* O metodo recebe 3 parametros
        var resultadoSenha = _passwordHasher.VerifyHashedPassword(
            usuario, //? Objeto que representa o usuário encontrado no banco.
            usuario.Senha, //? Hash da senha armazenada no banco de dados.
            model.Senha //? Senha digitada pelo usuário no formulario de login.
        );

        //* Verifica se a senha está incorreta
        if(resultadoSenha == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError("", "E-mail ou senha inválidos.");
            return View(model);
        }

        //* Verifica se a senha está correta, mas o hash precisa ser atualizado
        if(resultadoSenha == PasswordVerificationResult.Success)
        {
            //? Gera um novo hash da senha utilizando as configurações de segurança atuais.
            usuario.Senha = _passwordHasher.HashPassword(usuario, model.Senha);

            // Salva o novo hash no banco de dados
            await _context.SaveChangesAsync();
        }

        //* Any retorna um bool (True or False)
        //* Verifica se o usuario está cadastrado como admnistrador
        //* Verifica dentro da tabela de adm se o usuario possui registro
        //* Retorna True se existir pelo menos um registro que atende a condição informada.
        bool administrador = await _context.Administrador.AnyAsync(a => a.IdAdministrador == usuario.IdUsuario);

        bool aluno = await _context.Aluno.AnyAsync(a => a.IdAluno == usuario.IdUsuario);

        string? tipoUsuario = administrador ? "Administrador" : aluno ? "Aluno" : null;

        //* Cria uma lista de claims com as informaçoes do usuario  autenticado
        //* Claims -> são informações do usuario que permitem identificar e controlar o acesso.
        var claims = new List<Claim>
        {   
            //? Armazena o identificador(id) do usuario
            new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),

            //? Armazena o nome do usuário
            new Claim(ClaimTypes.Name, usuario.Nome),

            //? Armazena o e-mail do usuário
            new Claim(ClaimTypes.Email, usuario.Email),

            //? Armazena o tipo de usuário para controle de acesso
            new Claim(ClaimTypes.Role, tipoUsuario)
        };

        //* Cria a identidade do usuario utilizando as Claims
        //* ClaimsIdentity -> Como se fosse uma carteirinha de identificação do usuario detro do sistema.
        //* cookies -> informa que estamo utilizando o tipo de autenticação por cookies.
        var identidade = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme
        );

        //* Representar o usuário autenticado
        //* Classe do asp.net para representar o usuário
        var principal = new ClaimsPrincipal(identidade);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal
        );

        return RedirectToAction("Index", "Home");
    }
}

