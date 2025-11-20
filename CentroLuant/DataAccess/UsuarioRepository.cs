using System.Data.SqlClient;
using CentroLuant.Models;

namespace CentroLuant.DataAccess
{
    public class UsuarioRepository
    {
        private readonly string _connectionString;

        public UsuarioRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<Usuario> ObtenerUsuarios()
        {
            var lista = new List<Usuario>();

            using (var cn = new SqlConnection(_connectionString))
            {
                cn.Open();
                string sql = "SELECT * FROM Usuario";
                using (var cmd = new SqlCommand(sql, cn))
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new Usuario
                        {
                            ID_Usuario = (int)dr["ID_Usuario"],
                            NombreCompleto = dr["NombreCompleto"].ToString(),
                            UsuarioLogin = dr["UsuarioLogin"].ToString(),
                            ContrasenaHash = dr["ContrasenaHash"].ToString(),
                            Rol = dr["Rol"].ToString(),
                            Activo = (bool)dr["Activo"]
                        });
                    }
                }
            }
            return lista;
        }

        public void CrearUsuario(Usuario u)
        {
            using (var cn = new SqlConnection(_connectionString))
            {
                cn.Open();
                string sql = @"INSERT INTO Usuario 
                    (NombreCompleto, UsuarioLogin, ContrasenaHash, Rol, Activo)
                    VALUES (@Nombre, @Login, @Pass, @Rol, @Activo)";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@Nombre", u.NombreCompleto);
                    cmd.Parameters.AddWithValue("@Login", u.UsuarioLogin);
                    cmd.Parameters.AddWithValue("@Pass", u.ContrasenaHash);
                    cmd.Parameters.AddWithValue("@Rol", u.Rol);
                    cmd.Parameters.AddWithValue("@Activo", u.Activo);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Obtener por ID
        public Usuario ObtenerPorId(int id)
        {
            Usuario u = null;

            using (var cn = new SqlConnection(_connectionString))
            {
                cn.Open();
                string sql = "SELECT * FROM Usuario WHERE ID_Usuario = @ID";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@ID", id);

                    using (var dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            u = new Usuario
                            {
                                ID_Usuario = (int)dr["ID_Usuario"],
                                NombreCompleto = dr["NombreCompleto"].ToString(),
                                UsuarioLogin = dr["UsuarioLogin"].ToString(),
                                ContrasenaHash = dr["ContrasenaHash"].ToString(),
                                Rol = dr["Rol"].ToString(),
                                Activo = (bool)dr["Activo"]
                            };
                        }
                    }
                }
            }
            return u;
        }

        // Editar usuario
        public void EditarUsuario(Usuario u)
        {
            using (var cn = new SqlConnection(_connectionString))
            {
                cn.Open();
                string sql = @"UPDATE Usuario SET
                               NombreCompleto = @Nombre,
                               UsuarioLogin = @Login,
                               Rol = @Rol,
                               Activo = @Activo
                               WHERE ID_Usuario = @ID";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@Nombre", u.NombreCompleto);
                    cmd.Parameters.AddWithValue("@Login", u.UsuarioLogin);
                    cmd.Parameters.AddWithValue("@Rol", u.Rol);
                    cmd.Parameters.AddWithValue("@Activo", u.Activo);
                    cmd.Parameters.AddWithValue("@ID", u.ID_Usuario);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Desactivar usuario
        public void DesactivarUsuario(int id)
        {
            using (var cn = new SqlConnection(_connectionString))
            {
                cn.Open();
                string sql = "UPDATE Usuario SET Activo = 0 WHERE ID_Usuario = @ID";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@ID", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Resetear contraseña
        public void ResetPassword(int id, string nuevaPassHash)
        {
            using (var cn = new SqlConnection(_connectionString))
            {
                cn.Open();
                string sql = "UPDATE Usuario SET ContrasenaHash = @Pass WHERE ID_Usuario = @ID";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@Pass", nuevaPassHash);
                    cmd.Parameters.AddWithValue("@ID", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Validar Login
        public Usuario Login(string usuario, string passHash)
        {
            using (var cn = new SqlConnection(_connectionString))
            {
                cn.Open();
                string sql = @"SELECT * FROM Usuario
                               WHERE UsuarioLogin = @U AND ContrasenaHash = @P AND Activo = 1";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@U", usuario);
                    cmd.Parameters.AddWithValue("@P", passHash);

                    using (var dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            return new Usuario
                            {
                                ID_Usuario = (int)dr["ID_Usuario"],
                                NombreCompleto = dr["NombreCompleto"].ToString(),
                                UsuarioLogin = dr["UsuarioLogin"].ToString(),
                                ContrasenaHash = dr["ContrasenaHash"].ToString(),
                                Rol = dr["Rol"].ToString(),
                                Activo = (bool)dr["Activo"]
                            };
                        }
                    }
                }
            }
            return null;
        }
        public int ContarAdminsActivos()
        {
            using (var cn = new SqlConnection(_connectionString))
            {
                cn.Open();
                string sql = "SELECT COUNT(*) FROM Usuario WHERE Rol = 'Administrador' AND Activo = 1";
                using (var cmd = new SqlCommand(sql, cn))
                {
                    return (int)cmd.ExecuteScalar();
                }
            }
        }
        public void ActivarUsuario(int id)
        {
            using (var cn = new SqlConnection(_connectionString))
            {
                cn.Open();
                string sql = "UPDATE Usuario SET Activo = 1 WHERE ID_Usuario = @ID";
                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@ID", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public void EliminarUsuario(int id)
        {
            using (var cn = new SqlConnection(_connectionString))
            {
                cn.Open();
                string sql = "DELETE FROM Usuario WHERE ID_Usuario = @ID";
                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@ID", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public List<Usuario> ObtenerEspecialistasActivos()
        {
            var lista = new List<Usuario>();

            using (var cn = new SqlConnection(_connectionString))
            {
                cn.Open();
                string sql = @"SELECT * FROM Usuario 
                       WHERE Rol = 'Especialista' AND Activo = 1
                       ORDER BY NombreCompleto";

                using (var cmd = new SqlCommand(sql, cn))
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new Usuario
                        {
                            ID_Usuario = (int)dr["ID_Usuario"],
                            NombreCompleto = dr["NombreCompleto"].ToString(),
                            UsuarioLogin = dr["UsuarioLogin"].ToString(),
                            ContrasenaHash = dr["ContrasenaHash"].ToString(),
                            Rol = dr["Rol"].ToString(),
                            Activo = (bool)dr["Activo"]
                        });
                    }
                }
            }

            return lista;
        }

    }
}
