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

builder.Services.AddSingleton<DataParser>();

builder.Services.AddMailingContext(config);
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddSingleton<IMailService, MailService>();

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

app.MapPost("/send/{id}", async (int id, Stream data, MailService mailService, CancellationToken cancellationToken) =>
{
    OneOf<FluentEmail.Core.Models.SendResponse, NotFound, List<ValidationError>> result = await mailService.SendMailAsync(id, data, cancellationToken).ConfigureAwait(false);

    return result.Match(
        sent => Results.Ok(sent),
        notFound => Results.NotFound(),
        validationErrors => Results.BadRequest(validationErrors)
    );
});

app.Run();
