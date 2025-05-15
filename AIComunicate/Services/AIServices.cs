using System.Runtime.InteropServices;
using System.Text;
using AIComunicate.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AIComunicate.Services
{
    public class AIServices
    {
        private const string _connectionString = "Data Source=chat.db";
        private void InitializeDb()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
        CREATE TABLE IF NOT EXISTS ChatMessages (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Message TEXT NOT NULL
        );";
            command.ExecuteNonQuery();
        }
        public async Task<Response<string>> SendMessage(string message)
        {
            //InitializeDb();
            SaveDB("User: "+message);
            var httpClient = new HttpClient();

            var requestBody = new
            {
                contents = new[]
                {
                new {
                    parts = new[] {
                        new { text = $"Lịch Sử chat : {GetHistory()}" + "User: " + message }
                    }
                }
            }
            };

            string json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            string apiKey = "AIzaSyD2lh31zNMIz5Vq7fg5ZtPW3QBOxFyatP0"; // <-- Thay bằng API key thật
            string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={apiKey}";

            var response = await httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var jObj = JObject.Parse(responseBody);
                // Truy cập thẳng vào text
                string text = (string)jObj["candidates"]?[0]?["content"]?["parts"]?[0]?["text"] ?? "No text found";
                SaveDB("Gemini: " + text);
                return new Response<string>
                {
                    Code = "00",
                    Value = text,
                    Message = "Send Message Done!",
                    Records = 1

                };
            }
            else
            {
                return new Response<string>
                {
                    Code = "01",
                    Message = $"Error: {response.StatusCode}",
                    Records = 0
                };
            }
        }
        public void SaveDB(string message)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "INSERT INTO ChatMessages (Message) VALUES ($msg)";
                command.Parameters.AddWithValue("$msg", message);
                command.ExecuteNonQuery();
            }
        }
        public string GetHistory()
        {
            string messages = "";
            using var connection = new SqliteConnection("Data Source=chat.db");
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT Message FROM ChatMessages ORDER BY Id;";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                messages += reader.GetString(0);
                //messages.Add(reader.GetString(0));
            }
            return messages;
        }
    }
}
