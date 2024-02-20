using FluentEmail.Core;

namespace TempMaiSe.Mailer;

public interface IMailInformationToMailMapper
{
    IFluentEmail Map(MailInformation mailInformation, IFluentEmail email);
}
