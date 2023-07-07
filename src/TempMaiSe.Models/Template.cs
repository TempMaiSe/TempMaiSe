namespace TempMaiSe.Models;

public class Template
{
    public int Id { get; set; }

    public MailAddress? From { get; set; }

    public ICollection<MailAddress> To { get; init; } = new List<MailAddress>();

    public ICollection<MailAddress> Cc { get; init; } = new List<MailAddress>();

    public ICollection<MailAddress> Bcc { get; init; } = new List<MailAddress>();

    public ICollection<MailAddress> ReplyTo { get; init; } = new List<MailAddress>();

    public ICollection<Tag> Tags { get; init; } = new List<Tag>();

    public ICollection<Header> Headers { get; init; } = new List<Header>();

    public string SubjectTemplate { get; set; } = string.Empty;

    public string? HtmlBodyTemplate { get; set; }

    public string? PlainTextBodyTemplate { get; set; }

    public Priority Priority { get; set; } = Priority.None;

    public string JsonSchema { get; set; } = string.Empty;
}
