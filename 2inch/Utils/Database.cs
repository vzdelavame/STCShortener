using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;

namespace _2inch.Utils
{
    public class Database
    {
        private static readonly string SQL_CONNECTION_STRING = Environment.GetEnvironmentVariable("CUSTOMCONNSTR_Connection_String");

        public async static Task<string> GetLongString(string shortLink)
        {
            
            using (SqlConnection connection = new SqlConnection(SQL_CONNECTION_STRING)) 
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(null, connection))
                {
                    command.CommandText = "SELECT longLink FROM links WHERE shortLink = @link";
                    command.Parameters.AddWithValue("@link", shortLink);
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if(await reader.ReadAsync()) 
                        {
                            string longLink = reader.GetString(0);
                            return longLink;
                        }
                    }
                }
                return null;
            }
        }

        public async static Task<bool> VerifyAdminCredentials(Models.Auth login) //funkcia na porovanie hesla v databaze a zadaneho hesla
        {
            using (SqlConnection conn = new SqlConnection(SQL_CONNECTION_STRING))
            {
                string queryString = $"SELECT * FROM userAccounts WHERE userEmail = @user AND userPassword = @password";
                using (SqlCommand command = new SqlCommand(queryString, conn))
                {
                    command.Parameters.AddWithValue("@user", login.Name);
                    command.Parameters.AddWithValue("@password", login.Pass);
                    await conn.OpenAsync();
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            await conn.CloseAsync();
                            return true;
                        }
                    }
                }
                return false;
            }
        }
        public async static Task InsertLink(Models.Link link)
        {
            using (SqlConnection conn = new SqlConnection(SQL_CONNECTION_STRING))
            {
                //Toto by malo vložiť long_link a short_link, tieto názvy stĺpcov som používal podľa predošlích funkcii.
                string queryString = "INSERT INTO links (createdBy, shortLink, longLink)";
                queryString += " VALUES(@createdBy, @longLink, @shortLink)";

                await conn.OpenAsync();

                using (SqlCommand insert = new SqlCommand(queryString, conn))
                {
                    insert.Parameters.AddWithValue("@createdBy", link.createdBy);
                    insert.Parameters.AddWithValue("@longLink", link.longLink);
                    insert.Parameters.AddWithValue("@shortLink", link.shortLink);

                    await insert.ExecuteNonQueryAsync();
                    await conn.CloseAsync();
                }
            }
        }

        public async static Task GetAllLinks()
        {
            List<Models.Link> LinkList = new List<Models.Link>();

            using (SqlConnection conn = new SqlConnection(SQL_CONNECTION_STRING))
            {
                string queryString = "SELECT FROM links (id, createdBy, shortLink, longLink, clicked, creationTime)";
                queryString += " VALUES(@id, @createdBy, @shortLink, @longLink, @clicked, @creationTime)";

                await conn.OpenAsync();

                using (SqlCommand getAll = new SqlCommand(queryString, conn))
                {
                    getAll.Parameters.AddWithValue("@id", SqlDbType.Int);
                    getAll.Parameters.AddWithValue("@createdBy", SqlDbType.Text);
                    getAll.Parameters.AddWithValue("@shortLink", SqlDbType.Text);
                }
            }
        }
    }
}
