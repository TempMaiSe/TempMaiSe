using Microsoft.AspNetCore.Identity;

using Fluid;

using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using System.Text;
using FluentEmail.Core;
using System.Diagnostics;

using TempMaiSe.Mailer;
using TempMaiSe.Models;
using TempMaiSe.Razor;

Activity.DefaultIdFormat = ActivityIdFormat.W3C;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
ConfigurationManager config = builder.Configuration;

builder.AddOpenTelemetry();

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddSingleton<FluidParser>();

builder.Services.AddFluentEmail(config);

builder.Services.AddSingleton<ITemplateToMailHeadersMapper, TemplateToMailHeadersMapper>();
builder.Services.AddSingleton<IMailInformationToMailHeadersMapper, MailInformationToMailHeadersMapper>();

// Add services to the container.
builder.Services.AddMailingContext(config);
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<MailingContext>();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<MailingContext>();

builder.Services.AddRazorPages();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.MapPost("/send/{id}", async (int id, Stream data, IFluentEmail mailer, Instrumentation instrumentation, MailingContext mailingContext, FluidParser fluidParser, ITemplateToMailHeadersMapper mailHeaderMapper, IMailInformationToMailHeadersMapper mailInfoMapper, CancellationToken cancellationToken) =>
{
    using Activity? activity = instrumentation.ActivitySource.StartActivity("SendMail")!;
    activity?.AddTag("TemplateId", id);

    Template? template = await mailingContext.Templates.FindAsync(new object[] { id }, cancellationToken).ConfigureAwait(false);
    if (template is null)
    {
        return Results.NotFound();
    }

    TemplateData templateData = template.Data;

    using var sr = new StreamReader(data, Encoding.UTF8);
    string str = await sr.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
    JsonTextReader reader = new(new StringReader(str));

    JSchemaValidatingReader validatingReader = new(reader)
    {
        Schema = JSchema.Parse(templateData.JsonSchema)
    };

    List<ValidationError> errors = [];
    validatingReader.ValidationEventHandler += (o, a) => errors.Add(a.ValidationError);

    JsonSerializer serializer = new();
    MailInformation mailInformation = serializer.Deserialize<MailInformation>(validatingReader)!;
    if (errors.Count > 0)
    {
        return Results.BadRequest(errors);
    }

    IFluidTemplate fluidSubjectTemplate = fluidParser.Parse(templateData.SubjectTemplate);
    TemplateContext templateContext = new(mailInformation.Data);
    string subject = await fluidSubjectTemplate.RenderAsync(templateContext).ConfigureAwait(false);

    IFluentEmail mail = mailer.Subject(subject);
    mail = mailHeaderMapper.Map(templateData, mail);
    mail = mailInfoMapper.Map(mailInformation, mail);

    if (string.IsNullOrWhiteSpace(templateData.HtmlBodyTemplate))
    {
        IFluidTemplate plainTextFluidTemplate = fluidParser.Parse(templateData.PlainTextBodyTemplate);
        string plainTextBody = await plainTextFluidTemplate.RenderAsync(templateContext).ConfigureAwait(false);
        mail = mail.Body(plainTextBody, false);
    }
    else
    {
        IFluidTemplate htmlFluidTemplate = fluidParser.Parse(templateData.HtmlBodyTemplate);
        string htmlBody = await htmlFluidTemplate.RenderAsync(templateContext, System.Text.Encodings.Web.HtmlEncoder.Default).ConfigureAwait(false);
        mail = mail.Body(htmlBody, true);

        if (!string.IsNullOrWhiteSpace(templateData.PlainTextBodyTemplate))
        {
            IFluidTemplate plainTextFluidTemplate = fluidParser.Parse(templateData.PlainTextBodyTemplate);
            string plainTextBody = await plainTextFluidTemplate.RenderAsync(templateContext).ConfigureAwait(false);
            mail = mail.PlaintextAlternativeBody(plainTextBody);
        }
    }

    FluentEmail.Core.Models.SendResponse resp = await mail.SendAsync(cancellationToken).ConfigureAwait(false);

    instrumentation.MailsSent.Add(1);

    return Results.Ok(resp);
});

app.Run();
