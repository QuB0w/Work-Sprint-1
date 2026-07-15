using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sprint_1_WebAPI.Models;

namespace Sprint_1_WebAPI.DataAccess.Configurations;

public sealed class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("bookings");

        builder.HasKey(booking => booking.Id);
        builder.Property(booking => booking.Id).ValueGeneratedNever();
        builder.Property(booking => booking.EventId).IsRequired();
        builder.Property(booking => booking.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(booking => booking.CreatedAt).IsRequired();

        builder.HasOne(booking => booking.Event)
            .WithMany(eventItem => eventItem.Bookings)
            .HasForeignKey(booking => booking.EventId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
