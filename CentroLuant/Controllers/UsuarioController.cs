using Microsoft.AspNetCore.Mvc;
using CentroLuant.DataAccess;
using CentroLuant.Models;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace CentroLuant.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class UsuarioController : Controller
    {
        private readonly UsuarioRepository _repo;

        public UsuarioController(IConfiguration config)
        {
            _repo = new UsuarioRepository(config.GetConnectionString("DefaultConnection"));
        }

        private string Hash(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));

            // 👇 Igual que lo que ya hay en la tabla Usuario
            return Convert.ToBase64String(bytes);
        }

        public IActionResult Index()
        {
            var usuarios = _repo.ObtenerUsuarios(); 

            return View(usuarios);
        }

        public IActionResult Crear() => View();

        [HttpPost]
        public IActionResult Crear(Usuario u)
        {
            u.ContrasenaHash = Hash(u.ContrasenaHash);
            _repo.CrearUsuario(u);
            return RedirectToAction("Index");
        }

        public IActionResult Editar(int id)
        {
            return View(_repo.ObtenerPorId(id));
        }

        [HttpPost]
        public IActionResult Editar(Usuario u)
        {
            _repo.EditarUsuario(u);
            return RedirectToAction("Index");
        }

        public IActionResult Desactivar(int id)
        {
            var usuario = _repo.ObtenerPorId(id);
            if (usuario == null) return NotFound();

            // 1. No permitir desactivarse a sí mismo
            if (User.Identity?.Name == usuario.UsuarioLogin)
            {
                TempData["msg"] = "No puedes desactivar tu propia cuenta.";
                return RedirectToAction("Index");
            }

            // 2. Si el usuario es administrador → proteger
            if (usuario.Rol == "Administrador" && usuario.Activo)
            {
                int adminsActivos = _repo.ContarAdminsActivos();

                if (adminsActivos <= 1)
                {
                    TempData["msg"] = "No se puede desactivar al único administrador activo.";
                    return RedirectToAction("Index");
                }
            }

            _repo.DesactivarUsuario(id);

            TempData["msg"] = "Usuario desactivado correctamente.";
            return RedirectToAction("Index");
        }


        public IActionResult ResetPassword(int id)
        {
            if (id == 1)
            {
                TempData["msg"] = "No puedes restablecer la contraseña del administrador";
                return RedirectToAction("Index");
            }
            _repo.ResetPassword(id, Hash("123456"));

            TempData["msg"] = "Contraseña temporal: 123456";
            return RedirectToAction("Index");
        }
        public IActionResult Activar(int id)
        {
            var usuario = _repo.ObtenerPorId(id);
            if (usuario == null) return NotFound();

            // No permitir activar administradores deshabilitados si eso genera riesgos (esto es opcional)
            // En realidad activar no tiene restricciones serias, así que lo permitimos.

            _repo.ActivarUsuario(id);

            TempData["msg"] = "Usuario activado correctamente.";
            return RedirectToAction("Index");
        }
        public IActionResult Eliminar(int id)
        {
            var usuario = _repo.ObtenerPorId(id);
            if (usuario == null) return NotFound();

            // 1. No se puede eliminar a uno mismo
            if (User.Identity?.Name == usuario.UsuarioLogin)
            {
                TempData["msg"] = "No puedes eliminar tu propia cuenta.";
                return RedirectToAction("Index");
            }

            // 2. No se puede eliminar un administrador
            if (usuario.Rol == "Administrador")
            {
                TempData["msg"] = "No se puede eliminar una cuenta de administrador.";
                return RedirectToAction("Index");
            }

            _repo.EliminarUsuario(id);
            TempData["msg"] = "Usuario eliminado correctamente.";
            return RedirectToAction("Index");
        }

    }
}
