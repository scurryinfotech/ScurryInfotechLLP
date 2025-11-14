using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Scurry_Infotech_LLP.Models;
using Microsoft.Extensions.Configuration;

namespace Scurry_Infotech_LLP.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _config;

        public HomeController(ILogger<HomeController> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

    
        [HttpPost]
        [Route("api/contact")]
        public IActionResult Contact([FromBody] ContactForm model)
        {
            try
            {
                
                if (model == null)
                {
                    model = new ContactForm
                    {
                        Name = Request.Form["name"],
                        Email = Request.Form["email"],
                        Phone = Request.Form["phone"],
                        Company = Request.Form["company"],
                        Service = Request.Form["service"],
                        Message = Request.Form["message"]
                    };
                }

               
                if (!TryValidateModel(model))
                {
                    return BadRequest(ModelState);
                }

               
                string connectionString = _config.GetConnectionString("DefaultConnection");

                using (var conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        INSERT INTO ContactForm 
                        (name, email, phone, company, service, message, isActive)
                        VALUES (@name, @email, @phone, @company, @service, @message, 1);
                    ";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", model.Name ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@email", model.Email ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@phone", model.Phone ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@company", model.Company ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@service", model.Service ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@message", model.Message ?? (object)DBNull.Value);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                _logger.LogInformation("Contact form submitted by {Name} ({Email}) for {Service}", model.Name, model.Email, model.Service);

                return Ok(new { success = true, message = "Data saved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving contact form data");
                return StatusCode(500, new { success = false, message = "Internal Server Error" });
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
