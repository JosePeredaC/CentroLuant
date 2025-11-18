using CentroLuant.Models; // Necesitamos usar la clase Paciente
using System.Data; // Para los tipos de datos como 'SqlDbType'
using System.Data.SqlClient; // Para hablar con SQL Server
using Microsoft.Extensions.Configuration;

namespace CentroLuant.DataAccess
{
    public class PacienteRepository
    {
        private readonly string _connectionString;

        // Constructor: Recibe la cadena de conexión
        // (Más adelante la inyectaremos automáticamente)
        public PacienteRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // --- TAREA 1: Insertar Paciente (CUS 02) ---
        public bool InsertarPaciente(Paciente paciente)
        {
            // Usamos 'using' para asegurar que la conexión se cierre
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                // 1. Comando T-SQL con parámetros por seguridad
                string sql = @"INSERT INTO Paciente (DNI, Nombres, Apellidos, FechaNacimiento, Direccion, Telefono, CorreoElectronico)
                               VALUES (@DNI, @Nombres, @Apellidos, @FechaNacimiento, @Direccion, @Telefono, @Correo)";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    // 2. Asignar valores a los parámetros
                    command.Parameters.AddWithValue("@DNI", paciente.DNI);
                    command.Parameters.AddWithValue("@Nombres", paciente.Nombres);
                    command.Parameters.AddWithValue("@Apellidos", paciente.Apellidos);

                    // Manejo de valores nulos
                    command.Parameters.AddWithValue("@FechaNacimiento", (object)paciente.FechaNacimiento ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Direccion", (object)paciente.Direccion ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Telefono", (object)paciente.Telefono ?? DBNull.Value);
                    command.Parameters.AddWithValue("@CorreoElectronico", (object)paciente.CorreoElectronico ?? DBNull.Value);

                    // 3. Abrir conexión y ejecutar
                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();

                    // 4. Retornar éxito
                    return rowsAffected > 0;
                }
            }
        }

        // --- TAREA 2: Consultar Paciente por DNI (CUS 05) ---
        public Paciente ConsultarPacientePorDNI(string dni)
        {
            Paciente paciente = null;
            string sql = "SELECT * FROM Paciente WHERE DNI = @DNI";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@DNI", dni);
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read()) // Si se encontró un registro
                        {
                            paciente = new Paciente
                            {
                                DNI = reader["DNI"].ToString(),
                                Nombres = reader["Nombres"].ToString(),
                                Apellidos = reader["Apellidos"].ToString(),
                                FechaNacimiento = reader["FechaNacimiento"] as DateTime?,
                                Direccion = reader["Direccion"] as string,
                                Telefono = reader["Telefono"] as string,
                                CorreoElectronico = reader["CorreoElectronico"] as string
                            };
                        }
                    }
                }
            }
            return paciente;
        }

        // --- TAREA 3: Consultar Todos los Pacientes ---
        public List<Paciente> ConsultarTodosLosPacientes()
        {
            List<Paciente> pacientes = new List<Paciente>();
            string sql = "SELECT * FROM Paciente ORDER BY Apellidos";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            pacientes.Add(new Paciente
                            {
                                DNI = reader["DNI"].ToString(),
                                Nombres = reader["Nombres"].ToString(),
                                Apellidos = reader["Apellidos"].ToString(),
                                FechaNacimiento = reader["FechaNacimiento"] as DateTime?,
                                Direccion = reader["Direccion"] as string,
                                Telefono = reader["Telefono"] as string,
                                CorreoElectronico = reader["CorreoElectronico"] as string
                            });
                        }
                    }
                }
            }
            return pacientes;
        }
        public Paciente ConsultarPacientePorHistorial(int idHistorial)
        {
            // Usamos un JOIN para encontrar al paciente
            string sql = @"SELECT P.* FROM Paciente P
                   JOIN Historial_Medico H ON P.DNI = H.DNI_Paciente
                   WHERE H.ID_Historial = @ID_Historial";

            Paciente paciente = null;
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@ID_Historial", idHistorial);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // (Reutilizamos el código de 'ConsultarPacientePorDNI' para mapear)
                            paciente = new Paciente
                            {
                                DNI = reader["DNI"].ToString(),
                                Nombres = reader["Nombres"].ToString(),
                                Apellidos = reader["Apellidos"].ToString(),
                                // ... (llenar los demás campos si es necesario)
                            };
                        }
                    }
                }
            }
            return paciente;
        }

    }
}
