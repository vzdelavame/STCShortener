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

        private static readonly string SQL_CONNECTION_STRING = ConfigurationManager.ConnectionStrings["Connection_String"].ConnectionString;
        //Malo by to ťahať z Web App Settings!1!!1!!
        
        public async static Task<string> GetLongString(string shortLink)
        {
            Console.WriteLine(SQL_CONNECTION_STRING);
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
            bool response = false;
            using (SqlConnection conn = new SqlConnection(SQL_CONNECTION_STRING))
            {
                string queryString = $"SELECT * FROM userAccounts WHERE userEmail = @user";
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
    }
}
