using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sprint_1_WebAPI.Models;

namespace Sprint_1_WebAPI.DataAccess.Configurations;

public sealed class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("events");

        builder.HasKey(eventItem => eventItem.Id);
        builder.Property(eventItem => eventItem.Id).ValueGeneratedNever();
        builder.Property(eventItem => eventItem.Title).IsRequired().HasMaxLength(200);
        builder.Property(eventItem => eventItem.Description).HasMaxLength(2000);
        builder.Property(eventItem => eventItem.StartAt).IsRequired();
        builder.Property(eventItem => eventItem.EndAt).IsRequired();
        builder.Property(eventItem => eventItem.TotalSeats).IsRequired();
        builder.Property(eventItem => eventItem.AvailableSeats).IsRequired();

        builder.HasMany(eventItem => eventItem.Bookings)
            .WithOne(booking => booking.Event)
            .HasForeignKey(booking => booking.EventId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
