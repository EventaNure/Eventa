using System.Reflection;
using Eventa.Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Eventa.Infrastructure
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            builder.Entity<EventTag>()
                .HasOne(et => et.Event)
                .WithMany(e => e.EventTags)
                .HasForeignKey(et => et.EventId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<EventTag>()
                .HasOne(et => et.Tag)
                .WithMany(t => t.EventTags)
                .HasForeignKey(et => et.TagId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<EventDateTime>()
                .HasOne(edt => edt.Event)
                .WithMany(e => e.EventDateTimes)
                .HasForeignKey(edt => edt.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<EventTag>().HasKey(et => new {et.EventId, et.TagId});
        }

        public DbSet<Event> Events { get; set; }

        public DbSet<Tag> Tags { get; set; }

        public DbSet<EventTag> EventTags { get; set; }

        public DbSet<EventDateTime> EventDateTimes { get; set; }
    }
}
