using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense_Income_App
{
    public class DatabaseConnection
    {
        public static SqlConnection conn = null;

        public void Connect()
        {
            if (conn == null)
            {
                string connectionString = "Data Source=DESKTOP-1NKI4EO\\SQLEXPRESS;Initial Catalog=test3;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True;";
                conn = new SqlConnection(connectionString);
            }

            if (conn.State == System.Data.ConnectionState.Closed)
            {
                conn.Open();
            }
        }

        public void Disconnect()
        {
            if (conn != null && conn.State == System.Data.ConnectionState.Open)
            {
                conn.Close();
            }
        }
    }
}
