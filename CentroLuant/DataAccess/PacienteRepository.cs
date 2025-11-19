using System.Data.SqlClient;
using CentroLuant.Models;

namespace CentroLuant.DataAccess
{
    public class PacienteRepository
    {
        private readonly string _connectionString;

        public PacienteRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        // INSERTAR PACIENTE
        public bool InsertarPaciente(Paciente paciente)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                const string sql = @"
                    INSERT INTO Paciente 
                        (DNI, Nombres, Apellidos, FechaNacimiento, Direccion, Telefono, CorreoElectronico)
                    VALUES 
                        (@DNI, @Nombres, @Apellidos, @FechaNacimiento, @Direccion, @Telefono, @CorreoElectronico)";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@DNI", paciente.DNI);
                    command.Parameters.AddWithValue("@Nombres", paciente.Nombres);
                    command.Parameters.AddWithValue("@Apellidos", paciente.Apellidos);
                    command.Parameters.AddWithValue("@FechaNacimiento", (object?)paciente.FechaNacimiento ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Direccion", (object?)paciente.Direccion ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Telefono", (object?)paciente.Telefono ?? DBNull.Value);
                    command.Parameters.AddWithValue("@CorreoElectronico", (object?)paciente.CorreoElectronico ?? DBNull.Value);

                    connection.Open();
                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        // CONSULTAR POR DNI
        public Paciente? ConsultarPacientePorDNI(string dni)
        {
            Paciente? paciente = null;
            const string sql = "SELECT * FROM Paciente WHERE DNI = @DNI";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@DNI", dni);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        paciente = new Paciente
                        {
                            DNI = reader["DNI"].ToString()!,
                            Nombres = reader["Nombres"].ToString()!,
                            Apellidos = reader["Apellidos"].ToString()!,
                            FechaNacimiento = reader["FechaNacimiento"] as DateTime?,
                            Direccion = reader["Direccion"] as string,
                            Telefono = reader["Telefono"] as string,
                            CorreoElectronico = reader["CorreoElectronico"] as string
                        };
                    }
                }
            }

            return paciente;
        }

        // CONSULTAR TODOS
        public List<Paciente> ConsultarTodosLosPacientes()
        {
            var pacientes = new List<Paciente>();
            const string sql = "SELECT * FROM Paciente ORDER BY Apellidos, Nombres";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        pacientes.Add(new Paciente
                        {
                            DNI = reader["DNI"].ToString()!,
                            Nombres = reader["Nombres"].ToString()!,
                            Apellidos = reader["Apellidos"].ToString()!,
                            FechaNacimiento = reader["FechaNacimiento"] as DateTime?,
                            Direccion = reader["Direccion"] as string,
                            Telefono = reader["Telefono"] as string,
                            CorreoElectronico = reader["CorreoElectronico"] as string
                        });
                    }
                }
            }

            return pacientes;
        }

        // EDITAR PACIENTE
        public bool EditarPaciente(Paciente p)
        {
            using var cn = new SqlConnection(_connectionString);

            const string sql = @"
                UPDATE Paciente SET
                    Nombres = @Nombres,
                    Apellidos = @Apellidos,
                    FechaNacimiento = @FechaNacimiento,
                    Direccion = @Direccion,
                    Telefono = @Telefono,
                    CorreoElectronico = @CorreoElectronico
                WHERE DNI = @DNI";

            using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@DNI", p.DNI);
            cmd.Parameters.AddWithValue("@Nombres", p.Nombres);
            cmd.Parameters.AddWithValue("@Apellidos", p.Apellidos);
            cmd.Parameters.AddWithValue("@FechaNacimiento", (object?)p.FechaNacimiento ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Direccion", (object?)p.Direccion ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Telefono", (object?)p.Telefono ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CorreoElectronico", (object?)p.CorreoElectronico ?? DBNull.Value);

            cn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        // ELIMINAR PACIENTE
        public bool EliminarPaciente(string dni, out string? mensajeError)
        {
            mensajeError = null;

            using var cn = new SqlConnection(_connectionString);
            cn.Open();

            // 1) Verificar si tiene historial médico
            const string sqlHist = "SELECT COUNT(*) FROM Historial_Medico WHERE DNI_Paciente = @DNI";
            using (var cmdHist = new SqlCommand(sqlHist, cn))
            {
                cmdHist.Parameters.AddWithValue("@DNI", dni);
                int countHist = (int)cmdHist.ExecuteScalar();

                if (countHist > 0)
                {
                    mensajeError = "No se puede eliminar el paciente porque tiene un historial clínico registrado.";
                    return false;
                }
            }

            // 2) Verificar si tiene citas asociadas
            const string sqlCitas = "SELECT COUNT(*) FROM Cita WHERE DNI_Paciente = @DNI";
            using (var cmdCita = new SqlCommand(sqlCitas, cn))
            {
                cmdCita.Parameters.AddWithValue("@DNI", dni);
                int countCitas = (int)cmdCita.ExecuteScalar();

                if (countCitas > 0)
                {
                    mensajeError = "No se puede eliminar el paciente porque tiene citas registradas en el sistema.";
                    return false;
                }
            }

            // 3) Si no tiene historial ni citas → se puede borrar
            const string sqlDelete = "DELETE FROM Paciente WHERE DNI = @DNI";
            using (var cmdDel = new SqlCommand(sqlDelete, cn))
            {
                cmdDel.Parameters.AddWithValue("@DNI", dni);
                int rows = cmdDel.ExecuteNonQuery();
                return rows > 0;
            }
        }



        // OBTENER PACIENTE A PARTIR DE UN ID_HISTORIAL
        public Paciente? ConsultarPacientePorHistorial(int idHistorial)
        {
            const string sql = @"
                SELECT P.* 
                FROM Paciente P
                INNER JOIN Historial_Medico H ON P.DNI = H.DNI_Paciente
                WHERE H.ID_Historial = @ID_Historial";

            Paciente? paciente = null;

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@ID_Historial", idHistorial);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        paciente = new Paciente
                        {
                            DNI = reader["DNI"].ToString()!,
                            Nombres = reader["Nombres"].ToString()!,
                            Apellidos = reader["Apellidos"].ToString()!,
                            FechaNacimiento = reader["FechaNacimiento"] as DateTime?,
                            Direccion = reader["Direccion"] as string,
                            Telefono = reader["Telefono"] as string,
                            CorreoElectronico = reader["CorreoElectronico"] as string
                        };
                    }
                }
            }

            return paciente;
        }
        public List<Paciente> BuscarPacientes(string termino)
        {
            var pacientes = new List<Paciente>();

            const string sql = @"
        SELECT * 
        FROM Paciente
        WHERE DNI LIKE @Filtro
           OR Nombres LIKE @Filtro
           OR Apellidos LIKE @Filtro
        ORDER BY Apellidos, Nombres";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@Filtro", "%" + termino + "%");
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        pacientes.Add(new Paciente
                        {
                            DNI = reader["DNI"].ToString()!,
                            Nombres = reader["Nombres"].ToString()!,
                            Apellidos = reader["Apellidos"].ToString()!,
                            FechaNacimiento = reader["FechaNacimiento"] as DateTime?,
                            Direccion = reader["Direccion"] as string,
                            Telefono = reader["Telefono"] as string,
                            CorreoElectronico = reader["CorreoElectronico"] as string
                        });
                    }
                }
            }

            return pacientes;
        }
    }
}
