using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace EnrrVa.Common
{
    public class DataConnection
    {
        public static string serverName;
        public static string dbName;

        public static SqlConnection GetOpenDataConnection()
        {
            string connstring = "server=" + serverName + ";Trusted_connection=yes;database=" + dbName;

            SqlConnection sCon = new SqlConnection(connstring);

            try
            {
                sCon.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return sCon;

        }
    }
}
