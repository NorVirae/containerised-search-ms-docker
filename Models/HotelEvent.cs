namespace SearchAPi.Models
{
    public class HotelEvent
    {
        public string userId { get; set; }
        public string id { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }
        public int Rating { get; set; }
        public string CityName { get; set; }
        public string FileName { get; set; }
        public DateTime CreationDateTIme { get; set; }
    }
}
