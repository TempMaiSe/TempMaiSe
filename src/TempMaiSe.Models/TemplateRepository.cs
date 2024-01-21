namespace TempMaiSe.Models;

public class TemplateRepository : ITemplateRepository
{
    private readonly MailingContext _context;

    public TemplateRepository(MailingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
    }

    /// <inheritdoc />
    public async Task<Template?> GetTemplateAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Templates.FindAsync(new object[] { id }, cancellationToken).ConfigureAwait(false);
    }
}