using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CentroLuant.DataAccess;
using CentroLuant.Models;

namespace CentroLuant.Controllers
{
    [Authorize(Roles = "Administrador,Especialista")]
    public class ClinicoController : Controller
    {
        private readonly PacienteRepository _pacRepo;
        private readonly HistorialRepository _histRepo;

        public ClinicoController(IConfiguration config)
        {
            var conn = config.GetConnectionString("DefaultConnection")
                       ?? throw new Exception("Cadena de conexión no configurada.");

            _pacRepo = new PacienteRepository(conn);
            _histRepo = new HistorialRepository(conn);
        }

        // ================================================================
        //                     VISUALIZAR HISTORIAL
        // ================================================================

        [HttpGet]
        public IActionResult Historial(string? dni)
        {
            var vm = new HistorialViewModel();

            if (!string.IsNullOrWhiteSpace(dni))
            {
                var paciente = _pacRepo.ConsultarPacientePorDNI(dni);

                if (paciente == null)
                {
                    ViewBag.Error = "No existe un paciente registrado con ese DNI.";
                    return View(vm);
                }

                var historial = _histRepo.ObtenerPorDni(dni)
                                ?? _histRepo.CrearHistorial(dni);

                var tratamientos = _histRepo.ObtenerTratamientos(historial.ID_Historial);

                vm.Paciente = paciente;
                vm.Historial = historial;
                vm.Tratamientos = tratamientos;
                vm.NuevoTratamiento = new Tratamiento
                {
                    ID_Historial = historial.ID_Historial,
                    FechaTratamiento = DateTime.Today,
                    Costo = 0,
                    DNI_Paciente = dni
                };
            }

            if (TempData["msg"] != null)
                ViewBag.Msg = TempData["msg"];

            if (TempData["msgError"] != null)
                ViewBag.MsgError = TempData["msgError"];

            return View(vm);
        }

        // ================================================================
        //                     EDITAR HISTORIAL
        // ================================================================

        [Authorize(Roles = "Administrador,Especialista")]
        public IActionResult EditarHistorial(int idHistorial)
        {
            var historial = _histRepo.ObtenerPorId(idHistorial);
            if (historial == null) return NotFound();

            var paciente = _pacRepo.ConsultarPacientePorHistorial(idHistorial);

            ViewBag.Paciente = paciente;
            return View(historial);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Especialista")]
        public IActionResult EditarHistorial(HistorialMedico model)
        {
            if (!_histRepo.ActualizarHistorial(model.ID_Historial, model.ObservacionesIniciales))
            {
                ViewBag.Error = "No se pudo guardar los cambios del historial.";
                return View(model);
            }

            TempData["msg"] = "Historial clínico actualizado correctamente.";
            return RedirectToAction("Historial", new { dni = model.DNI_Paciente });
        }

        // ================================================================
        //                     EDITAR TRATAMIENTO
        // ================================================================

        [Authorize(Roles = "Administrador,Especialista")]
        public IActionResult EditarTratamiento(int id)
        {
            var tratamiento = _histRepo.ObtenerTratamientoPorId(id);
            if (tratamiento == null) return NotFound();

            return View(tratamiento);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Especialista")]
        public IActionResult EditarTratamiento(Tratamiento model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (!_histRepo.ActualizarTratamiento(model))
            {
                ViewBag.Error = "No se pudo actualizar el tratamiento.";
                return View(model);
            }

            TempData["msg"] = "Tratamiento actualizado correctamente.";

            return RedirectToAction("Historial", new { dni = model.DNI_Paciente });
        }

        // ================================================================
        //                     REGISTRAR TRATAMIENTO
        // ================================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegistrarTratamiento(Tratamiento t)
        {
            if (!ModelState.IsValid)
            {
                TempData["msgError"] = "Complete los datos requeridos del tratamiento.";
                return RedirectToAction("Historial", new { dni = t.DNI_Paciente });
            }

            _histRepo.AgregarTratamiento(t);
            TempData["msg"] = "Tratamiento registrado en el historial.";

            return RedirectToAction("Historial", new { dni = t.DNI_Paciente });
        }
    }
}
