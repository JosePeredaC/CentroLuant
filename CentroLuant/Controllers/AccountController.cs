using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using CentroLuant.Models;

namespace CentroLuant.Controllers
{
    public class AccountController : Controller
    {
        private readonly Dictionary<string, (string Password, string Rol)> _usuarios =
            new()
            {
                { "recepcion", ("1234", "Recepcionista") },
                { "doctor", ("1234", "Especialista") }
            };

        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (!_usuarios.ContainsKey(model.Usuario) ||
                _usuarios[model.Usuario].Password != model.Contrasena)
            {
                model.MensajeError = "Usuario o contraseña incorrectos.";
                return View(model);
            }

            var rol = _usuarios[model.Usuario].Rol;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, model.Usuario),
                new Claim(ClaimTypes.Role, rol)
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        public IActionResult Denied()
        {
            return Content("Acceso denegado.");
        }
    }
}
