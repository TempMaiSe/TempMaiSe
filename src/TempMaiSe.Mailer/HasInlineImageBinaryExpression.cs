using Fluid.Ast;
using Fluid.Values;

namespace TempMaiSe.Mailer;

internal sealed class HasInlineImageBinaryExpression(Expression left, Expression right) : BinaryExpression(left, right)
{
    public override async ValueTask<FluidValue> EvaluateAsync(Fluid.TemplateContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.AmbientValues.TryGetValue(nameof(InlineAttachmentCollection), out InlineAttachmentCollection? inlineAttachments) is not true)
        {
            return BooleanValue.False;
        }

        FluidValue rightValue = await Right.EvaluateAsync(context).ConfigureAwait(false);
        if (rightValue is not StringValue imageFileName)
        {
            return BooleanValue.False;
        }

        if (inlineAttachments!.TryGetAttachmentByFileName(imageFileName.ToStringValue(), out InlineAttachmentWithId? _) is false)
        {
            return BooleanValue.False;
        }

        return BooleanValue.True;
    }
}