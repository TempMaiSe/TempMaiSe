using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;

using Microsoft.AspNetCore.TestHost;

using TempMaiSe.Mailer;
using TempMaiSe.Models;

using Testcontainers.Papercut;

namespace TempMaiSe.Samples.Api.Tests;

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
    public async Task Post_Send_Returns_Success_For_Good_Template_Id_Simple_Object()
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
                    using TemplateContext context = services.BuildServiceProvider().GetRequiredService<TemplateContext>();
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
            ReplyTo = ["lawyer@example.com"],
            To = ["please-scam-me@example.com"],
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
        using HttpClient httpClient = new();
        httpClient.BaseAddress = new Uri(container.GetBaseAddress());
        PapercutMessageList? messages = await httpClient.GetFromJsonAsync<PapercutMessageList>(new Uri("/api/messages", UriKind.Relative)).ConfigureAwait(true);
        Assert.Equal(1, messages!.TotalMessageCount);
        PapercutMessage? message = await httpClient.GetFromJsonAsync<PapercutMessage>(new Uri($"/api/messages/{messages.Messages.Single().Id}", UriKind.Relative)).ConfigureAwait(true);
        Assert.NotNull(message);
        Assert.Equal("Inheritance from Uncle Bob", message.Subject);
        Assert.Equal("Please send me 1.000 $. My paypal is paypal@example.net", message.TextBody);
        Assert.Null(message.HtmlBody);

        Assert.NotNull(message.From);
        Assert.Contains(message.From, address => address.Address == "government@example.org");

        Assert.NotNull(message.To);
        Assert.Contains(message.To, address => address.Address == "please-scam-me@example.com");

        Assert.NotNull(message.Bcc);
        Assert.Contains(message.Bcc, address => address.Address == "prince@example.org");
    }

    [Fact]
    public async Task Post_Send_Returns_Error_For_Missing_Required_Property()
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
                    using TemplateContext context = services.BuildServiceProvider().GetRequiredService<TemplateContext>();
                    context.Templates.Add(new Template
                    {
                        Id = 69,
                        Data = new TemplateData
                        {
                            SubjectTemplate = "Inheritance from Uncle {{ uncle }}",
                            Priority = Priority.High,
                            Bcc = {
                                new() { Address = "prince@example.org" }
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
            ReplyTo = ["lawyer@example.com"],
            To = ["please-scam-me@example.com"],
            Priority = Priority.Normal,
            Data = new Dictionary<string, object>()
        };

        // Act
        using HttpContent content = new StringContent(JsonSerializer.Serialize(mail), Encoding.UTF8, "application/json");
        HttpResponseMessage response = await client.PostAsync(new Uri("/send/69", UriKind.Relative), content).ConfigureAwait(true);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        string error = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
        Assert.Contains("Required properties are missing from object: email.", error, StringComparison.InvariantCultureIgnoreCase);
    }

    [Fact]
    public async Task Post_Send_Returns_Success_For_Good_Template_Id_Complex_Object()
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
                    using TemplateContext context = services.BuildServiceProvider().GetRequiredService<TemplateContext>();
                    context.Templates.Add(new Template
                    {
                        Id = 1337,
                        Data = new TemplateData
                        {
                            SubjectTemplate = "{{ Head.Name }} — Claim your free gift now",
                            PlainTextBodyTemplate = "Use a HTML mailer, {{ Head.Name }}!",
                            HtmlBodyTemplate =
"""
<p>Hi {{Head.Name}},<br><br>
you received a coupon for a free gift. Please use it within the next 24 hours.</p>

<ul>
    {% for item in CouponData.Items %}
    <li>{{ item.Name }}</li>
    {% endfor %}
</ul>

<p>Best,<br>
{{ Head.SupportAgent }}</p>
""",
                            JsonSchema =
"""
{
    "$schema": "https://json-schema.org/draft/2020-12/schema",
    "type": "object",
    "properties": {
        "Head": {
            "type": "object",
            "properties": {
                "Name": { "type": "string" },
                "SupportAgent": { "type": "string" }
            },
            "required": ["Name", "SupportAgent"]
        },
        "CouponData": {
            "type": "object",
            "Items": {
                "type": "array",
                "items": {
                    "type": "object",
                    "properties": {
                        "Name": { "type": "string" }
                    },
                    "required": ["Name"]
                }
            },
            "required": ["Items"]
        }
    },
    "required": ["Head", "CouponData"]
}
"""
                        }
                    });
                    context.SaveChanges();
                });
            }).CreateClient();

        List<CouponItem> couponItems =
        [
            new("AK <script>47"),
            new("M4A1-S")
        ];
        MailInformation mail = new()
        {
            From = "couponmanager@example.org",
            To = ["couponer@example.com"],
            Priority = Priority.Normal,
            Data = new Dictionary<string, object>
            {
                { "Head", new Dictionary<string, object>{ { "Name", "Erika Mustermann" }, { "SupportAgent", "Neo" } } },
                { "CouponData", new Dictionary<string, object>{ { "Items", couponItems } } },
            }
        };

        // Act
        using HttpContent content = new StringContent(JsonSerializer.Serialize(mail), Encoding.UTF8, "application/json");
        HttpResponseMessage response = await client.PostAsync(new Uri("/send/1337", UriKind.Relative), content).ConfigureAwait(true);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using HttpClient httpClient = new();
        httpClient.BaseAddress = new Uri(container.GetBaseAddress());
        PapercutMessageList? messages = await httpClient.GetFromJsonAsync<PapercutMessageList>(new Uri("/api/messages", UriKind.Relative)).ConfigureAwait(true);
        Assert.Equal(1, messages!.TotalMessageCount);
        PapercutMessage? message = await httpClient.GetFromJsonAsync<PapercutMessage>(new Uri($"/api/messages/{messages.Messages.Single().Id}", UriKind.Relative)).ConfigureAwait(true);
        Assert.NotNull(message);
        Assert.Equal("Erika Mustermann — Claim your free gift now", message.Subject);
        Assert.Equal(
"""
<p>Hi Erika Mustermann,<br><br>
you received a coupon for a free gift. Please use it within the next 24 hours.</p>

<ul>
    
    <li>AK &lt;script&gt;47</li>
    
    <li>M4A1-S</li>
    
</ul>

<p>Best,<br>
Neo</p>
""", message.HtmlBody);
        Assert.Equal("Use a HTML mailer, Erika Mustermann!", message.TextBody);

        Assert.NotNull(message.From);
        Assert.Contains(message.From, address => address.Address == "couponmanager@example.org");

        Assert.NotNull(message.To);
        Assert.Contains(message.To, address => address.Address == "couponer@example.com");
    }

    [Fact]
    public async Task Post_Send_Returns_Success_For_Good_Template_Id_Complex_Object_With_Attachment()
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
                    using TemplateContext context = services.BuildServiceProvider().GetRequiredService<TemplateContext>();
                    context.Templates.Add(new Template
                    {
                        Id = 420,
                        Data = new TemplateData
                        {
                            Attachments =
                            {
                                new() { FileName = "coupon.pdf", MediaType = "application/pdf", Data = Encoding.UTF8.GetBytes("dummy") }
                            },
                            SubjectTemplate = "{{ Head.Name }} — Claim your free gift now",
                            PlainTextBodyTemplate = "Use a HTML mailer, {{ Head.Name }}!",
                            HtmlBodyTemplate =
"""
<p>Hi {{Head.Name}},<br><br>
you received a coupon for a free gift. Please use it within the next 24 hours.</p>

<ul>
    {% for item in CouponData.Items %}
    <li>{{ item.Name }}</li>
    {% endfor %}
</ul>

<p>Best,<br>
{{ Head.SupportAgent }}</p>
""",
                            JsonSchema =
"""
{
    "$schema": "https://json-schema.org/draft/2020-12/schema",
    "type": "object",
    "properties": {
        "Head": {
            "type": "object",
            "properties": {
                "Name": { "type": "string" },
                "SupportAgent": { "type": "string" }
            },
            "required": ["Name", "SupportAgent"]
        },
        "CouponData": {
            "type": "object",
            "Items": {
                "type": "array",
                "items": {
                    "type": "object",
                    "properties": {
                        "Name": { "type": "string" }
                    },
                    "required": ["Name"]
                }
            },
            "required": ["Items"]
        }
    },
    "required": ["Head", "CouponData"]
}
"""
                        }
                    });
                    context.SaveChanges();
                });
            }).CreateClient();

        List<CouponItem> couponItems =
        [
            new("AK <script>47"),
            new("M4A1-S")
        ];
        MailInformation mail = new()
        {
            From = "couponmanager@example.org",
            To = ["couponer@example.com"],
            Priority = Priority.Normal,
            Data = new Dictionary<string, object>
            {
                { "Head", new Dictionary<string, object>{ { "Name", "Erika Mustermann" }, { "SupportAgent", "Neo" } } },
                { "CouponData", new Dictionary<string, object>{ { "Items", couponItems } } },
            },
            Attachments =
            {
                new() { FileName = "logo.png", MediaType = "image/png", Data = Encoding.UTF8.GetBytes("dummy2") }
            },
        };

        // Act
        using HttpContent content = new StringContent(JsonSerializer.Serialize(mail), Encoding.UTF8, "application/json");
        HttpResponseMessage response = await client.PostAsync(new Uri("/send/420", UriKind.Relative), content).ConfigureAwait(true);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using HttpClient httpClient = new();
        httpClient.BaseAddress = new Uri(container.GetBaseAddress());
        PapercutMessageList? messages = await httpClient.GetFromJsonAsync<PapercutMessageList>(new Uri("/api/messages", UriKind.Relative)).ConfigureAwait(true);
        Assert.Equal(1, messages!.TotalMessageCount);
        PapercutMessage? message = await httpClient.GetFromJsonAsync<PapercutMessage>(new Uri($"/api/messages/{messages.Messages.Single().Id}", UriKind.Relative)).ConfigureAwait(true);
        Assert.NotNull(message);
        Assert.Equal("Erika Mustermann — Claim your free gift now", message.Subject);
        Assert.Equal(
"""
<p>Hi Erika Mustermann,<br><br>
you received a coupon for a free gift. Please use it within the next 24 hours.</p>

<ul>
    
    <li>AK &lt;script&gt;47</li>
    
    <li>M4A1-S</li>
    
</ul>

<p>Best,<br>
Neo</p>
""", message.HtmlBody);
        Assert.Equal("Use a HTML mailer, Erika Mustermann!", message.TextBody);

        Assert.NotNull(message.From);
        Assert.Contains(message.From, address => address.Address == "couponmanager@example.org");

        Assert.NotNull(message.To);
        Assert.Contains(message.To, address => address.Address == "couponer@example.com");

        Assert.Contains(message.Sections, section => section.MediaType == "application/pdf" && section.FileName == "coupon.pdf");
        Assert.Contains(message.Sections, section => section.MediaType == "image/png" && section.FileName == "logo.png");
    }

    private record CouponItem(string Name);
}
