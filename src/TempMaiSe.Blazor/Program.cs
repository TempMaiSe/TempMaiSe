using TempMaiSe.Blazor;
using Fluid;
using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using Blazorise.RichTextEdit;

using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using System.Text;
using FluentEmail.Core;
using System.Diagnostics;
using TempMaiSe.Models;
using TempMaiSe.Mailer;

Activity.DefaultIdFormat = ActivityIdFormat.W3C;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
ConfigurationManager config = builder.Configuration;

builder.Services.AddMailingContext(config);

builder.AddOpenTelemetry();

// Add services to the container.
builder.Services.AddHealthChecks()
    .AddDbContextCheck<MailingContext>();

builder.Services.AddProblemDetails();
builder.Services.AddSingleton<FluidParser>();
builder.Services.AddRazorComponents()
    .AddServerComponents();

builder.Services
    .AddBlazorise(options =>
    {
        options.Immediate = true;
    })
    .AddBootstrap5Providers()
    .AddFontAwesomeIcons()
    .AddBlazoriseRichTextEdit();

builder.Services.AddFluentEmail(config);

builder.Services.AddSingleton<ITemplateToMailHeadersMapper, TemplateToMailHeadersMapper>();
builder.Services.AddSingleton<IMailInformationToMailHeadersMapper, MailInformationToMailHeadersMapper>();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}

app.MapHealthChecks("/healthz");

app.UseHttpsRedirection();

app.UseStaticFiles();

app.MapRazorComponents<App>();

app.MapPost("/send/{id}", async (int id, Stream data, IFluentEmail mailer, Instrumentation instrumentation, MailingContext mailingContext, FluidParser fluidParser, ITemplateToMailHeadersMapper mailHeaderMapper, IMailInformationToMailHeadersMapper mailInfoMapper, CancellationToken cancellationToken) =>
{
    using Activity? activity = instrumentation.ActivitySource.StartActivity("SendMail")!;
    activity?.AddTag("TemplateId", id);

    Template? template = await mailingContext.Templates.FindAsync(new object[] { id }, cancellationToken).ConfigureAwait(false);
    if (template is null)
    {
        return Results.NotFound();
    }

    using var sr = new StreamReader(data, Encoding.UTF8);
    string str = await sr.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
    JsonTextReader reader = new(new StringReader(str));

    JSchemaValidatingReader validatingReader = new(reader)
    {
        Schema = JSchema.Parse(template.JsonSchema)
    };

    List<Newtonsoft.Json.Schema.ValidationError> errors = [];
    validatingReader.ValidationEventHandler += (o, a) => errors.Add(a.ValidationError);

    JsonSerializer serializer = new();
    MailInformation mailInformation = serializer.Deserialize<MailInformation>(validatingReader)!;
    if (errors.Count > 0)
    {
        return Results.BadRequest(errors);
    }

    IFluidTemplate fluidSubjectTemplate = fluidParser.Parse(template.SubjectTemplate);
    TemplateContext templateContext = new(mailInformation.Data);
    string subject = await fluidSubjectTemplate.RenderAsync(templateContext).ConfigureAwait(false);

    IFluentEmail mail = mailer.Subject(subject);
    mail = mailHeaderMapper.Map(template, mail);
    mail = mailInfoMapper.Map(mailInformation, mail);

    if (string.IsNullOrWhiteSpace(template.HtmlBodyTemplate))
    {
        IFluidTemplate plainTextFluidTemplate = fluidParser.Parse(template.PlainTextBodyTemplate);
        string plainTextBody = await plainTextFluidTemplate.RenderAsync(templateContext).ConfigureAwait(false);
        mail = mail.Body(plainTextBody, false);
    }
    else
    {
        IFluidTemplate htmlFluidTemplate = fluidParser.Parse(template.HtmlBodyTemplate);
        string htmlBody = await htmlFluidTemplate.RenderAsync(templateContext, System.Text.Encodings.Web.HtmlEncoder.Default).ConfigureAwait(false);
        mail = mail.Body(htmlBody, true);

        if (!string.IsNullOrWhiteSpace(template.PlainTextBodyTemplate))
        {
            IFluidTemplate plainTextFluidTemplate = fluidParser.Parse(template.PlainTextBodyTemplate);
            string plainTextBody = await plainTextFluidTemplate.RenderAsync(templateContext).ConfigureAwait(false);
            mail = mail.PlaintextAlternativeBody(plainTextBody);
        }
    }

    FluentEmail.Core.Models.SendResponse resp = await mail.SendAsync(cancellationToken).ConfigureAwait(false);

    instrumentation.MailsSent.Add(1);

    return Results.Ok(resp);
});

app.Run();
