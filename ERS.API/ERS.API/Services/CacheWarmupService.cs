using ERS.Core.Interfaces;

namespace ERS.API.Services
{
    public class CacheWarmupService : BackgroundService
    {
        private readonly IServiceProvider service_provider;
        private readonly ILogger<CacheWarmupService> logger;

        public CacheWarmupService(IServiceProvider service_provider, ILogger<CacheWarmupService> logger)
        {
            this.service_provider = service_provider;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Background Cache Warm-up Service is starting.");

            try
            {
                await Task.Delay(2000, stoppingToken);

                using var scope = service_provider.CreateScope();
                var rec_service = scope.ServiceProvider.GetRequiredService<IRecommendService>();

                logger.LogInformation("Starting to warm up CRS cache and TF-IDF matrix...");

                await rec_service.WarmUpAsync();

                logger.LogInformation("CRS Cache successfully warmed up!.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during CRS cache warm-up.");
            }
        }
    }
}
