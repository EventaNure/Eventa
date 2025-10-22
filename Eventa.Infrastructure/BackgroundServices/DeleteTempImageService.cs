using Eventa.Application.Services;
using Eventa.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Eventa.Infrastructure.BackgroundServices
{
    public class DeleteTempImageService : BackgroundService
    {
        private IServiceProvider _serviceProvider;

        public DeleteTempImageService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var fileService = scope.ServiceProvider.GetRequiredService<IFileService>();
            while (!stoppingToken.IsCancellationRequested)
            {
                fileService.ClearFolder("preview-events");

                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }
    }
}
