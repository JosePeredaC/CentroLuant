using System.Data.SqlClient;
using CentroLuant.Models;

namespace CentroLuant.DataAccess
{
    public class FacturaRepository
    {
        private readonly string _connection;

        public FacturaRepository(string conn)
        {
            _connection = conn;
        }

        public int CrearFactura(Factura f)
        {
            const string sql = @"
                INSERT INTO Factura (DNI_Paciente, FechaEmision, MontoTotal, DescripcionServicios, EstadoPago)
                OUTPUT INSERTED.ID_Factura
                VALUES (@DNI, @Fecha, @Monto, @Desc, @Estado)";

            using var cn = new SqlConnection(_connection);
            using var cmd = new SqlCommand(sql, cn);

            cmd.Parameters.AddWithValue("@DNI", f.DNI_Paciente);
            cmd.Parameters.AddWithValue("@Fecha", f.FechaEmision);
            cmd.Parameters.AddWithValue("@Monto", f.MontoTotal);
            cmd.Parameters.AddWithValue("@Desc", (object?)f.DescripcionServicios ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Estado", f.EstadoPago);

            cn.Open();
            return (int)cmd.ExecuteScalar();
        }

        public Factura? ObtenerFactura(int id)
        {
            const string sql = @"SELECT * FROM Factura WHERE ID_Factura = @ID";

            using var cn = new SqlConnection(_connection);
            using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@ID", id);

            cn.Open();
            using var dr = cmd.ExecuteReader();

            if (!dr.Read()) return null;

            return new Factura
            {
                ID_Factura = (int)dr["ID_Factura"],
                DNI_Paciente = dr["DNI_Paciente"].ToString()!,
                FechaEmision = (DateTime)dr["FechaEmision"],
                MontoTotal = (decimal)dr["MontoTotal"],
                DescripcionServicios = dr["DescripcionServicios"].ToString(),
                EstadoPago = dr["EstadoPago"].ToString()!
            };
        }
    }
}
