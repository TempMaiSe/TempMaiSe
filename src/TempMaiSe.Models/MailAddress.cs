using Microsoft.EntityFrameworkCore;

namespace TempMaiSe.Models;

[Keyless]
public record MailAddress(string Address, string? Name = null);
