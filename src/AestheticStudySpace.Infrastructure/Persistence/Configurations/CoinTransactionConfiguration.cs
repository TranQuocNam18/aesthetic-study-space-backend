using AestheticStudySpace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AestheticStudySpace.Infrastructure.Persistence.Configurations;

public class CoinTransactionConfiguration : IEntityTypeConfiguration<CoinTransaction>
{
    public void Configure(EntityTypeBuilder<CoinTransaction> builder)
    {
        builder.ToTable("CoinTransactions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Reason).HasMaxLength(200).IsRequired();

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.RelatedMission)
            .WithMany()
            .HasForeignKey(x => x.RelatedMissionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.RelatedPurchase)
            .WithMany()
            .HasForeignKey(x => x.RelatedPurchaseId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => new { x.UserId, x.CreatedAt });
    }
}

