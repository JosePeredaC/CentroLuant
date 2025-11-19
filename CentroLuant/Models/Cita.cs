using System.ComponentModel.DataAnnotations;

namespace CentroLuant.Models
{
    public class Cita
    {
        public int ID_Cita { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Fecha { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan Hora { get; set; }

        public string Estado { get; set; } = "Programada";

        [Required]
        public string DNI_Paciente { get; set; }

        // OJO: ahora es ID_Usuario del especialista
        [Required]
        public int ID_Especialista { get; set; }

        // Para mostrar en la UI
        public string? PacienteNombreCompleto { get; set; }
        public string? EspecialistaNombre { get; set; }

        public DateTime FechaHora => Fecha.Date.Add(Hora);
    }
}