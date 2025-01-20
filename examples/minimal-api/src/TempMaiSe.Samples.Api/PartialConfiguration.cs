using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TempMaiSe.Models;

namespace TempMaiSe.Samples.Api;

public class PartialConfiguration : IEntityTypeConfiguration<Partial>
{
    public void Configure(EntityTypeBuilder<Partial> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Property(p => p.Key).IsRequired();
        builder.HasIndex(p => p.Key).IsUnique();
        builder.OwnsMany(p => p.InlineAttachments);
    }
}
