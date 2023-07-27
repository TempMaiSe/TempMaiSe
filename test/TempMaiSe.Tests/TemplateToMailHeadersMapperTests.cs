using FluentEmail.Core;
using TempMaiSe.Mailer;
using TempMaiSe.Models;

namespace TempMaiSe.Tests;

public class TemplateToMailHeadersMapperTests
{
    private readonly TemplateToMailHeadersMapper _mapper = new();

    [Fact]
    public void Map_Returns_Original_Email_Instance()
    {
        // Arrange
        Template template = new() { To = { new("dummy@example.org") } };
        Email email = new();

        // Act
        IFluentEmail actual = _mapper.Map(template, email);

        // Assert
        Assert.Same(actual, email);
    }

    [Fact]
    public void Map_Throws_ArgumentNullException_When_Given_No_Template()
    {
        // Arrange
        IFluentEmail email = new Mock<IFluentEmail>().Object;

        // Act & Assert
        _ = Assert.Throws<ArgumentNullException>("configuredTemplate", () => _mapper.Map(null, email));
    }

    [Fact]
    public void Map_Throws_ArgumentNullException_When_Given_No_Email()
    {
        // Arrange
        Template template = new() { To = { new("foo@example.org", "Foo Bar") } };

        // Act & Assert
        _ = Assert.Throws<ArgumentNullException>("email", () => _mapper.Map(template, null));
    }

    [Theory]
    [InlineData("foo@example.org", null)]
    [InlineData("foo@example.net", "dummy")]
    public void Map_Sets_From_From_Template(string address, string? name)
    {
        // Arrange
        Template template = new() { From = new(address, name) };
        Mock<IFluentEmail> emailMock = new();
        emailMock.Setup(it => it.SetFrom(address, name)).Returns(emailMock.Object).Verifiable();

        // Act
        _ = _mapper.Map(template, emailMock.Object);

        // Assert
        emailMock.VerifyAll();
        emailMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("foo@example.org", null)]
    [InlineData("foo@example.net", "dummy")]
    public void Map_Adds_To_From_Template(string address, string? name)
    {
        // Arrange
        Template template = new() { To = { new(address, name) } };
        Mock<IFluentEmail> emailMock = new();
        emailMock.Setup(it => it.To(address, name)).Returns(emailMock.Object).Verifiable();

        // Act
        _ = _mapper.Map(template, emailMock.Object);

        // Assert
        emailMock.VerifyAll();
        emailMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("foo@example.org", null)]
    [InlineData("foo@example.net", "dummy")]
    public void Map_Adds_Cc_From_Template(string address, string? name)
    {
        // Arrange
        Template template = new() { Cc = { new(address, name) } };
        Mock<IFluentEmail> emailMock = new();
        emailMock.Setup(it => it.CC(address, name)).Returns(emailMock.Object).Verifiable();

        // Act
        _ = _mapper.Map(template, emailMock.Object);

        // Assert
        emailMock.VerifyAll();
        emailMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("foo@example.org", null)]
    [InlineData("foo@example.net", "dummy")]
    public void Map_Adds_Bcc_From_Template(string address, string? name)
    {
        // Arrange
        Template template = new() { Bcc = { new(address, name) } };
        Mock<IFluentEmail> emailMock = new();
        emailMock.Setup(it => it.BCC(address, name)).Returns(emailMock.Object).Verifiable();

        // Act
        _ = _mapper.Map(template, emailMock.Object);

        // Assert
        emailMock.VerifyAll();
        emailMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("foo@example.org", null)]
    [InlineData("foo@example.net", "dummy")]
    public void Map_Adds_ReplyTo_From_Template(string address, string? name)
    {
        // Arrange
        Template template = new() { ReplyTo = { new(address, name) } };
        Mock<IFluentEmail> emailMock = new();
        emailMock.Setup(it => it.ReplyTo(address, name)).Returns(emailMock.Object).Verifiable();

        // Act
        _ = _mapper.Map(template, emailMock.Object);

        // Assert
        emailMock.VerifyAll();
        emailMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("test")]
    [InlineData("some_other_value")]
    public void Map_Adds_Single_Tag_From_Template(string someTagName)
    {
        // Arrange
        Template template = new() { Tags = { new(someTagName) } };
        Mock<IFluentEmail> emailMock = new();
        emailMock.Setup(it => it.Tag(someTagName)).Returns(emailMock.Object).Verifiable();

        // Act
        _ = _mapper.Map(template, emailMock.Object);

        // Assert
        emailMock.VerifyAll();
        emailMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("test", "whatever")]
    [InlineData("some_other_value", "example")]
    public void Map_Adds_Multiple_Tags_From_Template(string someTagName, string someOtherTagName)
    {
        // Arrange
        Template template = new() { Tags = { new(someTagName), new(someOtherTagName) } };
        Mock<IFluentEmail> emailMock = new();
        emailMock.Setup(it => it.Tag(someTagName)).Returns(emailMock.Object).Verifiable();
        emailMock.Setup(it => it.Tag(someOtherTagName)).Returns(emailMock.Object).Verifiable();

        // Act
        _ = _mapper.Map(template, emailMock.Object);

        // Assert
        emailMock.VerifyAll();
        emailMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("test", "it")]
    [InlineData("some_other_value", "is also cool")]
    public void Map_Adds_Single_Header_From_Template(string headerName, string headerValue)
    {
        // Arrange
        Template template = new() { Headers = { new(headerName, headerValue) } };
        Mock<IFluentEmail> emailMock = new();
        emailMock.Setup(it => it.Header(headerName, headerValue)).Returns(emailMock.Object).Verifiable();

        // Act
        _ = _mapper.Map(template, emailMock.Object);

        // Assert
        emailMock.VerifyAll();
        emailMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("test", "whatever", "do", "your thing")]
    [InlineData("some_other_value", "example", "x-dont-spam-me", "some score")]
    public void Map_Adds_Multiple_Headers_From_Template(string firstHeaderName, string firstHeaderValue, string secondHeaderName, string secondHeaderValue)
    {
        // Arrange
        Template template = new() { Headers = { new(firstHeaderName, firstHeaderValue), new(secondHeaderName, secondHeaderValue) } };
        Mock<IFluentEmail> emailMock = new();
        emailMock.Setup(it => it.Header(firstHeaderName, firstHeaderValue)).Returns(emailMock.Object).Verifiable();
        emailMock.Setup(it => it.Header(secondHeaderName, secondHeaderValue)).Returns(emailMock.Object).Verifiable();

        // Act
        _ = _mapper.Map(template, emailMock.Object);

        // Assert
        emailMock.VerifyAll();
        emailMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void Map_Sets_LowPriority_From_Template()
    {
        // Arrange
        Template template = new() { Priority = Priority.Low };
        Mock<IFluentEmail> emailMock = new();
        emailMock.Setup(it => it.LowPriority()).Returns(emailMock.Object).Verifiable();

        // Act
        _ = _mapper.Map(template, emailMock.Object);

        // Assert
        emailMock.VerifyAll();
        emailMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void Map_Sets_HighPriority_From_Template()
    {
        // Arrange
        Template template = new() { Priority = Priority.High };
        Mock<IFluentEmail> emailMock = new();
        emailMock.Setup(it => it.HighPriority()).Returns(emailMock.Object).Verifiable();

        // Act
        _ = _mapper.Map(template, emailMock.Object);

        // Assert
        emailMock.VerifyAll();
        emailMock.VerifyNoOtherCalls();
    }
}
