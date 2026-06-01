using AestheticStudySpace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AestheticStudySpace.Infrastructure.Persistence.Configurations;

public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.ToTable("Rooms");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Name).HasMaxLength(120).IsRequired();
        builder.Property(r => r.Description).HasMaxLength(500);
        builder.Property(r => r.ThumbnailUrl).HasMaxLength(2048);
        builder.Property(r => r.BackgroundUrl).HasMaxLength(2048);

        // Optional FK: null means admin/global room, non-null means user-created room
        builder.Property(r => r.UserId).IsRequired(false);
        builder.HasOne(r => r.Owner)
               .WithMany()
               .HasForeignKey(r => r.UserId)
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired(false);
    }
}
