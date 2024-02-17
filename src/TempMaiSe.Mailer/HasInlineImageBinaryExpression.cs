using Fluid.Ast;
using Fluid.Values;

namespace TempMaiSe.Mailer;

internal sealed class HasInlineImageBinaryExpression(Expression left, Expression right) : BinaryExpression(left, right)
{
    public override async ValueTask<FluidValue> EvaluateAsync(Fluid.TemplateContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.AmbientValues.TryGetValue("InlineAttachments", out object? inlineAttachmentsRaw) is not true
            || inlineAttachmentsRaw is not Dictionary<string, FluentEmail.Core.Models.Attachment> inlineAttachments)
        {
            return BooleanValue.False;
        }

        FluidValue rightValue = await Right.EvaluateAsync(context).ConfigureAwait(false);
        if (rightValue is not StringValue imageFileName)
        {
            return BooleanValue.False;
        }

        if (inlineAttachments.TryGetValue(imageFileName.ToStringValue(), out FluentEmail.Core.Models.Attachment? attachment) is false)
        {
            return BooleanValue.False;
        }

        return BooleanValue.True;
    }
}