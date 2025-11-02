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

            builder.Entity<TicketInCart>()
                .HasOne<ApplicationUser>()
                .WithMany(u => u.TicketsInCart)
                .HasForeignKey(c => c.UserId);

            builder.Entity<Order>()
                .HasOne<ApplicationUser>()
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId);

            builder.Entity<TicketInCart>().HasKey(tic => new { tic.UserId, tic.SeatId});

            builder.Entity<EventTag>().HasKey(et => new {et.EventId, et.TagId});
        }

        public DbSet<Event> Events { get; set; }

        public DbSet<Tag> Tags { get; set; }

        public DbSet<EventTag> EventTags { get; set; }

        public DbSet<EventDateTime> EventDateTimes { get; set; }

        public DbSet<Place> Places { get; set; }

        public DbSet<Seat> Seats { get; set; }

        public DbSet<Row> Rows { get; set; }

        public DbSet<RowType> RowTypes { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<TicketInCart> TicketsInCart { get; set; }

        public DbSet<TicketInOrder> TicketsInOrder { get; set; }
    }
}
