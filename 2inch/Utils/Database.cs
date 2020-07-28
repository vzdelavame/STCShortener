using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;

namespace _2inch
{
    public class Database
    {

        private static string SQL_CONNECTION_STRING = "sem vloz connection string";
        
        public static string getLongLink(string shortLink)
        {
            using (SqlConnection connection = new SqlConnection(SQL_CONNECTION_STRING)) 
            {
                using (SqlCommand command = new SqlCommand(null, connection))
                {
                    command.CommandText = "SELECT long_link FROM links_table WHERE short_link = '@link'";
                    command.Parameters.Add(new SqlParameter("@link", shortLink));
                    command.Prepare();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if(reader.Read()) 
                        {
                            string longLink = reader.GetString(0);
                            return longLink;
                        }
                    }
                }
                return null;
            }
        }
    }
}
