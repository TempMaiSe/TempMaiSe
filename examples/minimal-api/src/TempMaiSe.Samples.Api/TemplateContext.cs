using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using TempMaiSe.Models;

namespace TempMaiSe.Samples.Api;

public class TemplateContext(DbContextOptions<TemplateContext> options) : IdentityDbContext(options)
{
    public virtual DbSet<Template> Templates { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.ApplyConfiguration(new TemplateConfiguration());
        base.OnModelCreating(builder);
    }
}
