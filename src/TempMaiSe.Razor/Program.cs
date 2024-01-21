using Microsoft.AspNetCore.Identity;

using Fluid;
using Newtonsoft.Json.Schema;

using System.Diagnostics;

using TempMaiSe.Mailer;
using TempMaiSe.Models;
using TempMaiSe.OpenTelemetry;
using TempMaiSe.Razor;

using OneOf;
using OneOf.Types;

Activity.DefaultIdFormat = ActivityIdFormat.W3C;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
ConfigurationManager config = builder.Configuration;

builder.AddOpenTelemetry<Program>();

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddSingleton<FluidParser>();

builder.Services.AddFluentEmail(config);

builder.Services.AddSingleton<ITemplateToMailHeadersMapper, TemplateToMailHeadersMapper>();
builder.Services.AddSingleton<IMailInformationToMailHeadersMapper, MailInformationToMailHeadersMapper>();

builder.Services.AddSingleton<IDataParser, DataParser>();

builder.Services.AddTemplateContext(config);
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddScoped<ITemplateRepository, TemplateRepository>();
builder.Services.AddScoped<IMailService, MailService>();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<TempMaiSe.Razor.TemplateContext>();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<TempMaiSe.Razor.TemplateContext>();

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

app.MapPost("/send/{id}", async (int id, Stream data, IMailService mailService, CancellationToken cancellationToken) =>
{
    static IDictionary<string, string[]> ConvertValidationErrorsToValidationProblem(List<ValidationError> validationErrors)
    {
        return validationErrors
            .GroupBy(error => error.Path)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.Message).ToArray()
            );
    }

    OneOf<FluentEmail.Core.Models.SendResponse, NotFound, List<ValidationError>> result = await mailService.SendMailAsync(id, data, cancellationToken).ConfigureAwait(false);

    return result.Match(
        sent => Results.Ok(sent),
        notFound => Results.NotFound(),
        validationErrors => Results.ValidationProblem(ConvertValidationErrorsToValidationProblem(validationErrors))
    );
});

app.Run();

public partial class Program { }
