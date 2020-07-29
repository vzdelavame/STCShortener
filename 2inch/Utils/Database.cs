using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using _2inch.Controllers;

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
                int clicked = -1;
                int id = -1;
                string longLink = null;
                using (SqlCommand command = new SqlCommand(null, connection))
                {
                    command.CommandText = "SELECT * FROM links WHERE shortLink = @link";
                    command.Parameters.AddWithValue("@link", shortLink);
                    
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if(await reader.ReadAsync()) 
                        {
                            longLink = reader.GetString(3);

                            clicked = reader.GetInt32(4);
                            id = reader.GetInt32(0);
                        }
                    }
                }

                string queryString = "UPDATE links SET clicked = @clicked WHERE id = @id";

                using (SqlCommand changeClick = new SqlCommand(queryString, connection)) //Potentially Nuclear Material, Handle With Care
                {
                    int clickedincr = clicked++;
                    changeClick.Parameters.AddWithValue("@clicked", clickedincr);
                    changeClick.Parameters.AddWithValue("@id", id);

                    await changeClick.ExecuteNonQueryAsync();
                }

                return longLink;
            }
        }

        public async static Task<Models.Link> GetLinkById(int id)
        {
            
            using (SqlConnection connection = new SqlConnection(SQL_CONNECTION_STRING)) 
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand(null, connection))
                {
                    command.CommandText = "SELECT * FROM links WHERE id = @id";
                    command.Parameters.AddWithValue("@id", id);
                    //command.Parameters.AddWithValue("@clicked", clicked+1); Should Update
                    using (SqlDataAdapter Adapter = new SqlDataAdapter(command))
                    {
                        DataTable table = new DataTable();

                        Adapter.Fill(table);

                        foreach (DataRow row in table.Rows)
                        { //Vyberame data z Table, vytvarame Objekty a populujeme ich informaciami
                            string createdBy = row["createdBy"].ToString();
                            string longLink = row["longLink"].ToString();
                            string shortLink = row["shortLink"].ToString();
                            int click = int.Parse(row["clicked"].ToString());
                            string creationTime = row["creationTime"].ToString();

                            Models.Link linkObj = new Models.Link(id, createdBy, longLink, shortLink, click, creationTime);

                            return linkObj;
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

        public async static Task<bool> CheckDuples(string short_Link) //funkcia na zistenie duplikacie short url v databaze
        {
            using (SqlConnection connection = new SqlConnection(SQL_CONNECTION_STRING))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(null, connection))
                {
                    command.CommandText = "SELECT shortLink FROM links WHERE shortLink = @link";
                    command.Parameters.AddWithValue("@link", short_Link);
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            //string short_Link = reader.GetString(0);
                            return short_Link == reader.GetString(0);
                        }
                    }
                }
                return false;
            }
        }

        public async static Task<List<Models.Link>> GetAllLinks()
        {
            List<Models.Link> LinkList = new List<Models.Link>(); //List na vsetky rows

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
                        { //Vyberame data z Table, vytvarame Objekty a populujeme ich informaciami
                            int id = int.Parse(row["id"].ToString());
                            string createdBy = row["createdBy"].ToString();
                            string longLink = row["longLink"].ToString();
                            string shortLink = row["shortLink"].ToString();
                            int click = int.Parse(row["clicked"].ToString());
                            string creationTime = row["creationTime"].ToString();

                            Models.Link linkObj = new Models.Link(id, createdBy, longLink, shortLink, click, creationTime);

                            LinkList.Add(linkObj); //Pridavame do Listu
                        }
                    }
                }
                return LinkList;
            }
        }

        public async static Task DeleteLink(int id)
        {
            using (SqlConnection conn = new SqlConnection(SQL_CONNECTION_STRING))
            {
                string queryString = "DELETE FROM links WHERE id = @id";

                await conn.OpenAsync();

                using (SqlCommand delete = new SqlCommand(queryString, conn))
                {
                    delete.Parameters.AddWithValue("@id", id);

                    await delete.ExecuteNonQueryAsync();
                }
            }
        }

        public async static Task EditLink(Models.Link link)
        {
            using (SqlConnection conn = new SqlConnection(SQL_CONNECTION_STRING))
            { //Možno by bolo dobré implementovať kontrolu toho či sa LoggedInUser = createdBy a ak nie, tak nepovoliť edit?
                string queryString = "UPDATE links";
                queryString += " SET shortLink = @short, longLink = @long Where id = @id";

                await conn.OpenAsync();

                using (SqlCommand edit = new SqlCommand(queryString, conn))
                {
                    edit.Parameters.AddWithValue("@short", link.shortLink);
                    edit.Parameters.AddWithValue("@long", link.longLink);
                    edit.Parameters.AddWithValue("@id", link.id);

                    await edit.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
