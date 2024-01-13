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
                configuration.UseSetting("FluentEmail:MailKit:Server", $"{container.Hostname}:{container.SmtpPort}");

                configuration.ConfigureTestServices(services =>
                {
                    using MailingContext context = services.BuildServiceProvider().GetRequiredService<MailingContext>();
                    context.Templates.Add(new Template
                    {
                        Id = 42,
                        Data = new TemplateData
                        {
                            SubjectTemplate = "Inheritance from Uncle {{ Model.Uncle }}}",
                            Priority = Priority.High,
                            Bcc = {
                                new() { Address= "prince@example.org" }
                            },
                            PlainTextBodyTemplate = "Please send me 1.000 $. My paypal is {{ Model.email }}",
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
        var foo = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
        Assert.Equal("foo", foo);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
