using CentroLuant.Models;
using System.Data;
using System.Data.SqlClient;

namespace CentroLuant.DataAccess
{
    public class HistorialRepository
    {
        private readonly string _connectionString;

        public HistorialRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // --- TAREA 1: Consultar Historial Completo (CUS 06) ---
        // (Este método es más complejo porque une dos tablas)
        public HistorialMedico ConsultarHistorialCompleto(string dni)
        {
            HistorialMedico historial = null;
            string sqlHistorial = "SELECT * FROM Historial_Medico WHERE DNI_Paciente = @DNI";
            string sqlTratamientos = "SELECT * FROM Tratamiento WHERE ID_Historial = @ID_Historial ORDER BY FechaTratamiento DESC";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // 1. Obtener el Historial principal
                using (SqlCommand cmdHistorial = new SqlCommand(sqlHistorial, connection))
                {
                    cmdHistorial.Parameters.AddWithValue("@DNI", dni);
                    using (SqlDataReader reader = cmdHistorial.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            historial = new HistorialMedico
                            {
                                ID_Historial = (int)reader["ID_Historial"],
                                DNI_Paciente = reader["DNI_Paciente"].ToString(),
                                FechaCreacion = (DateTime)reader["FechaCreacion"],
                                ObservacionesIniciales = reader["ObservacionesIniciales"] as string
                            };
                        }
                    } // El reader se cierra aquí
                }

                // 2. Si encontramos un historial, buscar sus tratamientos
                if (historial != null)
                {
                    using (SqlCommand cmdTratamientos = new SqlCommand(sqlTratamientos, connection))
                    {
                        cmdTratamientos.Parameters.AddWithValue("@ID_Historial", historial.ID_Historial);
                        using (SqlDataReader reader = cmdTratamientos.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Tratamiento t = new Tratamiento
                                {
                                    ID_Tratamiento = (int)reader["ID_Tratamiento"],
                                    ID_Historial = (int)reader["ID_Historial"],
                                    FechaTratamiento = (DateTime)reader["FechaTratamiento"],
                                    Diagnostico = reader["Diagnostico"] as string,
                                    TipoTratamiento = reader["TipoTratamiento"].ToString(),
                                    Observaciones = reader["Observaciones"] as string,
                                    Costo = (decimal)reader["Costo"]
                                };
                                historial.Tratamientos.Add(t);
                            }
                        }
                    }
                }
            }
            return historial; // Devuelve el historial con su lista de tratamientos
        }

        // --- TAREA 2: Insertar un Nuevo Tratamiento (CUS 07) ---
        public bool InsertarNuevoTratamiento(Tratamiento tratamiento)
        {
            string sql = @"INSERT INTO Tratamiento (ID_Historial, FechaTratamiento, Diagnostico, TipoTratamiento, Observaciones, Costo)
                           VALUES (@ID_Historial, @FechaTratamiento, @Diagnostico, @TipoTratamiento, @Observaciones, @Costo)";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@ID_Historial", tratamiento.ID_Historial);
                    command.Parameters.AddWithValue("@FechaTratamiento", tratamiento.FechaTratamiento);
                    command.Parameters.AddWithValue("@Diagnostico", (object)tratamiento.Diagnostico ?? DBNull.Value);
                    command.Parameters.AddWithValue("@TipoTratamiento", tratamiento.TipoTratamiento);
                    command.Parameters.AddWithValue("@Observaciones", (object)tratamiento.Observaciones ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Costo", tratamiento.Costo);

                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }

        // (Nota: Los métodos para 'ActualizarTratamiento' o 'InsertarHistorial' seguirían un patrón similar)
    }
}
