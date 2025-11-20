namespace CentroLuant.Models
{
    public class Factura
    {
        public int ID_Factura { get; set; }
        public string DNI_Paciente { get; set; }
        public DateTime FechaEmision { get; set; }
        public decimal MontoTotal { get; set; }
        public string? DescripcionServicios { get; set; }
        public string EstadoPago { get; set; }
    }
}
