using System.Data.SqlClient;
using CentroLuant.Models;

namespace CentroLuant.DataAccess
{
    public class HistorialRepository
    {
        private readonly string _connectionString;

        public HistorialRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Obtener historial por DNI (si no existe, null)
        public HistorialMedico? ObtenerPorDni(string dni)
        {
            const string sql = @"
                SELECT ID_Historial, DNI_Paciente, FechaCreacion, ObservacionesIniciales
                FROM Historial_Medico
                WHERE DNI_Paciente = @DNI";

            using var cn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@DNI", dni);

            cn.Open();
            using var dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                return new HistorialMedico
                {
                    ID_Historial = (int)dr["ID_Historial"],
                    DNI_Paciente = dr["DNI_Paciente"].ToString()!,
                    FechaCreacion = (DateTime)dr["FechaCreacion"],
                    ObservacionesIniciales = dr["ObservacionesIniciales"] as string
                };
            }

            return null;
        }

        // Crear historial si no existe
        public HistorialMedico CrearHistorial(string dni, string? observacionesIniciales = null)
        {
            const string sql = @"
                INSERT INTO Historial_Medico (DNI_Paciente, FechaCreacion, ObservacionesIniciales)
                OUTPUT INSERTED.ID_Historial
                VALUES (@DNI, @Fecha, @Obs)";

            using var cn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(sql, cn);

            cmd.Parameters.AddWithValue("@DNI", dni);
            cmd.Parameters.AddWithValue("@Fecha", DateTime.Now.Date);
            cmd.Parameters.AddWithValue("@Obs", (object?)observacionesIniciales ?? DBNull.Value);

            cn.Open();
            int id = (int)cmd.ExecuteScalar();

            return new HistorialMedico
            {
                ID_Historial = id,
                DNI_Paciente = dni,
                FechaCreacion = DateTime.Now.Date,
                ObservacionesIniciales = observacionesIniciales
            };
        }

        public List<Tratamiento> ObtenerTratamientos(int idHistorial)
        {
            var lista = new List<Tratamiento>();

            const string sql = @"
                SELECT ID_Tratamiento, ID_Historial, FechaTratamiento,
                       Diagnostico, TipoTratamiento, Observaciones, Costo
                FROM Tratamiento
                WHERE ID_Historial = @ID
                ORDER BY FechaTratamiento DESC, ID_Tratamiento DESC";

            using var cn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@ID", idHistorial);

            cn.Open();
            using var dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                lista.Add(new Tratamiento
                {
                    ID_Tratamiento = (int)dr["ID_Tratamiento"],
                    ID_Historial = (int)dr["ID_Historial"],
                    FechaTratamiento = (DateTime)dr["FechaTratamiento"],
                    Diagnostico = dr["Diagnostico"] as string,
                    TipoTratamiento = dr["TipoTratamiento"].ToString()!,
                    Observaciones = dr["Observaciones"] as string,
                    Costo = (decimal)dr["Costo"]
                });
            }

            return lista;
        }

        public void AgregarTratamiento(Tratamiento t)
        {
            const string sql = @"
                INSERT INTO Tratamiento
                    (ID_Historial, FechaTratamiento, Diagnostico, TipoTratamiento, Observaciones, Costo)
                VALUES
                    (@ID_H, @Fecha, @Diag, @Tipo, @Obs, @Costo)";

            using var cn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(sql, cn);

            cmd.Parameters.AddWithValue("@ID_H", t.ID_Historial);
            cmd.Parameters.AddWithValue("@Fecha", t.FechaTratamiento.Date);
            cmd.Parameters.AddWithValue("@Diag", (object?)t.Diagnostico ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Tipo", t.TipoTratamiento);
            cmd.Parameters.AddWithValue("@Obs", (object?)t.Observaciones ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Costo", t.Costo);

            cn.Open();
            cmd.ExecuteNonQuery();
        }
    }
}
