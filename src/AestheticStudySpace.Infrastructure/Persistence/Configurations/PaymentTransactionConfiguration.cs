using AestheticStudySpace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AestheticStudySpace.Infrastructure.Persistence.Configurations;

public class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
{
    public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
    {
        builder.ToTable("PaymentTransactions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Provider).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Purpose).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.TransactionCode).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Currency).HasMaxLength(10).IsRequired();
        builder.Property(x => x.ProviderPayloadJson).HasColumnType("text");
        builder.Property(x => x.MetadataJson).HasColumnType("text");
        builder.Property(x => x.IsFulfilled).HasDefaultValue(false);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.TransactionCode).IsUnique();
        builder.HasIndex(x => new { x.Provider, x.Status });
        builder.HasIndex(x => new { x.Purpose, x.Status });
    }
}

