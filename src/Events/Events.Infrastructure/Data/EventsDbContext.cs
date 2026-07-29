using Events.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Events.Infrastructure.Data;

public class EventsDbContext : DbContext
{
    public DbSet<Event> Events => Set<Event>();

    public EventsDbContext(DbContextOptions<EventsDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Event>(b =>
        {
            b.HasKey(e => e.Id);
            b.Property(e => e.Title).IsRequired().HasMaxLength(200);
            b.Property(e => e.StartAt).IsRequired();
            b.Property(e => e.EndAt).IsRequired();
        });
    }
}
