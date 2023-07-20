using TempMaiSe.Blazor;
using Fluid;
using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using Blazorise.RichTextEdit;
using System.Dynamic;

using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using System.Text;
using FluentEmail.Core;
using System.Diagnostics;
using TempMaiSe.Models;

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

app.MapPost("/send/{id}", async (int id, Stream data, IFluentEmail mailer, Instrumentation instrumentation, MailingContext mailingContext, FluidParser fluidParser, CancellationToken cancellationToken) =>
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

    List<Newtonsoft.Json.Schema.ValidationError> errors = new();
    validatingReader.ValidationEventHandler += (o, a) => errors.Add(a.ValidationError);

    JsonSerializer serializer = new();
    dynamic dynamic = serializer.Deserialize<ExpandoObject>(validatingReader)!;
    if (errors.Count > 0)
    {
        return Results.BadRequest(errors);
    }

    IFluidTemplate fluidSubjectTemplate = fluidParser.Parse(template.SubjectTemplate);
    TemplateContext templateContext = new(dynamic);
    string subject = await fluidSubjectTemplate.RenderAsync(templateContext).ConfigureAwait(false);

    IFluentEmail mail = mailer.Subject(subject);
    if (template.From is MailAddress from)
    {
        mail = mail.SetFrom(from.Address, from.Name);
    }

    foreach (MailAddress to in template.To)
    {
        mail = mail.To(to.Address, to.Name);
    }

    foreach (MailAddress cc in template.Cc)
    {
        mail = mail.CC(cc.Address, cc.Name);
    }

    foreach (MailAddress bcc in template.Bcc)
    {
        mail = mail.BCC(bcc.Address, bcc.Name);
    }

    foreach (MailAddress replyTo in template.ReplyTo)
    {
        mail = mail.ReplyTo(replyTo.Address, replyTo.Name);
    }

    foreach (Tag tag in template.Tags)
    {
        mail = mail.Tag(tag.Name);
    }

    foreach (Header header in template.Headers)
    {
        mail = mail.Header(header.Name, header.Value);
    }

    switch (template.Priority)
    {
        case Priority.High:
            mail = mail.HighPriority();
            break;
        case Priority.Low:
            mail = mail.LowPriority();
            break;
    }

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
