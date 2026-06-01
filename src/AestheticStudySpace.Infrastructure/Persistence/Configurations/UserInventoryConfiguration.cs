using AestheticStudySpace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AestheticStudySpace.Infrastructure.Persistence.Configurations;

public class UserInventoryConfiguration : IEntityTypeConfiguration<UserInventory>
{
    public void Configure(EntityTypeBuilder<UserInventory> builder)
    {
        builder.ToTable("UserInventories");
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.StoreItem)
            .WithMany()
            .HasForeignKey(x => x.StoreItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.UserId, x.StoreItemId }).IsUnique();
        builder.HasIndex(x => x.UserId);
    }
}

