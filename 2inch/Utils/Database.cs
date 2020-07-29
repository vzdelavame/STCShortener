﻿using System;
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

        private const string sqlConnString = "Connection_String";
        private static readonly string SQL_CONNECTION_STRING = ConfigurationManager.ConnectionStrings[sqlConnString].ConnectionString;

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
<<<<<<< HEAD
=======
            
            bool response = false;
>>>>>>> 5d61b76876022e939bcddb31bef98ac9eeb624ce
            using (SqlConnection conn = new SqlConnection(SQL_CONNECTION_STRING))
            {
                string queryString = $"SELECT * FROM userAccounts WHERE userEmail = @user AND userPassword = HASHBYTES('SHA2_512', @password)";
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
<<<<<<< HEAD
=======
            
>>>>>>> 5d61b76876022e939bcddb31bef98ac9eeb624ce
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
    }
}
