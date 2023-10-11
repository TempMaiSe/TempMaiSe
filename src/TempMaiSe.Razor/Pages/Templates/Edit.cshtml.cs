using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

using TempMaiSe.Models;

namespace TempMaiSe.Razor.Pages.Templates;

public class EditModel(MailingContext context) : PageModel
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

        Template = template;
        return Page();
    }

    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see https://aka.ms/RazorPagesCRUD.
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        context.Attach(Template).State = EntityState.Modified;

        try
        {
            await context.SaveChangesAsync().ConfigureAwait(false);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!TemplateExists(Template.Id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return RedirectToPage("./Index");
    }

    private bool TemplateExists(int id)
    {
        return (context.Templates?.Any(e => e.Id == id)).GetValueOrDefault();
    }
}
