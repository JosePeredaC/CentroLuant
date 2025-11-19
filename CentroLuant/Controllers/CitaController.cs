using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CentroLuant.DataAccess;
using CentroLuant.Models;

namespace CentroLuant.Controllers
{
    [Authorize(Roles = "Administrador,Recepcionista")]
    public class CitaController : Controller
    {
        private readonly CitaRepository _repoCitas;
        private readonly PacienteRepository _repoPacientes;
        private readonly UsuarioRepository _repoUsuarios;

        public CitaController(IConfiguration config)
        {
            var conn = config.GetConnectionString("DefaultConnection")
                       ?? throw new Exception("Cadena de conexión no configurada.");

            _repoCitas = new CitaRepository(conn);
            _repoPacientes = new PacienteRepository(conn);
            _repoUsuarios = new UsuarioRepository(conn);
        }

        public IActionResult Index(DateTime? fecha)
        {
            var citas = _repoCitas.ObtenerCitas(fecha);
            ViewBag.FechaFiltro = fecha?.ToString("yyyy-MM-dd");
            return View(citas);
        }

        // GET: Crear
        public IActionResult Crear()
        {
            var hoy = DateTime.Today;

            var model = new Cita
            {
                Fecha = hoy,
                Hora = new TimeSpan(9, 0, 0),
                Estado = "Programada"
            };

            ViewBag.Especialistas = _repoUsuarios.ObtenerEspecialistasActivos();
            return View(model);
        }

        // POST: Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(Cita model)
        {
            if (string.IsNullOrWhiteSpace(model.DNI_Paciente))
            {
                ModelState.AddModelError(nameof(model.DNI_Paciente),
                    "Debe ingresar el DNI del paciente.");
            }

            if (model.Fecha == default)
            {
                ModelState.AddModelError(nameof(model.Fecha),
                    "Debe seleccionar una fecha válida.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Especialistas = _repoUsuarios.ObtenerEspecialistasActivos();
                return View(model);
            }

            var paciente = _repoPacientes.ConsultarPacientePorDNI(model.DNI_Paciente);
            if (paciente == null)
            {
                ViewBag.Error = "No existe un paciente registrado con ese DNI.";
                ViewBag.Especialistas = _repoUsuarios.ObtenerEspecialistasActivos();
                return View(model);
            }

            var especialista = _repoUsuarios.ObtenerPorId(model.ID_Especialista);
            if (especialista == null || especialista.Rol != "Especialista" || !especialista.Activo)
            {
                ViewBag.Error = "El especialista seleccionado no es válido o está inactivo.";
                ViewBag.Especialistas = _repoUsuarios.ObtenerEspecialistasActivos();
                return View(model);
            }

            if (string.IsNullOrWhiteSpace(model.Estado))
                model.Estado = "Programada";

            if (!_repoCitas.CrearCita(model, out var msgError))
            {
                ViewBag.Error = msgError;
                ViewBag.Especialistas = _repoUsuarios.ObtenerEspecialistasActivos();
                return View(model);
            }

            TempData["msg"] = "Cita registrada correctamente.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cancelar(int id)
        {
            _repoCitas.CambiarEstado(id, "Cancelada");
            TempData["msg"] = "Cita cancelada.";
            return RedirectToAction("Index");
        }
    }
}
