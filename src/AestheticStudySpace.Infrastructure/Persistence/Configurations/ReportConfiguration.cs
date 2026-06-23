using AestheticStudySpace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AestheticStudySpace.Infrastructure.Persistence.Configurations;

public class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.ToTable("Reports");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Title).HasMaxLength(256).IsRequired();
        builder.Property(r => r.Content).HasMaxLength(4000).IsRequired();
        builder.Property(r => r.Type).HasMaxLength(50).HasDefaultValue("Feedback").IsRequired();
        builder.Property(r => r.AttachmentUrl).HasMaxLength(2048);
        builder.Property(r => r.Status).HasMaxLength(50).HasDefaultValue("Pending").IsRequired();

        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.UserId);
        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.CreatedAt);
    }
}
