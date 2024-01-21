using Fluid;
using Newtonsoft.Json.Schema;
using FluentEmail.Core;
using FluentEmail.Core.Models;

using System.Diagnostics;
using TempMaiSe.Mailer;
using TempMaiSe.Models;
using TempMaiSe.OpenTelemetry;

using OneOf;
using OneOf.Types;

using Microsoft.EntityFrameworkCore;

namespace TempMaiSe.Tests;

[Trait("Category", "Unit")]
public class MailServiceTests
{
    [Fact]
    public async Task SendMailAsync_TemplateNotFound_ReturnsNotFound()
    {
        // Arrange
        DbContextOptions<MailingContext> options = new DbContextOptionsBuilder<MailingContext>()
            .UseInMemoryDatabase(databaseName: nameof(SendMailAsync_TemplateNotFound_ReturnsNotFound))
            .Options;
        Mock<IFluentEmail> mailer = new();
        Mock<IMailingInstrumentation> instrumentation = GetInstrumentationMock();
        Mock<ITemplateRepository> templateRepository = new();
        Mock<DataParser> dataParser = new();
        Mock<FluidParser> fluidParser = new();
        Mock<ITemplateToMailHeadersMapper> mailHeaderMapper = new();
        Mock<IMailInformationToMailHeadersMapper> mailInfoMapper = new();

        int templateId = 1;
        templateRepository.Setup(c => c.GetTemplateAsync(templateId, It.IsAny<CancellationToken>())).Returns(Task.FromResult<Template?>(null));

        Stream data = new MemoryStream();

        MailService mailService = new(
            mailer.Object,
            instrumentation.Object,
            templateRepository.Object,
            dataParser.Object,
            fluidParser.Object,
            mailHeaderMapper.Object,
            mailInfoMapper.Object
        );

        // Act
        OneOf<SendResponse, NotFound, List<ValidationError>> result = await mailService.SendMailAsync(templateId, data).ConfigureAwait(true);

        // Assert
        Assert.IsType<NotFound>(result.Value);

        // Verify
        templateRepository.Verify();
    }

    private static Mock<IMailingInstrumentation> GetInstrumentationMock()
    {
#pragma warning disable CA2000 // Dispose objects before losing scope
        ActivitySource activitySource = new("Test");
#pragma warning restore CA2000 // Dispose objects before losing scope
        Mock<IMailingInstrumentation> instrumentation = new();
        instrumentation.SetupGet(c => c.ActivitySource).Returns(activitySource);
        return instrumentation;
    }
}
