using Bookings.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bookings.Infrastructure.Data;

public class BookingsDbContext : DbContext
{
    public DbSet<Booking> Bookings => Set<Booking>();

    public BookingsDbContext(DbContextOptions<BookingsDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Booking>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.EventId).IsRequired();
            b.Property(x => x.UserId).IsRequired();
            b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            b.Property(x => x.CreatedAt).IsRequired();
        });
    }
}
