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

        private static string SQL_CONNECTION_STRING = "Server=tcp:zaverecnyprojekt.database.windows.net,1433;Initial Catalog = shortenerletnaskola; Persist Security Info=False;User ID = andrejmokris; Password=ZaverecnapracaSTC2019;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout = 30;";
        
        public static string getLongLink(string shortLink)
        {
            using (SqlConnection connection = new SqlConnection(SQL_CONNECTION_STRING)) 
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(null, connection))
                {
                    command.CommandText = "SELECT * FROM links_table WHERE short_link = @link";
                    command.Parameters.AddWithValue("@link", shortLink);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if(reader.Read()) 
                        {
                            string longLink = reader.GetString(1);
                            return longLink;
                        }
                    }
                }
                return null;
            }
        }
    }
}
