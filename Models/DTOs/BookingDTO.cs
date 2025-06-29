namespace TravelBookingApi.Models.DTOs
{
    public class BookingDTO
    {
        public int BookingId { get; set; }
        public int UserId { get; set; }
        public int? HotelId { get; set; }
        public int? FlightId { get; set; }
        public decimal TotalPrice { get; set; }
        public string? Status { get; set; }
    }
}