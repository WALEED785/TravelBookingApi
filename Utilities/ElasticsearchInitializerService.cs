using TravelBookingApi.Services.AISearch;

namespace TravelBookingApi.Utilities
{
    public class ElasticsearchInitializerService : IHostedService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<ElasticsearchInitializerService> _logger;

        public ElasticsearchInitializerService(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<ElasticsearchInitializerService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting Elasticsearch initialization...");

            try
            {
                // Create a new scope to access scoped services
                using var scope = _serviceScopeFactory.CreateScope();
                var elasticsearchService = scope.ServiceProvider.GetRequiredService<IElasticsearchService>();

                // Initialize Elasticsearch indices
                var result = await elasticsearchService.CreateIndicesAsync();

                if (result)
                {
                    _logger.LogInformation("Elasticsearch indices created successfully");
                }
                else
                {
                    _logger.LogError("Failed to create Elasticsearch indices");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Elasticsearch initialization");
                // Depending on your requirements, you might want to:
                // - Rethrow the exception to prevent application startup
                // - Log and continue (current behavior)
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Elasticsearch initializer service stopped");
            return Task.CompletedTask;
        }
    }
}