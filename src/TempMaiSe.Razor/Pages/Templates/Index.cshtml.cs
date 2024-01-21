using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

using TempMaiSe.Models;

namespace TempMaiSe.Razor.Pages.Templates;

public class IndexModel(TemplateContext context) : PageModel
{
    public IList<Template> Templates { get; private set; } = default!;

    public async Task OnGetAsync()
    {
        if (context.Templates != null)
        {
            Templates = await context.Templates.ToListAsync().ConfigureAwait(false);
        }
    }
}
