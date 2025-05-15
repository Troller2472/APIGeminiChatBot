using AIComunicate.Models;
using Microsoft.AspNetCore.Mvc;
using AIComunicate.Services;
using Microsoft.Data.Sqlite;

namespace AIComunicate.Controllers
{
    [ApiController]
    [Route("[controller]/[Action]")]
    public class CallAIController : Controller
    {
        [HttpPost]
        public IActionResult SendMessage(string message)
        {
            Response<string> result = new Response<string>();
            var respone = new AIServices().SendMessage(message);
            return Ok(respone.Result.Value);
        }
        [HttpGet]
        public IActionResult GetMessages()
        {
            var messages = new List<string>();
            using var connection = new SqliteConnection("Data Source=chat.db");
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT Message FROM ChatMessages ORDER BY Id;";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                messages.Add(reader.GetString(0));
            }
            return Ok(messages);
        }
    }
}
