using System.Text.Json.Serialization;

namespace WeatherApp.Models
{

    public class WResponse
    {
        public List<Weatherls> List { get; set; }
    }
    public class Weatherls
    {
        public Main Main { get; set; }
        public string Name { get; set; }
    }
    public class Main
    {
        public float temp_min { get; set; }
        public float temp_max { get; set; }
        public int humidity { get; set; }
        public string logid { get; set; }
    }
}
public class WeatherUpdateModel
{
    public int w_id { get; set; }
    public float Minimum_temp { get; set; }
    public float Maximum_temp { get; set; }
    public int humidity { get; set; }
    public DateTime date { get; set; }
}