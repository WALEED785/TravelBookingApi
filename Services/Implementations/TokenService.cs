using TravelBookingApi.Services.Interfaces;

namespace TravelBookingApi.Services.Implementations
{
    public class SimpleTokenService : ITokenService
    {
        private readonly IConfiguration _config;

        public SimpleTokenService(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateToken(string username)
        {
            return Guid.NewGuid().ToString();
        }
    }
}