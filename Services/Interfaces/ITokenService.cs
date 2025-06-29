namespace TravelBookingApi.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(string username);
    }
}