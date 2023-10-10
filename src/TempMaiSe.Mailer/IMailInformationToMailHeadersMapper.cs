using FluentEmail.Core;

namespace TempMaiSe.Mailer;

public interface IMailInformationToMailHeadersMapper
{
    IFluentEmail Map(MailInformation mailInformation, IFluentEmail email);
}
