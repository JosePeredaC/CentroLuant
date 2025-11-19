namespace CentroLuant.Models
{
    public class Paciente
    {
        public string DNI { get; set; }
        public string Nombres { get; set; }
        public string Apellidos { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string? Direccion { get; set; }
        public string? Telefono { get; set; }
        public string? CorreoElectronico { get; set; }
        public string NombreCompleto => $"{Nombres} {Apellidos}";

    }
}
