using FluentEmail.Core;
using TempMaiSe.Models;

namespace TempMaiSe.Mailer;

public interface ITemplateToMailMapper
{
    IFluentEmail Map(TemplateData configuredTemplate, IFluentEmail email);
}
