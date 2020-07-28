using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;

namespace _2inch.Utils
{
    public class Database
    {

        private static string SQL_CONNECTION_STRING = "sem vloz connection string";
        public static string getLongLink(string shortLink)
        {
            using (SqlConnection connection = new SqlConnection(SQL_CONNECTION_STRING)) {
                SqlCommand command = new SqlCommand(null, connection);
                command.CommandText = "SELECT * FROM links_table WHERE short_link = '@link'";
                command.Parameters.Add(new SqlParameter("@link", shortLink));
                command.Prepare();

                SqlDataAdapter sda = new SqlDataAdapter(command);
                DataTable dtbl = new DataTable();
                sda.Fill(dtbl);
                if (dtbl.Rows.Count == 1)
                {
                    foreach (DataRow row in dtbl.Rows)
                    {
                        string long_link = row["long_link"].ToString();
                        return long_link;
                    }
                }
                return null;
            }
        }
    }
}
