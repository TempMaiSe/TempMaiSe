﻿using FluentEmail.Core;
using TempMaiSe.Models;

namespace TempMaiSe.Mailer;

public class MailInformationToMailHeadersMapper : IMailInformationToMailHeadersMapper
{
    public IFluentEmail Map(MailInformation mailInformation, IFluentEmail email)
    {
        ArgumentNullException.ThrowIfNull(mailInformation);
        ArgumentNullException.ThrowIfNull(email);

        if (!string.IsNullOrWhiteSpace(mailInformation.From))
        {
            email = email.SetFrom(mailInformation.From);
        }

        foreach (string to in mailInformation.To)
        {
            email = email.To(to);
        }

        foreach (string cc in mailInformation.Cc)
        {
            email = email.CC(cc);
        }

        foreach (string bcc in mailInformation.Bcc)
        {
            email = email.BCC(bcc);
        }

        foreach (string replyTo in mailInformation.ReplyTo)
        {
            email = email.ReplyTo(replyTo);
        }

        foreach (string tag in mailInformation.Tags)
        {
            email = email.Tag(tag);
        }

        foreach (Header header in mailInformation.Headers)
        {
            email = email.Header(header.Name, header.Value);
        }

        switch (mailInformation.Priority)
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
