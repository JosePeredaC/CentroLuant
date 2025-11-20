using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CentroLuant.DataAccess;
using CentroLuant.Models;
using Rotativa.AspNetCore;   // <- para PDF si usas Rotativa

namespace CentroLuant.Controllers
{
    [Authorize(Roles = "Administrador,Recepcionista")]
    public class FacturaController : Controller
    {
        private readonly PacienteRepository _repoPacientes;
        private readonly HistorialRepository _repoHistorial;
        private readonly FacturaRepository _repoFacturas;

        public FacturaController(IConfiguration config)
        {
            var conn = config.GetConnectionString("DefaultConnection");
            _repoPacientes = new PacienteRepository(conn);
            _repoHistorial = new HistorialRepository(conn);
            _repoFacturas = new FacturaRepository(conn);
        }

        // 🔎 Paso 1: búsqueda amigable por DNI o nombre
        [HttpGet]
        public IActionResult Index(string? termino)
        {
            var vm = new FacturaBuscarViewModel { Termino = termino };

            if (!string.IsNullOrWhiteSpace(termino))
            {
                var pacientes = _repoPacientes.BuscarPacientes(termino);
                vm.Resultados = pacientes;

                if (!pacientes.Any())
                {
                    vm.Mensaje = "No se encontraron pacientes con ese DNI o nombre.";
                }
            }

            return View(vm);
        }

        // Paso 2: servicios del paciente (esto ya lo tenías, solo ajustado al new tuple names)
        public IActionResult Servicios(string dni)
        {
            var paciente = _repoPacientes.ConsultarPacientePorDNI(dni);
            if (paciente == null) return NotFound();

            var historial = _repoHistorial.ObtenerPorDni(dni);
            if (historial == null)
            {
                ViewBag.Error = "El paciente no tiene historial clínico.";
                return View();
            }

            var tratamientos = _repoHistorial.ObtenerTratamientos(historial.ID_Historial);

            return View((Paciente: paciente, Tratamientos: tratamientos));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GenerarFactura(string dni, decimal total, string descripcion)
        {
            var factura = new Factura
            {
                DNI_Paciente = dni,
                FechaEmision = DateTime.Now,
                MontoTotal = total,
                DescripcionServicios = descripcion,
                EstadoPago = "Pendiente"
            };

            var id = _repoFacturas.CrearFactura(factura);

            return RedirectToAction("FacturaEmitida", new { id });
        }

        // Paso 4: mostrar factura bonita
        public IActionResult FacturaEmitida(int id)
        {
            var factura = _repoFacturas.ObtenerFactura(id);
            if (factura == null) return NotFound();

            var paciente = _repoPacientes.ConsultarPacientePorDNI(factura.DNI_Paciente);

            var vm = new FacturaEmitidaViewModel
            {
                Factura = factura,
                Paciente = paciente
            };

            return View(vm);
        }

        public IActionResult DescargarPdf(int id)
        {
            var factura = _repoFacturas.ObtenerFactura(id);
            if (factura == null) return NotFound();

            var paciente = _repoPacientes.ConsultarPacientePorDNI(factura.DNI_Paciente);

            var vm = new FacturaEmitidaViewModel
            {
                Factura = factura,
                Paciente = paciente
            };

            // 👇 ahora usamos la vista FacturaPdf
            return new ViewAsPdf("FacturaPdf", vm)
            {
                FileName = $"Factura_{id}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait
            };
        }
    }
}
