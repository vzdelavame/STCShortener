using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using _2inch.Controllers;
using System.Security.Cryptography; 
using System.Text; 

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

                using (SqlCommand changeClick = new SqlCommand(queryString, connection))
                {
                    changeClick.Parameters.AddWithValue("@clicked", clicked+1);
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

        public async static Task<Models.Auth> GetUserById(int id)
        {
            
            using (SqlConnection connection = new SqlConnection(SQL_CONNECTION_STRING)) 
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand(null, connection))
                {
                    command.CommandText = "SELECT * FROM userAccounts WHERE id = @id";
                    command.Parameters.AddWithValue("@id", id);
                    //command.Parameters.AddWithValue("@clicked", clicked+1); Should Update
                    using (SqlDataAdapter Adapter = new SqlDataAdapter(command))
                    {
                        DataTable table = new DataTable();

                        Adapter.Fill(table);

                        foreach (DataRow row in table.Rows)
                        { //Vyberame data z Table, vytvarame Objekty a populujeme ich informaciami
                            string userEmail = row["userEmail"].ToString();
                            int PermissionLevel = int.Parse(row["userPermission"].ToString());

                            Models.Auth authObj = new Models.Auth(userEmail, PermissionLevel);

                            return authObj;
                        }
                    }
                }
                return null;
            }
        }

        public async static Task<Models.Link> GetLinkByShortLink(string shortLink)
        {
            
            using (SqlConnection connection = new SqlConnection(SQL_CONNECTION_STRING)) 
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand(null, connection))
                {
                    command.CommandText = "SELECT * FROM links WHERE shortLink = @shortLink";
                    command.Parameters.AddWithValue("@shortLink", shortLink);

                    using (SqlDataAdapter Adapter = new SqlDataAdapter(command))
                    {
                        DataTable table = new DataTable();

                        Adapter.Fill(table);

                        foreach (DataRow row in table.Rows)
                        { //Vyberame data z Table, vytvarame Objekty a populujeme ich informaciami
                            int id = int.Parse(row["id"].ToString());
                            string createdBy = row["createdBy"].ToString();
                            string longLink = row["longLink"].ToString();
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

         public async static Task<Models.Link> GetLinkByLongLink(string longLink)
        {
            
            using (SqlConnection connection = new SqlConnection(SQL_CONNECTION_STRING)) 
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand(null, connection))
                {
                    command.CommandText = "SELECT * FROM links WHERE longLink = @longLink";
                    command.Parameters.AddWithValue("@longLink", longLink);

                    using (SqlDataAdapter Adapter = new SqlDataAdapter(command))
                    {
                        DataTable table = new DataTable();

                        Adapter.Fill(table);

                        foreach (DataRow row in table.Rows)
                        { //Vyberame data z Table, vytvarame Objekty a populujeme ich informaciami
                            int id = int.Parse(row["id"].ToString());
                            string createdBy = row["createdBy"].ToString();
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

        public async static Task<Models.Auth> VerifyAdminCredentials(Models.Auth login) //funkcia na porovanie hesla v databaze a zadaneho hesla
        {
            using (SqlConnection conn = new SqlConnection(SQL_CONNECTION_STRING))
            {
                string queryString = $"SELECT userPassword, userPermission FROM userAccounts WHERE userEmail = @user";
                using (SqlCommand command = new SqlCommand(queryString, conn))
                {
                    command.Parameters.AddWithValue("@user", login.Name);
                    await conn.OpenAsync();
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            if(reader.GetString(0) != ComputeSha256Hash(login.Pass))
                                return null;
                            login.Pass = "";
                            login.PermissionLevel = reader.GetInt32(1);
                            return login;
                        }
                    }
                }
                return null;
            }
        }

        static string ComputeSha256Hash(string rawData)  
        {  
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())  
            {  
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));  
  
                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();  
                for (int i = 0; i < bytes.Length; i++)  
                {  
                    builder.Append(bytes[i].ToString("x2"));  
                }  
                return builder.ToString();  
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

        public async static Task<List<Models.Link>> GetAllLinksByUser(string createdBy)
        {
            List<Models.Link> LinkList = new List<Models.Link>(); //List na vsetky rows

            using (SqlConnection conn = new SqlConnection(SQL_CONNECTION_STRING))
            {
                string queryString = "SELECT * FROM links WHERE createdBy = @createdBy";

                await conn.OpenAsync();

                using (SqlCommand getAll = new SqlCommand(queryString, conn))
                {
                    getAll.Parameters.AddWithValue("@createdBy", createdBy);
                    
                    using (SqlDataAdapter Adapter = new SqlDataAdapter(getAll))
                    {
                        DataTable table = new DataTable();

                        Adapter.Fill(table);

                        foreach (DataRow row in table.Rows)
                        { //Vyberame data z Table, vytvarame Objekty a populujeme ich informaciami
                            int id = int.Parse(row["id"].ToString());
                            string longLink = row["longLink"].ToString();
                            string shortLink = row["shortLink"].ToString();
                            int click = int.Parse(row["clicked"].ToString());
                            string[] creationTime = row["creationTime"].ToString().Split(" ");

                            Models.Link linkObj = new Models.Link(id, createdBy, longLink, shortLink, click, creationTime[0]);

                            LinkList.Add(linkObj); //Pridavame do Listu
                        }
                    }
                }
                return LinkList;
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
                            string longLink = row["longLink"].ToString();
                            string shortLink = row["shortLink"].ToString();
                            string createdBy = row["createdBy"].ToString();
                            int click = int.Parse(row["clicked"].ToString());
                            string[] creationTime = row["creationTime"].ToString().Split(" ");

                            Models.Link linkObj = new Models.Link(id, createdBy, longLink, shortLink, click, creationTime[0]);

                            LinkList.Add(linkObj); //Pridavame do Listu
                        }
                    }
                }
                return LinkList;
            }
        }

        public async static Task<List<Models.Auth>> GetAllUsers()
        {
            List<Models.Auth> AuthList = new List<Models.Auth>(); //List na vsetky rows

            using (SqlConnection conn = new SqlConnection(SQL_CONNECTION_STRING))
            {
                string queryString = "SELECT * FROM userAccounts";

                await conn.OpenAsync();

                using (SqlCommand getAll = new SqlCommand(queryString, conn))
                {   
                    using (SqlDataAdapter Adapter = new SqlDataAdapter(getAll))
                    {
                        DataTable table = new DataTable();

                        Adapter.Fill(table);

                        foreach (DataRow row in table.Rows)
                        { //Vyberame data z Table, vytvarame Objekty a populujeme ich informaciami
                            int PermissionLevel = int.Parse(row["userPermission"].ToString());
                            string userEmail = row["userEmail"].ToString();

                            Models.Auth authObj = new Models.Auth(userEmail, PermissionLevel);

                            AuthList.Add(authObj); //Pridavame do Listu
                        }
                    }
                }
                return AuthList;
            }
        }

        public async static Task<bool> DeleteLink(int id, string User)
        {
            using (SqlConnection conn = new SqlConnection(SQL_CONNECTION_STRING))
            {
                string queryString = "DELETE FROM links WHERE id = @id";

                    using (SqlCommand delete = new SqlCommand(queryString, conn))
                    {
                        delete.Parameters.AddWithValue("@id", id);

                        await delete.ExecuteNonQueryAsync();

                        return true;
                    }
            }
        }

        public async static Task<bool> DeleteUser(int id)
        {
            using (SqlConnection conn = new SqlConnection(SQL_CONNECTION_STRING))
            {
                string queryString = "DELETE FROM userAccounts WHERE id = @id";

                    using (SqlCommand delete = new SqlCommand(queryString, conn))
                    {
                        delete.Parameters.AddWithValue("@id", id);

                        await delete.ExecuteNonQueryAsync();

                        return true;
                    }
            }
        }

        public async static Task<bool> CheckOwner(string userEmail)
        {
            using (SqlConnection conn = new SqlConnection(SQL_CONNECTION_STRING))
            {
                string extractString = "SELECT id FROM userAccounts WHERE userEmail = @userEmail";

                await conn.OpenAsync();

                using (SqlCommand find = new SqlCommand(extractString, conn))
                {
                    find.Parameters.AddWithValue("@userEmail", userEmail);

                    using (SqlDataReader reader = await find.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        public async static Task<bool> EditLink(Models.Link link, string User, string newOwner)
        {
            using (SqlConnection conn = new SqlConnection(SQL_CONNECTION_STRING))
            { //Možno by bolo dobré implementovať kontrolu toho či sa LoggedInUser = createdBy a ak nie, tak nepovoliť edit?
                string queryString = "UPDATE links";
                queryString += " SET shortLink = @short, longLink = @long, createdBy = @newOwner WHERE id = @id";

                await conn.OpenAsync();

                using (SqlCommand edit = new SqlCommand(queryString, conn))
                {
                    edit.Parameters.AddWithValue("@short", link.shortLink);
                    edit.Parameters.AddWithValue("@long", link.longLink);
                    edit.Parameters.AddWithValue("@newOwner", newOwner);
                    edit.Parameters.AddWithValue("@id", link.id);

                    await edit.ExecuteNonQueryAsync();
                }
            }
            return true;
        }
    }
}
