using Eventa.Application.Repositories;
using Eventa.Application.Services;
using Eventa.Application.Services.Events;
using Eventa.Application.Services.Orders;
using Eventa.Application.Services.Places;
using Eventa.Application.Services.Sections;
using Eventa.Application.Services.Tags;
using Eventa.Application.Services.TicketsInCart;
using Eventa.Infrastructure.Repositories;
using Eventa.Infrastructure.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Eventa.Infrastructure
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IEventService, EventService>();
            services.AddScoped<ITagService, TagService>();
            services.AddScoped<IPlaceService, PlaceService>();
            services.AddScoped<ITicketInCartService, TicketInCartService>();
            services.AddScoped<ISeatService, SeatService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IPaymentService, StripePaymentService>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IEmailSender, SendGridEmailSender>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddAutoMapper((e) => { }, AppDomain.CurrentDomain.GetAssemblies());
            return services;
        }
    }
}
