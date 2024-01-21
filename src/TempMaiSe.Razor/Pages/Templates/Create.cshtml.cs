using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using TempMaiSe.Models;

namespace TempMaiSe.Razor.Pages.Templates;

public class CreateModel(TemplateContext context) : PageModel
{
    public IActionResult OnGet()
    {
        return Page();
    }

    [BindProperty]
    public Template Template { get; set; } = default!;

    // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid || context.Templates == null || Template == null)
        {
            return Page();
        }

        context.Templates.Add(Template);
        await context.SaveChangesAsync().ConfigureAwait(false);

        return RedirectToPage("./Index");
    }
}
