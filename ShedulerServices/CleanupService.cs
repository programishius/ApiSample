using Microsoft.Extensions.Options;
using ApiSample.Services;
using System.ComponentModel.Design;

namespace ApiSample.ShedulerServices
{
    public class CleanupService : BackgroundService
    {
        private readonly IDataServices _dataService;
        private readonly int _cleanupInterval;

        public CleanupService(IDataServices dataService, IOptions<Settings> settings)
        {
            _dataService = dataService;
            _cleanupInterval = settings.Value.CleanupIntervalInSeconds;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _dataService.CleanupData();
                await Task.Delay(TimeSpan.FromSeconds(_cleanupInterval), stoppingToken);
            }
        }
    }
}
