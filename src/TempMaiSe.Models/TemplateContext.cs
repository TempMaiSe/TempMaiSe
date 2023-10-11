using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TempMaiSe.Models;

public class MailingContext(DbContextOptions<MailingContext> options) : IdentityDbContext(options)
{
    public DbSet<Template> Templates { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        base.OnModelCreating(builder);
        builder.ApplyConfiguration(new TemplateConfiguration());
    }
}
