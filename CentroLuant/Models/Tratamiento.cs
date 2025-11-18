namespace CentroLuant.Models
{
    public class Tratamiento
    {
        public int ID_Tratamiento { get; set; }
        public int ID_Historial{ get; set; }
        public DateTime FechaTratamiento { get; set; }
        public string? Diagnostico{ get; set; }
        public string TipoTratamiento{ get; set; }
        public string? Observaciones { get; set; }
        public decimal Costo{ get; set; }
    }
}
