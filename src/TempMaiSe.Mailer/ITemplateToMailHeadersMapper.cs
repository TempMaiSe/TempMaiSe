using FluentEmail.Core;
using TempMaiSe.Models;

namespace TempMaiSe.Mailer;

public interface ITemplateToMailHeadersMapper
{
    IFluentEmail Map(TemplateData configuredTemplate, IFluentEmail email);
}
