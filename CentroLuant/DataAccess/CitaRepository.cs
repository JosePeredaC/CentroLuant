using System.Data.SqlClient;
using CentroLuant.Models;

namespace CentroLuant.DataAccess
{
    public class CitaRepository
    {
        private readonly string _connectionString;

        public CitaRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Listar citas (opcionalmente filtradas por fecha)
        public List<Cita> ObtenerCitas(DateTime? fecha = null)
        {
            var lista = new List<Cita>();

            string sql = @"
                SELECT C.ID_Cita, C.Fecha, C.Hora, C.Estado,
                       C.DNI_Paciente, C.ID_Especialista,
                       P.Nombres + ' ' + P.Apellidos AS PacienteNombre,
                       U.NombreCompleto AS EspecialistaNombre
                FROM Cita C
                INNER JOIN Paciente P ON C.DNI_Paciente = P.DNI
                INNER JOIN Usuario U ON C.ID_Especialista = U.ID_Usuario
            ";

            if (fecha.HasValue)
            {
                sql += " WHERE C.Fecha = @Fecha";
            }

            sql += " ORDER BY C.Fecha, C.Hora";

            using var cn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(sql, cn);

            if (fecha.HasValue)
                cmd.Parameters.AddWithValue("@Fecha", fecha.Value.Date);

            cn.Open();
            using var dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                lista.Add(new Cita
                {
                    ID_Cita = (int)dr["ID_Cita"],
                    Fecha = (DateTime)dr["Fecha"],
                    Hora = (TimeSpan)dr["Hora"],
                    Estado = dr["Estado"].ToString()!,
                    DNI_Paciente = dr["DNI_Paciente"].ToString()!,
                    ID_Especialista = (int)dr["ID_Especialista"],
                    PacienteNombreCompleto = dr["PacienteNombre"].ToString(),
                    EspecialistaNombre = dr["EspecialistaNombre"].ToString()
                });
            }

            return lista;
        }

        public Cita? ObtenerPorId(int id)
        {
            const string sql = @"
                SELECT C.ID_Cita, C.Fecha, C.Hora, C.Estado,
                       C.DNI_Paciente, C.ID_Especialista,
                       P.Nombres + ' ' + P.Apellidos AS PacienteNombre,
                       U.NombreCompleto AS EspecialistaNombre
                FROM Cita C
                INNER JOIN Paciente P ON C.DNI_Paciente = P.DNI
                INNER JOIN Usuario U ON C.ID_Especialista = U.ID_Usuario
                WHERE C.ID_Cita = @ID";

            using var cn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@ID", id);

            cn.Open();
            using var dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                return new Cita
                {
                    ID_Cita = (int)dr["ID_Cita"],
                    Fecha = (DateTime)dr["Fecha"],
                    Hora = (TimeSpan)dr["Hora"],
                    Estado = dr["Estado"].ToString()!,
                    DNI_Paciente = dr["DNI_Paciente"].ToString()!,
                    ID_Especialista = (int)dr["ID_Especialista"],
                    PacienteNombreCompleto = dr["PacienteNombre"].ToString(),
                    EspecialistaNombre = dr["EspecialistaNombre"].ToString()
                };
            }

            return null;
        }

        public bool CrearCita(Cita c, out string? mensajeError)
        {
            mensajeError = null;

            const string sql = @"
                INSERT INTO Cita (Fecha, Hora, Estado, DNI_Paciente, ID_Especialista)
                VALUES (@Fecha, @Hora, @Estado, @DNI_Paciente, @ID_Especialista)";

            using var cn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(sql, cn);

            cmd.Parameters.AddWithValue("@Fecha", c.Fecha.Date);
            cmd.Parameters.AddWithValue("@Hora", c.Hora);
            cmd.Parameters.AddWithValue("@Estado", c.Estado);
            cmd.Parameters.AddWithValue("@DNI_Paciente", c.DNI_Paciente);
            cmd.Parameters.AddWithValue("@ID_Especialista", c.ID_Especialista);

            try
            {
                cn.Open();
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    mensajeError = "Ya existe una cita para ese especialista en la misma fecha y hora.";
                    return false;
                }

                throw;
            }
        }

        public void CambiarEstado(int idCita, string nuevoEstado)
        {
            const string sql = "UPDATE Cita SET Estado = @Estado WHERE ID_Cita = @ID";

            using var cn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(sql, cn);

            cmd.Parameters.AddWithValue("@Estado", nuevoEstado);
            cmd.Parameters.AddWithValue("@ID", idCita);

            cn.Open();
            cmd.ExecuteNonQuery();
        }

        public void EliminarCita(int id)
        {
            const string sql = "DELETE FROM Cita WHERE ID_Cita = @ID";

            using var cn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(sql, cn);

            cmd.Parameters.AddWithValue("@ID", id);

            cn.Open();
            cmd.ExecuteNonQuery();
        }
    }
}
