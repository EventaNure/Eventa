using Eventa.Application.Services.Orders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Eventa.Infrastructure.BackgroundServices
{
    public class DeleteExpireOrdersService : BackgroundService
    {
        private const int intervalInMinutes = 15;

        private readonly IServiceProvider _serviceProvider;

        public DeleteExpireOrdersService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var cartService = scope.ServiceProvider.GetRequiredService<IOrderService>();

            while (!stoppingToken.IsCancellationRequested)
            {
                await cartService.DeleteExpireOrdersAsync();
                await Task.Delay(TimeSpan.FromMinutes(intervalInMinutes), stoppingToken);
            }
        }
    }
}
