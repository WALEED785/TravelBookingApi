namespace TravelBookingApi.Models.DTOs
{
    public class HotelDTO
    {
        public int HotelId { get; set; }
        public string? Name { get; set; }
        public int DestinationId { get; set; }
        public decimal PricePerNight { get; set; }
        public decimal? Rating { get; set; }
        public string? Amenities { get; set; }
    }

    public class HotelCreateDTO
    {
        public string? Name { get; set; }
        public int DestinationId { get; set; }
        public decimal PricePerNight { get; set; }
        public decimal? Rating { get; set; }
        public string? Amenities { get; set; }
    }
}
