using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TempMaiSe.Models;

public class MailingContext(DbContextOptions<MailingContext> options) : IdentityDbContext(options)
{
    public virtual DbSet<Template> Templates { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.ApplyConfiguration(new TemplateConfiguration());
        base.OnModelCreating(builder);
    }
}
