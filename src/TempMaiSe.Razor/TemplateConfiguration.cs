﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using TempMaiSe.Models;

namespace TempMaiSe.Razor;

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
                ownedNavigationBuilder.OwnsOne(data => data.From);
                ownedNavigationBuilder.OwnsMany(data => data.To);
                ownedNavigationBuilder.OwnsMany(data => data.Cc);
                ownedNavigationBuilder.OwnsMany(data => data.Bcc);
                ownedNavigationBuilder.OwnsMany(data => data.ReplyTo);
                ownedNavigationBuilder.OwnsMany(data => data.Tags);
                ownedNavigationBuilder.OwnsMany(data => data.Headers);
            });
    }
}
