namespace CentroLuant.Models
{
    public class HistorialViewModel
    {
        public Paciente? Paciente { get; set; }
        public HistorialMedico? Historial { get; set; }
        public List<Tratamiento> Tratamientos { get; set; } = new();
        public Tratamiento NuevoTratamiento { get; set; } = new();
    }
}
