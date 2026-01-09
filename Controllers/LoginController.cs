using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using WeatherApp.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WeatherApp.Controllers
{
    
    public class LoginController : Controller
    {
        private readonly IConfiguration _config;
        private readonly SqlConnection _connection;

        public LoginController(IConfiguration config, SqlConnection connection)
        {
            _config = config;
            _connection = connection;
        }
        public IActionResult Login()
        {
            return View();
        }
        private string HashPassword(string password)
        {
            using SHA256 sha = SHA256.Create();
            byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
        public IActionResult LoginUser(LoginModel login)
        {
            string hashedPassword = HashPassword(login.password);

            using SqlCommand cmd = new SqlCommand("LoginUser", _connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@username", login.username);
            cmd.Parameters.AddWithValue("@password", hashedPassword);
            _connection.Open();

            int result= (int)cmd.ExecuteScalar();
            if (result == -1)
            {
                return Json(new { success = false, message = "Username does not exists" });
            }
            else if (result == 0)
            {
                return Json(new { success = false, message = "Incorrect Password!!" });
            }
            else
            {
                HttpContext.Session.SetInt32("logid",result);
                return Json(new { success = true ,data=result,message="Login Success!!"});
                
            }


        }
    }
}
