namespace CentroLuant.Models
{
    public class HistorialMedico
    {
        public int ID_Historial { get; set; }
        public string DNI_Paciente { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string? ObservacionesIniciales{ get; set; }
        public List<Tratamiento> Tratamientos { get; set; } = new List<Tratamiento>();
    }
}
