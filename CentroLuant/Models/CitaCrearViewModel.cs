using System;
using System.Collections.Generic;

namespace CentroLuant.Models
{
    public class CitaCrearViewModel
    {
        public DateTime? FechaSeleccionada { get; set; }
        public TimeSpan? HoraSeleccionada { get; set; }
        public string? DNI_Paciente { get; set; }
        public int ID_Especialista { get; set; }
        public string? Estado { get; set; } = "Programada";
        public List<TimeSpan> HorariosDisponibles { get; set; } = new();
        public List<Usuario> Especialistas { get; set; } = new();
        public string? MensajeError { get; set; }
    }
}
