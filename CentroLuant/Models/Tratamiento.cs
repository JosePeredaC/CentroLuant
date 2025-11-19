using System.ComponentModel.DataAnnotations;

namespace CentroLuant.Models
{
    public class Tratamiento
    {
        public int ID_Tratamiento { get; set; }

        [Required]
        public int ID_Historial { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime FechaTratamiento { get; set; }

        [MaxLength(100)]
        public string? Diagnostico { get; set; }

        [Required]
        [MaxLength(100)]
        public string TipoTratamiento { get; set; }

        public string? Observaciones { get; set; }

        [Required]
        [Range(0, 999999)]
        public decimal Costo { get; set; }

        // Solo para redirección desde el formulario
        public string? DNI_Paciente { get; set; }
    }
}
