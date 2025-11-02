using Eventa.Application.Services.TicketsInCart;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Eventa.Infrastructure.BackgroundServices
{
    public class DeleteExpireTicketsInCartService : BackgroundService
    {
        private const int intervalInMinutes = 15;

        private readonly IServiceProvider _serviceProvider;

        public DeleteExpireTicketsInCartService(IServiceProvider serviceProvider) {
            _serviceProvider = serviceProvider;
        }
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var cartService = scope.ServiceProvider.GetRequiredService<ITicketInCartService>();

            while (!stoppingToken.IsCancellationRequested)
            {
                await cartService.DeleteExpiredTicketsAsync();
                await Task.Delay(TimeSpan.FromMinutes(intervalInMinutes), stoppingToken);
            }
        }
    }
}
