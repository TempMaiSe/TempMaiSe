using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TempMaiSe.Models;

namespace TempMaiSe.Razor.Pages.Templates;

public class DeleteModel(TemplateContext context) : PageModel
{

    [BindProperty]
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

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        if (id == null || context.Templates == null)
        {
            return NotFound();
        }

        Template? template = await context.Templates.FindAsync(id).ConfigureAwait(false);

        if (template != null)
        {
            Template = template;
            context.Templates.Remove(Template);
            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        return RedirectToPage("./Index");
    }
}
