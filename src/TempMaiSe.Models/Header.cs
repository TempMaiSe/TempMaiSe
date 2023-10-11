using Microsoft.EntityFrameworkCore;

namespace TempMaiSe.Models;

[Keyless]
public record Header(string Name, string Value);
