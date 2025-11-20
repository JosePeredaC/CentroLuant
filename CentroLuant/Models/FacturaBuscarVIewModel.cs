using System.Collections.Generic;

namespace CentroLuant.Models
{
    public class FacturaBuscarViewModel
    {
        public string? Termino { get; set; }
        public string? Mensaje { get; set; }

        public List<Paciente> Resultados { get; set; } = new();
    }
}
