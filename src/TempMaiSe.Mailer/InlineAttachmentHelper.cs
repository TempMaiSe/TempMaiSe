using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using FluentEmail.Core;
using TempMaiSe.Models;

namespace TempMaiSe.Mailer;

internal static class InlineAttachmentHelper
{
    public static IFluentEmail AttachInline(this IFluentEmail email, MailInformation mailInformation)
    {
        ArgumentNullException.ThrowIfNull(email);
        ArgumentNullException.ThrowIfNull(mailInformation);

        return email.AttachInline(mailInformation.InlineAttachments);
    }

    public static IFluentEmail AttachInline(this IFluentEmail email, TemplateData configuredTemplate)
    {
        ArgumentNullException.ThrowIfNull(email);
        ArgumentNullException.ThrowIfNull(configuredTemplate);

        return email.AttachInline(configuredTemplate.InlineAttachments);
    }

    private static IFluentEmail AttachInline(this IFluentEmail email, IEnumerable<Attachment>? attachments)
    {
        ArgumentNullException.ThrowIfNull(email);

        if (attachments is null)
        {
            return email;
        }

        foreach (Attachment attachment in attachments)
        {
            string attachmentId = GetAttachmentId(attachment);
            if (email.Data.Attachments.Any(a => a.IsInline && attachmentId.Equals(a.ContentId, StringComparison.OrdinalIgnoreCase)))
            {
                // Skip duplicate attachments
                continue;
            }

            FluentEmail.Core.Models.Attachment fluentAttachment = new() { ContentId = attachmentId, Filename = attachment.FileName, ContentType = attachment.MediaType, Data = new MemoryStream(attachment.Data), IsInline = true };
            email = email.Attach(fluentAttachment);
        }

        return email;
    }

    private static string GetAttachmentId(Attachment attachment)
    {
        ArgumentNullException.ThrowIfNull(attachment);

        Span<byte> hash = stackalloc byte[SHA256.HashSizeInBytes];
        bool success = SHA256.TryHashData(attachment.Data, hash, out int bytesWritten);
        if (!success)
        {
            return Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
        }

        StringBuilder builder = new(SHA256.HashSizeInBytes * 2);
        for (int i = 0; i < hash.Length; i++)
        {
            builder = builder.Append(hash[i].ToString("x2", CultureInfo.InvariantCulture));
        }

        return builder.ToString();
    }
}
