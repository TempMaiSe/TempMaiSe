using System.Text.Encodings.Web;
using Fluid.Ast;
using Fluid.Values;

namespace TempMaiSe.Mailer;

internal static class InlineImageTag
{
    public static async ValueTask<Completion> WriteToAsync(Expression value, TextWriter writer, TextEncoder encoder, Fluid.TemplateContext context)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(encoder);
        ArgumentNullException.ThrowIfNull(context);

        if (context.AmbientValues.TryGetValue("InlineAttachments", out object? inlineAttachmentsRaw) is not true
            || inlineAttachmentsRaw is not Dictionary<string, FluentEmail.Core.Models.Attachment> inlineAttachments)
        {
            return Completion.Normal;
        }

        FluidValue fluidValue = await value.EvaluateAsync(context).ConfigureAwait(false);
        if (inlineAttachments.TryGetValue(fluidValue.ToStringValue(), out FluentEmail.Core.Models.Attachment? attachment) is false)
        {
            return Completion.Normal;
        }

        await writer.WriteAsync($"cid:{attachment.ContentId}").ConfigureAwait(false);
        return Completion.Normal;
    }
}
