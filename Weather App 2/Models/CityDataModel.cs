namespace Weather_App_2.Models
{
    public class CityDataModel
    {
        public string CityName { get; set; }
        public string Country { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public override string ToString()
        {
            return CityName;
        }
    }
}
