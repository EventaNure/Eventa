using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Eventa.Infrastructure.EntityTypeConfigurations
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.HasData(new ApplicationUser
            {
                Id = "1ed1ffc6-ab52-4bca-99e4-b9d8ee6b3816",
                UserName = "titarenkonik3@gmail.com",
                Email = "titarenkonik3@gmail.com",
                NormalizedEmail = "TITARENKONIK3@GMAIL.COM",
                NormalizedUserName = "TITARENKONIK3@GMAIL.COM",
                Name = "Mykyta",
                ConcurrencyStamp = "b1f6f6a5-6dcb-4f3c-8f2d-5e3e5c6e4f7a",
                SecurityStamp = "a1b2c3d4e5f6g7h8i9j0",
                PasswordHash = "AQAAAAIAAYagAAAAENDEN6HQ5Fam9YfJlSphPslWO0rt7rMVzNOVlhPK8b9Wp8wYHbbTyvRoc4xsY7P3Gw==",
                EmailConfirmed = true
            });
        }
    }
}