using Microsoft.EntityFrameworkCore;

namespace TempMaiSe.Models;

public class MailingContext(DbContextOptions<MailingContext> options) : DbContext(options)
{
    public DbSet<Template> Templates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.ApplyConfiguration(new TemplateConfiguration());
    }
}
