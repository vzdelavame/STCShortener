using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;

namespace _2inch
{
    public class database
    {
        public static string Database(string x)
        {
            SqlConnection sqlCon = new SqlConnection("sem vloz connection string");
            sqlCon.Open();
            string namedup = "SELECT * FROM links_table WHERE short_link = '" + x + "'";
            SqlDataAdapter sda = new SqlDataAdapter(namedup, sqlCon);
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
