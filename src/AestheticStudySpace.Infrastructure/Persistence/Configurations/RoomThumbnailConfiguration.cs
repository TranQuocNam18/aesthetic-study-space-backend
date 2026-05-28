using AestheticStudySpace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AestheticStudySpace.Infrastructure.Persistence.Configurations;

public class RoomThumbnailConfiguration : IEntityTypeConfiguration<RoomThumbnail>
{
    public void Configure(EntityTypeBuilder<RoomThumbnail> builder)
    {
        builder.ToTable("RoomThumbnails");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Url).HasMaxLength(2048).IsRequired();
        builder.Property(x => x.PublicId).HasMaxLength(200);

        builder.HasOne(x => x.RoomLayout)
            .WithMany()
            .HasForeignKey(x => x.RoomLayoutId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.RoomLayoutId).IsUnique();
    }
}

