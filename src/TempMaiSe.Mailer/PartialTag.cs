using Fluid;
using Fluid.Ast;
using Fluid.Values;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Encodings.Web;
using TempMaiSe.Models;

namespace TempMaiSe.Mailer;

public static class PartialTag
{
    public static async ValueTask<Completion> WriteToAsync(
        Expression value,
        TextWriter writer,
        TextEncoder encoder,
        TemplateContext context)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(encoder);
        ArgumentNullException.ThrowIfNull(context);

        FluidValue fluidValue = await value.EvaluateAsync(context).ConfigureAwait(false);
        string? fluidStringValue = fluidValue.ToStringValue();
        if (string.IsNullOrWhiteSpace(fluidStringValue))
        {
            return Completion.Normal;
        }

        if (!context.AmbientValues.TryGetValue(Constants.ServiceProvider, out IServiceProvider? serviceProvider))
        {
            return Completion.Normal;
        }

        if (await serviceProvider.GetRequiredService<IPartialRepository>().GetPartialAsync(fluidStringValue) is not Partial partial)
        {
            throw new ArgumentException($"Partial '{value}' not found.");
        }

        FluidParser fluidParser = serviceProvider.GetRequiredService<FluidParser>();
        string partialTemplateString = context.GetValue(Constants.IsHtml) is BooleanValue isHtml && isHtml.ToBooleanValue()
            ? partial.HtmlTemplate
            : partial.PlainTextTemplate;
        IFluidTemplate partialTemplate = fluidParser.Parse(partialTemplateString);
        await partialTemplate.RenderAsync(writer, encoder, context).ConfigureAwait(false);

        return Completion.Normal;
    }
}
