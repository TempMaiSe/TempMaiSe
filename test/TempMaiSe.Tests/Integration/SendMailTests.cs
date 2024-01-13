using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.TestHost;
using TempMaiSe.Mailer;
using TempMaiSe.Models;
using Testcontainers.Papercut;

namespace TempMaiSe.Tests.Integration;

[Trait("Category", "Integration")]
public class SendMailTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public SendMailTests(CustomWebApplicationFactory<Program> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _factory = factory;
    }

    [Fact]
    public async Task Post_Send_Returns_Not_Found_For_Random_Template_Id()
    {
        // Arrange
        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync(new Uri("/send/4711", UriKind.Relative), "dummy").ConfigureAwait(true);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Post_Send_Returns_Success_For_Good_Template_Id()
    {
        // Arrange
        PapercutContainer container = new PapercutBuilder()
            .Build();
        await container.StartAsync().ConfigureAwait(true);

        HttpClient client = _factory
            .WithWebHostBuilder(configuration =>
            {
                configuration.UseSetting("FluentEmail:Sender", "Smtp");
                configuration.UseSetting("FluentEmail:Smtp:Server", container.Hostname);
                configuration.UseSetting("FluentEmail:Smtp:Port", container.SmtpPort.ToString(CultureInfo.InvariantCulture));

                configuration.ConfigureTestServices(services =>
                {
                    using MailingContext context = services.BuildServiceProvider().GetRequiredService<MailingContext>();
                    context.Templates.Add(new Template
                    {
                        Id = 42,
                        Data = new TemplateData
                        {
                            SubjectTemplate = "Inheritance from Uncle {{ uncle }}",
                            Priority = Priority.High,
                            Bcc = {
                                new() { Address= "prince@example.org" }
                            },
                            PlainTextBodyTemplate = "Please send me 1.000 $. My paypal is {{ email }}",
                            JsonSchema =
"""
{
    "$schema": "https://json-schema.org/draft/2020-12/schema",
    "type": "object",
    "properties": {
        "email": { "type": "string", "format": "email" },
        "uncle": { "type": "string" }
    },
    "required": ["email"]
}
"""
                        }
                    });
                    context.SaveChanges();
                });
            }).CreateClient();

        MailInformation mail = new()
        {
            From = "government@example.org",
            ReplyTo = new[] { "lawyer@example.com" },
            To = new[] { "please-scam-me@examle.org" },
            Priority = Priority.Normal,
            Data = new Dictionary<string, object>()
            {
                { "email", "paypal@example.net" },
                { "uncle", "Bob" }
            }
        };

        // Act
        using HttpContent content = new StringContent(JsonSerializer.Serialize(mail), Encoding.UTF8, "application/json");
        HttpResponseMessage response = await client.PostAsync(new Uri("/send/42", UriKind.Relative), content).ConfigureAwait(true);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using HttpClient httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(container.GetBaseAddress());
        PapercutMessageList? messages = await httpClient.GetFromJsonAsync<PapercutMessageList>(new Uri("/api/messages", UriKind.Relative)).ConfigureAwait(true);
        Assert.Equal(1, messages!.TotalMessageCount);
        PapercutMessage message = await httpClient.GetFromJsonAsync<PapercutMessage>(new Uri($"/api/messages/{messages.Messages.Single().Id}", UriKind.Relative)).ConfigureAwait(true);
        Assert.Equal("Inheritance from Uncle Bob", message?.Subject);
        Assert.Equal("Please send me 1.000 $. My paypal is paypal@example.net", message?.TextBody);

        await container.StopAsync().ConfigureAwait(true);
    }
}
