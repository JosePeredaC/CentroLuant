using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CentroLuant.DataAccess;
using CentroLuant.Models;
// Se requiere para usar IConfiguration y obtener la cadena de conexión
using Microsoft.Extensions.Configuration;

namespace CentroLuant.Controllers
{
    [Authorize(Roles = "Administrador,Recepcionista")]
    public class PacienteController : Controller
    {
        private readonly PacienteRepository _repo;
        private readonly HistorialRepository _histRepo;
        public PacienteController(IConfiguration config, PacienteRepository repo)
        {
            _repo = repo;

            // Inicialización de HistorialRepository con la cadena de conexión
            var conn = config.GetConnectionString("DefaultConnection")
                           ?? throw new Exception("Cadena de conexión no configurada.");
            _histRepo = new HistorialRepository(conn);
        }

        public IActionResult Index(string? dni)
        {
            ViewBag.FiltroDni = dni;

            if (!string.IsNullOrWhiteSpace(dni))
            {
                var paciente = _repo.ConsultarPacientePorDNI(dni);

                if (paciente == null)
                {
                    ViewBag.MsgBusqueda = "No se encontraron pacientes con ese DNI.";
                    return View(Enumerable.Empty<Paciente>());
                }

                // Devolvemos solo ese paciente en la lista
                return View(new List<Paciente> { paciente });
            }

            // Sin filtro → todos los pacientes
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

        // CUS 02: Registrar datos personales (Corregido para validar 5 campos: DNI, Nombres, Apellidos, FechaNacimiento, Telefono)
        [HttpPost]
        public IActionResult Crear(Paciente model)
        {
            // CUS 02 - Paso 5: El sistema valida que los 5 campos obligatorios estén completos
            if (string.IsNullOrWhiteSpace(model.DNI) ||
                string.IsNullOrWhiteSpace(model.Nombres) ||
                string.IsNullOrWhiteSpace(model.Apellidos) ||
                model.FechaNacimiento == null ||
                string.IsNullOrWhiteSpace(model.Telefono))
            {
                // Mensaje de error actualizado
                ViewBag.Error = "Complete los campos obligatorios (DNI, Nombres, Apellidos, Fecha de Nacimiento y Teléfono).";
                return View(model);
            }

            // Flujo Alterno (Paso 6): Validación de DNI existente
            var existente = _repo.ConsultarPacientePorDNI(model.DNI);
            if (existente != null)
            {
                ViewBag.Error = "Se encontraron coincidencias con otro DNI.";
                return View(model);
            }

            // Flujo Principal - Paso 7: Registro exitoso
            _repo.InsertarPaciente(model);
            TempData["msg"] = "Paciente registrado correctamente.";
            return RedirectToAction("Index");
        }

        // ================================================================
        //                     CUS 03: REGISTRAR HISTORIAL MÉDICO (ACCIONES)
        // ================================================================

        [HttpGet]
        public IActionResult CrearHistorial(string? dni)
        {
            // CUS 03 - Pasos 2, 3: Búsqueda y validación de existencia de paciente
            if (string.IsNullOrWhiteSpace(dni))
            {
                ViewBag.HistorialExistente = false;
                return View(new HistorialMedico());
            }

            var paciente = _repo.ConsultarPacientePorDNI(dni);
            if (paciente == null)
            {
                ViewBag.Error = "No existe un paciente registrado con ese DNI.";
                return View(new HistorialMedico());
            }

            var historialExistente = _histRepo.ObtenerPorDni(dni);

            if (historialExistente != null)
            {
                // Muestra error si ya existe el historial (único por paciente)
                ViewBag.Error = $"El paciente {paciente.NombreCompleto} ya tiene un historial clínico.";
                ViewBag.HistorialExistente = true;
                return View(historialExistente);
            }

            // CUS 03 - Paso 4: Muestra datos del paciente para confirmar antes de crear
            ViewBag.Paciente = paciente;
            ViewBag.HistorialExistente = false;

            // Devuelve el modelo HistorialMedico con DNI pre-cargado
            return View(new HistorialMedico { DNI_Paciente = dni, FechaCreacion = DateTime.Today });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CrearHistorial(HistorialMedico model)
        {
            if (string.IsNullOrWhiteSpace(model.DNI_Paciente))
            {
                ViewBag.Error = "Debe especificar el DNI del paciente.";
                return View(model);
            }

            // Verificación de concurrencia/duplicado
            if (_histRepo.ObtenerPorDni(model.DNI_Paciente) != null)
            {
                TempData["msgError"] = "Error: El paciente ya tiene un historial clínico registrado.";
                return RedirectToAction("Index");
            }

            // CUS 03 - Paso 9: El sistema registra el historial médico
            var nuevoHistorial = _histRepo.CrearHistorial(model.DNI_Paciente, model.ObservacionesIniciales);

            // CUS 03 - Paso 11: Muestra notificación de éxito
            TempData["msg"] = $"Historial clínico creado correctamente para DNI: {nuevoHistorial.DNI_Paciente} (ID: {nuevoHistorial.ID_Historial}).";
            return RedirectToAction("Index");
        }


        // ================================================================
        //                     ACCIONES ORIGINALES (Editar/Eliminar)
        // ================================================================

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