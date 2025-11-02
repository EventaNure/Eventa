using Eventa.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Eventa.Infrastructure.BackgroundServices
{
    public class DeleteTempImageService : BackgroundService
    {
        private const int intervalInDays = 1;

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
                try
                {
                    fileService.ClearFolder("preview-events");
                }
                catch (IOException ex)
                {
                    {

                    }
                }
                await Task.Delay(TimeSpan.FromDays(intervalInDays), stoppingToken);
            }
        }
    }
}
