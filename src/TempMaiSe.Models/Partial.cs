using System.Collections.ObjectModel;

namespace TempMaiSe.Models;

public class Partial
{
    public required int Id { get; set; }

    public required string Key { get; set; }

    public string? HtmlTemplate { get; set; }

    public string? PlainTextTemplate { get; set; }

    public Collection<Attachment> InlineAttachments { get; } = [];
}
