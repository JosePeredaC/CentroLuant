namespace CentroLuant.Models
{
    public class HistorialMedico
    {
        public int ID_Historial { get; set; }
        public string DNI_Paciente { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string? ObservacionesIniciales { get; set; }
    }
}
