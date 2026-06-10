using AestheticStudySpace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AestheticStudySpace.Infrastructure.Persistence.Configurations;

public class StoreItemConfiguration : IEntityTypeConfiguration<StoreItem>
{
    public void Configure(EntityTypeBuilder<StoreItem> builder)
    {
        builder.ToTable("StoreItems");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Category).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.ThemeSource).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Name).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.AssetUrl).HasMaxLength(2048).IsRequired();
        builder.Property(x => x.PreviewUrl).HasMaxLength(2048);
        builder.Property(x => x.RejectionNote).HasMaxLength(1000);

        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30)
            .HasDefaultValue(Domain.Enums.StoreItemStatus.AdminCreated);

        // Creator relationship — optional FK (null = admin-created)
        builder.HasOne(x => x.Creator)
            .WithMany()
            .HasForeignKey(x => x.CreatorId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasIndex(x => x.Category);
        builder.HasIndex(x => x.ThemeSource);
        builder.HasIndex(x => x.IsActive);
        builder.HasIndex(x => x.IsPremium);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.CreatorId);
    }
}
