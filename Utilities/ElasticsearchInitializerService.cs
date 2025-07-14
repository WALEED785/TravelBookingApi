using TravelBookingApi.Services.AISearch;

namespace TravelBookingApi.Utilities;

public class ElasticsearchInitializerService : IHostedService
{
    private readonly IElasticsearchService _elasticsearchService;

    public ElasticsearchInitializerService(IElasticsearchService elasticsearchService)
    {
        _elasticsearchService = elasticsearchService;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _elasticsearchService.CreateIndicesAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}