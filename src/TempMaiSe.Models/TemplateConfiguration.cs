using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TempMaiSe.Models;

public class TemplateConfiguration : IEntityTypeConfiguration<Template>
{
    public void Configure(EntityTypeBuilder<Template> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.OwnsOne(template => template.From);
        builder.OwnsMany(template => template.To);
        builder.OwnsMany(template => template.Cc);
        builder.OwnsMany(template => template.Bcc);
        builder.OwnsMany(template => template.ReplyTo);
        builder.OwnsMany(template => template.Tags);
        builder.OwnsMany(template => template.Headers);
    }
}
