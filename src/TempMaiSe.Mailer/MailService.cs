using Fluid;
using Newtonsoft.Json.Schema;
using FluentEmail.Core;
using FluentEmail.Core.Models;

using System.Diagnostics;
using TempMaiSe.Models;
using TempMaiSe.OpenTelemetry;

using OneOf;
using OneOf.Types;

namespace TempMaiSe.Mailer;

/// <summary>
/// Represents a service for sending emails.
/// </summary>
public class MailService : IMailService
{
    private readonly IFluentEmail _mailer;

    private readonly IMailingInstrumentation _instrumentation;

    private readonly MailingContext _mailingContext;

    private readonly DataParser _dataParser;

    private readonly FluidParser _fluidParser;

    private readonly ITemplateToMailHeadersMapper _mailHeaderMapper;

    private readonly IMailInformationToMailHeadersMapper _mailInfoMapper;

    public MailService(
        IFluentEmail mailer,
        IMailingInstrumentation instrumentation,
        MailingContext mailingContext,
        DataParser dataParser,
        FluidParser fluidParser,
        ITemplateToMailHeadersMapper mailHeaderMapper,
        IMailInformationToMailHeadersMapper mailInfoMapper)
    {
        _mailer = mailer ?? throw new ArgumentNullException(nameof(mailer));
        _instrumentation = instrumentation ?? throw new ArgumentNullException(nameof(instrumentation));
        _mailingContext = mailingContext ?? throw new ArgumentNullException(nameof(mailingContext));
        _dataParser = dataParser ?? throw new ArgumentNullException(nameof(dataParser));
        _fluidParser = fluidParser ?? throw new ArgumentNullException(nameof(fluidParser));
        _mailHeaderMapper = mailHeaderMapper ?? throw new ArgumentNullException(nameof(mailHeaderMapper));
        _mailInfoMapper = mailInfoMapper ?? throw new ArgumentNullException(nameof(mailInfoMapper));
    }

    /// <inheritdoc/>
    public async Task<OneOf<SendResponse, NotFound, List<ValidationError>>> SendMailAsync(int id, Stream data, CancellationToken cancellationToken = default)
    {
        using Activity? activity = _instrumentation.ActivitySource.StartActivity("SendMail")!;
        activity?.AddTag("TemplateId", id);

        Template? template = await _mailingContext.Templates.FindAsync(new object[] { id }, cancellationToken).ConfigureAwait(false);
        if (template is null)
        {
            return new NotFound();
        }

        TemplateData templateData = template.Data;

        OneOf<MailInformation, List<ValidationError>> mailInformationOrErrors = await _dataParser.ParseAsync(templateData.JsonSchema, data, cancellationToken).ConfigureAwait(false);
        if (mailInformationOrErrors.TryPickT1(out List<ValidationError> errors, out MailInformation? mailInformation))
        {
            return errors;
        }

        IFluidTemplate fluidSubjectTemplate = _fluidParser.Parse(templateData.SubjectTemplate);
        TemplateContext templateContext = new(mailInformation.Data);
        string subject = await fluidSubjectTemplate.RenderAsync(templateContext).ConfigureAwait(false);

        IFluentEmail mail = _mailer.Subject(subject);
        mail = _mailHeaderMapper.Map(templateData, mail);
        mail = _mailInfoMapper.Map(mailInformation, mail);

        if (string.IsNullOrWhiteSpace(templateData.HtmlBodyTemplate))
        {
            IFluidTemplate plainTextFluidTemplate = _fluidParser.Parse(templateData.PlainTextBodyTemplate);
            string plainTextBody = await plainTextFluidTemplate.RenderAsync(templateContext).ConfigureAwait(false);
            mail = mail.Body(plainTextBody, false);
        }
        else
        {
            IFluidTemplate htmlFluidTemplate = _fluidParser.Parse(templateData.HtmlBodyTemplate);
            string htmlBody = await htmlFluidTemplate.RenderAsync(templateContext).ConfigureAwait(false);
            mail = mail.Body(htmlBody, true);
        }

        SendResponse resp = await mail.SendAsync(cancellationToken).ConfigureAwait(false);
        return resp;
    }
}
