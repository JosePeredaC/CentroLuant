using System.ComponentModel.DataAnnotations;

namespace CentroLuant.Models
{
    public class Usuario
    {
        public int ID_Usuario { get; set; }

        [Required]
        public string NombreCompleto { get; set; }

        [Required]
        public string UsuarioLogin { get; set; }

        [Required]
        public string ContrasenaHash { get; set; }

        [Required]
        public string Rol { get; set; } // Administrador, Recepcionista, Especialista

        public bool Activo { get; set; } = true;
    }
}
