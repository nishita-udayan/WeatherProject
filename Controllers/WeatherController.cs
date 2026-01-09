using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;


//using Newtonsoft.Json;
using System.Text.Json;

using WeatherApp.Models;

namespace WeatherApp.Controllers
{
    public class WeatherController : Controller
    {
        private readonly IHttpClientFactory _factory;
        private readonly IConfiguration _config;
        private readonly SqlConnection _connection;
        public WeatherController(SqlConnection connection, IConfiguration config, IHttpClientFactory factory)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _config = config;
            _factory = factory;
        }
        public IActionResult Weather_Page()
        {
            return View();
        }

        public IActionResult Weather_details() { return View();  }
        [HttpGet]
        public async Task<IActionResult> GetWeatherDetails(string city)
        {
            string apiKey = _config["OpenWeather:ApiKey"];
            string url =
                $"https://api.openweathermap.org/data/2.5/find?q={city}&appid={apiKey}&units=metric";

            var client = _factory.CreateClient();
            var response = await client.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();
            var weather = JsonSerializer.Deserialize<WResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var data = weather.List.First();
            return Json(new
            {
                City = data.Name,
                TemperatureMin = data.Main.temp_min,
                TemperatureMax = data.Main.temp_max,
                Humidity = data.Main.humidity

            });
        }

        [HttpPost]
        public IActionResult SaveWeather(Main d)
        {
            using SqlCommand cmd = new SqlCommand("Save_Weathersp", _connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@minimum_temp", d.temp_min);
            cmd.Parameters.AddWithValue("@maximum_temp", d.temp_max);
            cmd.Parameters.AddWithValue("@humidity", d.humidity);
            cmd.Parameters.AddWithValue("@logid", d.logid);
            _connection.Open();
            int result = (int)cmd.ExecuteScalar();
            _connection.Close();

            return Json(new { success = true });
        }

        [HttpGet]
        public IActionResult  GetSavedDetails(int logid)
        {
            DataTable dt = new DataTable();

            using (SqlCommand cmd = new SqlCommand("Get_WeatherDetailsp", _connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@logid", logid);

                _connection.Open();
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
                var datals = new List<Dictionary<string, object>>();

                foreach (DataRow row in dt.Rows)
                {
                    var dict = new Dictionary<string, object>();
                    foreach (DataColumn col in dt.Columns)
                    {
                        dict[col.ColumnName] = row[col];
                    }
                    datals.Add(dict);
                }
                _connection.Close();
                return Json(new {data= datals, success=true});
            }
        }

        private DataRow GetOldWeatherData(int w_id)
        {
            DataTable dt = new DataTable();

            using (SqlCommand cmd = new SqlCommand(
                "SELECT * FROM weather_data_tbl WHERE w_id = @w_id", _connection))
            {
                cmd.Parameters.AddWithValue("@w_id", w_id);
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }
            return dt.Rows[0];
        }
        private void LogChange( int w_id, string fieldName,string oldValue,string newValue,int? userid)
        {
            if (oldValue == newValue) return;
            if (_connection.State != ConnectionState.Open)
                _connection.Open();


            using (SqlCommand cmd = new SqlCommand(
                @"INSERT INTO Weatherlog
          (w_id, ChangedBy, ChangeType, OldValue, NewValue)
          VALUES (@w_id, @ChangedBy, @ChangeType, @OldValue, @NewValue)", _connection))
            {
                cmd.Parameters.AddWithValue("@w_id", w_id);
                cmd.Parameters.AddWithValue("@ChangedBy", userid);
                cmd.Parameters.AddWithValue("@ChangeType", fieldName);
                cmd.Parameters.AddWithValue("@OldValue", oldValue);
                cmd.Parameters.AddWithValue("@NewValue", newValue);
                
                cmd.ExecuteNonQuery();
            }
        }
        [HttpPost]
        public IActionResult UpdateWeatherDetails([FromBody] List<WeatherUpdateModel> model)
        {
            int? userid = HttpContext.Session.GetInt32("logid");

            if (userid == null)
                return Json(new { success = false, message = "Session expired" });

            foreach (var item in model)

            {
                DataRow oldData = GetOldWeatherData(item.w_id);

                // 2️⃣ Compare & log changes
                LogChange(item.w_id, "Minimum_temp",
                    oldData["Minimum_temp"].ToString(),
                    item.Minimum_temp.ToString(), userid);

                LogChange(item.w_id, "Maximum_temp",
                    oldData["Maximum_temp"].ToString(),
                    item.Maximum_temp.ToString(),
                    userid);

                LogChange(item.w_id, "humidity",
                    oldData["humidity"].ToString(),
                    item.humidity.ToString(),
                    userid);

                LogChange(item.w_id, "date",
                    oldData["date"].ToString(),
                    item.date.ToString(),
                    userid);
                using (SqlCommand cmd = new SqlCommand("Update_WeatherDetails", _connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@w_id", item.w_id);
                    cmd.Parameters.AddWithValue("@minTemp", item.Minimum_temp);
                    cmd.Parameters.AddWithValue("@maxTemp", item.Maximum_temp);
                    cmd.Parameters.AddWithValue("@humidity", item.humidity);
                    cmd.Parameters.AddWithValue("@date", item.date);

                    cmd.ExecuteNonQuery();
                    _connection.Close();

                }
            }

            return Json(new { success = true });
        }
    }
}
