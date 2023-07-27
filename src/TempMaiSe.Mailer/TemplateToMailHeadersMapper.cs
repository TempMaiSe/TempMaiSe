using FluentEmail.Core;
using TempMaiSe.Models;

namespace TempMaiSe.Mailer;

public class TemplateToMailHeadersMapper : ITemplateToMailHeadersMapper
{
    public IFluentEmail Map(Template configuredTemplate, IFluentEmail email)
    {
        ArgumentNullException.ThrowIfNull(configuredTemplate);
        ArgumentNullException.ThrowIfNull(email);

        if (configuredTemplate.From is MailAddress from)
        {
            email = email.SetFrom(from.Address, from.Name);
        }

        foreach (MailAddress to in configuredTemplate.To)
        {
            email = email.To(to.Address, to.Name);
        }

        foreach (MailAddress cc in configuredTemplate.Cc)
        {
            email = email.CC(cc.Address, cc.Name);
        }

        foreach (MailAddress bcc in configuredTemplate.Bcc)
        {
            email = email.BCC(bcc.Address, bcc.Name);
        }

        foreach (MailAddress replyTo in configuredTemplate.ReplyTo)
        {
            email = email.ReplyTo(replyTo.Address, replyTo.Name);
        }

        foreach (Tag tag in configuredTemplate.Tags)
        {
            email = email.Tag(tag.Name);
        }

        foreach (Header header in configuredTemplate.Headers)
        {
            email = email.Header(header.Name, header.Value);
        }

        switch (configuredTemplate.Priority)
        {
            case Priority.High:
                email = email.HighPriority();
                break;
            case Priority.Low:
                email = email.LowPriority();
                break;
        }

        return email;
    }
}
