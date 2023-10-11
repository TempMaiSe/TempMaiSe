using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TempMaiSe.Models;

public class TemplateConfiguration : IEntityTypeConfiguration<Template>
{
    public void Configure(EntityTypeBuilder<Template> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.OwnsOne(
            template => template.Data,
            ownedNavigationBuilder =>
        {
            ownedNavigationBuilder.ToJson();
            ownedNavigationBuilder.OwnsOne(template => template.From);
            ownedNavigationBuilder.OwnsMany(template => template.To);
            ownedNavigationBuilder.OwnsMany(template => template.Cc);
            ownedNavigationBuilder.OwnsMany(template => template.Bcc);
            ownedNavigationBuilder.OwnsMany(template => template.ReplyTo);
            ownedNavigationBuilder.OwnsMany(template => template.Tags);
            ownedNavigationBuilder.OwnsMany(template => template.Headers);
        });
    }
}
