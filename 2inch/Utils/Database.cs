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

        private static string SQL_CONNECTION_STRING = "String";
        
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

        public async static Task<bool> VerifyAdminCredentials(Models.Auth login) //funkcia na porovanie hesla v databaze a zadaneho hesla
        {
            bool response = false;
            using (SqlConnection conn = new SqlConnection(SQL_CONNECTION_STRING))
            {
                string queryString = $"SELECT * FROM admin_table WHERE email=@user";
                using (SqlCommand command = new SqlCommand(queryString, conn))
                {
                    command.Parameters.AddWithValue("@user", login.Name);
                    await conn.OpenAsync();
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            response = reader.GetString(1) == login.Pass;
                            break;
                        }
                        await conn.CloseAsync();
                    }
                }
                return response;
            }
        }
    }
}
