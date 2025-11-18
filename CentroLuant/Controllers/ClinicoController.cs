using CentroLuant.DataAccess;
using CentroLuant.Models;
using Microsoft.AspNetCore.Mvc;
using CentroLuant.DataAccess; // Para usar tus Repositorios
using CentroLuant.Models; // Para usar tus Modelos

namespace CentroLuant.Controllers
{
    public class ClinicoController : Controller
    {
        // Variables para guardar los repositorios
        private readonly PacienteRepository _pacienteRepo;
        private readonly HistorialRepository _historialRepo;

        // --- CONSTRUCTOR ---
        // El sistema "inyecta" automáticamente los repositorios que registramos en Program.cs
        public ClinicoController(PacienteRepository pacienteRepo, HistorialRepository historialRepo)
        {
            _pacienteRepo = pacienteRepo;
            _historialRepo = historialRepo;
        }

        // --- TAREA 1: Consultar Historial (CUS 06) ---
        // Este método se llamará cuando el Especialista busque un historial
        // GET: /Clinico/Historial
        public IActionResult Historial(string dni)
        {
            if (string.IsNullOrEmpty(dni))
            {
                // Si no se provee DNI, solo muestra la vista de búsqueda
                return View();
            }

            // 1. Usa el Repositorio para buscar el historial
            HistorialMedico historial = _historialRepo.ConsultarHistorialCompleto(dni);

            if (historial == null)
            {
                // Paciente no encontrado o no tiene historial
                ViewBag.Error = "No se encontró el historial para el DNI proporcionado.";
                return View();
            }

            // 2. Envía el modelo 'historial' (con sus tratamientos) a la Vista
            return View(historial);
        }

        // --- TAREA 2: Agregar Tratamiento (CUS 07) ---
        // Este método se llamará cuando el Especialista guarde un nuevo tratamiento
        // POST: /Clinico/AgregarTratamiento
        [HttpPost]
        public IActionResult AgregarTratamiento(Tratamiento nuevoTratamiento)
        {
            // Validamos que los datos del formulario sean correctos
            if (ModelState.IsValid)
                {
                // 1. Usa el Repositorio para insertar el tratamiento
                bool exito = _historialRepo.InsertarNuevoTratamiento(nuevoTratamiento);

                if (exito)
                {
                    // Si fue exitoso, redirige de vuelta al historial del paciente
                    // Necesitamos el DNI del paciente, que está en el Historial
                    var historial = _historialRepo.ConsultarHistorialCompleto(
                        _pacienteRepo.ConsultarPacientePorHistorial(nuevoTratamiento.ID_Historial).DNI
                    );
                    return RedirectToAction("Historial", new { dni = historial.DNI_Paciente });
                }
            }

            // Si hay un error, volvemos a mostrar la vista con los datos
            ViewBag.Error = "Error al guardar el tratamiento.";
            // (Necesitaríamos recargar el historial completo aquí)
            return View("Historial", nuevoTratamiento.ID_Historial);
        }

        // (Método de ejemplo para que el Especialista vea los pacientes)
        public IActionResult Index()
        {
            var listaPacientes = _pacienteRepo.ConsultarTodosLosPacientes();
            return View(listaPacientes);
        }
    }
}
