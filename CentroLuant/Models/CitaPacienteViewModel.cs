using System.Collections.Generic;

namespace CentroLuant.Models
{
    public class CitaPacienteViewModel
    {
        // Datos del paciente
        public Paciente? Paciente { get; set; }

        // Citas del paciente
        public List<Cita> Citas { get; set; } = new List<Cita>();

        // Agenda general de citas (panel secundario)
        public List<Cita> Agenda { get; set; } = new List<Cita>();

        // Filtros / propiedades de UI
        public string? DniBusqueda { get; set; }
        public string? MensajeError { get; set; }

        // Fecha para el filtro (yyyy-MM-dd)
        public string? FechaFiltro { get; set; }
    }
}
