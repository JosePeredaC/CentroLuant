using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CentroLuant.DataAccess;
using CentroLuant.Models;
namespace CentroLuant.Controllers
{
    [Authorize(Roles = "Administrador,Recepcionista")]
    public class PacienteController : Controller
    {
        private readonly PacienteRepository _repo;

        public PacienteController(PacienteRepository repo)
        {
            _repo = repo;
        }

        public IActionResult Index()
        {
            var pacientes = _repo.ConsultarTodosLosPacientes();
            return View(pacientes);
        }

        public IActionResult Detalle(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return NotFound();

            var paciente = _repo.ConsultarPacientePorDNI(id);
            if (paciente == null) return NotFound();

            return View(paciente);
        }

        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Crear(Paciente model)
        {
            if (string.IsNullOrWhiteSpace(model.DNI) ||
                string.IsNullOrWhiteSpace(model.Nombres) ||
                string.IsNullOrWhiteSpace(model.Apellidos))
            {
                ViewBag.Error = "Complete los campos obligatorios (DNI, Nombres, Apellidos).";
                return View(model);
            }

            var existente = _repo.ConsultarPacientePorDNI(model.DNI);
            if (existente != null)
            {
                ViewBag.Error = "Ya existe un paciente registrado con ese DNI.";
                return View(model);
            }

            _repo.InsertarPaciente(model);
            TempData["msg"] = "Paciente registrado correctamente.";
            return RedirectToAction("Index");
        }

        public IActionResult Editar(string id)
        {
            var p = _repo.ConsultarPacientePorDNI(id);
            if (p == null) return NotFound();

            return View(p);
        }

        [HttpPost]
        public IActionResult Editar(Paciente model)
        {
            if (string.IsNullOrWhiteSpace(model.Nombres) ||
                string.IsNullOrWhiteSpace(model.Apellidos))
            {
                ViewBag.Error = "Complete los campos obligatorios (Nombres, Apellidos).";
                return View(model);
            }

            bool ok = _repo.EditarPaciente(model);

            if (!ok)
            {
                ViewBag.Error = "No se pudo actualizar el registro.";
                return View(model);
            }

            TempData["msg"] = "Datos del paciente actualizados correctamente.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(string id) // id = DNI
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                TempData["msgError"] = "Paciente no válido.";
                return RedirectToAction("Index");
            }

            bool ok = _repo.EliminarPaciente(id, out var error);

            if (!ok)
            {
                TempData["msgError"] = error ?? "No se pudo eliminar el paciente.";
            }
            else
            {
                TempData["msg"] = "Paciente eliminado correctamente.";
            }

            return RedirectToAction("Index");
        }




    }
}
