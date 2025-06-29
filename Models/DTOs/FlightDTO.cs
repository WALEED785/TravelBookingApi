namespace TravelBookingApi.Models.DTOs
{
    public class FlightDTO
    {
        public int FlightId { get; set; }
        public string? Airline { get; set; }
        public int DepartureDestinationId { get; set; }
        public int ArrivalDestinationId { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public decimal Price { get; set; }
    }
}