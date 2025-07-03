namespace TravelBookingApi.Models.DTOs
{
    public class DestinationDTO
    {
        public int DestinationId { get; set; }
        public string? Name { get; set; }
        public string? Country { get; set; }
        public string? Description { get; set; }
    }

    public class DestinationCreateDTO
    {
        public string? Name { get; set; }
        public string? Country { get; set; }
        public string? Description { get; set; }
    }
}
