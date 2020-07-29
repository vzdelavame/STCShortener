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
                string queryString = "INSERT INTO links (createdBy, shortLink, longLink, clicked)";
                queryString += " VALUES(@createdBy, @shortLink, @longLink, @clicked)";

                await conn.OpenAsync();

                using (SqlCommand insert = new SqlCommand(queryString, conn))
                {
                    insert.Parameters.AddWithValue("@createdBy", link.createdBy);
                    insert.Parameters.AddWithValue("@longLink", link.longLink);
                    insert.Parameters.AddWithValue("@shortLink", link.shortLink);
                    insert.Parameters.AddWithValue("@clicked", link.clicked);

                    await insert.ExecuteNonQueryAsync();
                    await conn.CloseAsync();
                }
            }
        }

        public async static Task<List<Models.Link>> GetAllLinks()
        {
            List<Models.Link> LinkList = new List<Models.Link>();

            using (SqlConnection conn = new SqlConnection(SQL_CONNECTION_STRING))
            {
                string queryString = "SELECT * FROM links";

                await conn.OpenAsync();

                using (SqlCommand getAll = new SqlCommand(queryString, conn))
                {
                    
                    using (SqlDataAdapter Adapter = new SqlDataAdapter(getAll))
                    {
                        DataTable table = new DataTable();

                        Adapter.Fill(table);

                        foreach (DataRow row in table.Rows)
                        {
                            int id = int.Parse(row["id"].ToString());
                            string createdBy = row["createdBy"].ToString();
                            string longLink = row["longLink"].ToString();
                            string shortLink = row["shortLink"].ToString();
                            int click = int.Parse(row["clicked"].ToString());
                            string creationTime = row["creationTime"].ToString();

                            Models.Link linkObj = new Models.Link(id, createdBy, longLink, shortLink, click, creationTime);

                            LinkList.Add(linkObj);
                        }
                    }
                }
                return LinkList;
            }
        }
    }
}
