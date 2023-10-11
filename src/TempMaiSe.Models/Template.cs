namespace TempMaiSe.Models;

public class Template
{
    public int Id { get; set; }

    public TemplateData Data { get; init; } = null!;
}
