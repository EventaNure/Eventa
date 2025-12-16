using System.Text;
using Eventa.Infrastructure.BackgroundServices;
using Eventa.Infrastructure.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Stripe;

namespace Eventa.Infrastructure
{
    public static class InfrastructureServiceExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfigurationManager configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            services.AddDbContextFactory<ApplicationDbContext>(options =>
            {
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            });

            services.AddDefaultIdentity<ApplicationUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 8;
            })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            var token = configuration.GetSection("Jwt:Key").Value ?? throw new InvalidOperationException("Jwt key not fond");
            var issuer = configuration.GetSection("Jwt:Issuer").Value ?? throw new InvalidOperationException("Issuer not found");
            var audience = configuration.GetSection("Jwt:Audience").Value ?? throw new InvalidOperationException("Audience not found");

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(opt =>
                {
                    opt.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(token)),
                        ValidIssuer = issuer,
                        ValidAudience = audience
                    };
                });

            services.Configure<SmtpEmailOptions>(configuration.GetSection("SmtpEmailOptions"));
            services.Configure<SendGridEmailOptions>(configuration.GetSection("SendGrid"));
            services.Configure<JwtTokenOptions>(configuration.GetSection("Jwt"));
            services.Configure<PaymentOptions>(configuration.GetSection("Stripe"));
            services.Configure<GoogleOptions>(configuration.GetSection("OAuth"));

            services.AddHostedService<DeleteTempImageService>();
            services.AddHostedService<DeleteExpireTicketsInCartService>();
            services.AddHostedService<DeleteExpireOrdersService>();

            services.AddHttpClient();

            var stripeSettings = configuration.GetSection("Stripe");
            StripeConfiguration.ApiKey = stripeSettings["SecretKey"];

            return services;
        }
    }
}
