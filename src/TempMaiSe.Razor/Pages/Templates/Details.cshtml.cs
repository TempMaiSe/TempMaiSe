using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TempMaiSe.Models;

namespace TempMaiSe.Razor.Pages.Templates;

public class DetailsModel(TemplateContext context) : PageModel
{
    public Template Template { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null || context.Templates == null)
        {
            return NotFound();
        }

        Template? template = await context.Templates.FirstOrDefaultAsync(m => m.Id == id).ConfigureAwait(false);
        if (template == null)
        {
            return NotFound();
        }
        else
        {
            Template = template;
        }

        return Page();
    }
}
