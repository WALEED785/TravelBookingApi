namespace TravelBookingApi.Models.DTOs
{
    // Returned to the client
    public class BookingDTO
    {
        public int BookingId { get; set; }   // filled by DB
        public int UserId { get; set; }
        public int? HotelId { get; set; }
        public int? FlightId { get; set; }
        public string? HotelName { get; set; }
        public string? FlightName { get; set; }
        public decimal TotalPrice { get; set; }
        public string? Status { get; set; }
    }

    // Accepted from the client when creating
    public class BookingCreateDTO
    {
        public int UserId { get; set; }
        public int? HotelId { get; set; }
        public int? FlightId { get; set; }
        public decimal TotalPrice { get; set; }
        public string? Status { get; set; }
    }
}
