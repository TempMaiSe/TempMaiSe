using Fluid;

namespace TempMaiSe.Tests;

public class MailServiceExtensionsTests
{
    [Fact]
    public void AddMailService_Throws_ArgumentNullException_When_Given_Null_ServiceCollection()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act & Assert
        _ = Assert.Throws<ArgumentNullException>("services", () => MailServiceExtensions.AddMailService(services!));
    }

    [Fact]
    public void AddMailService_Can_Be_Called_Without_Parser_Configuration()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();

        // Act
        MailServiceExtensions.AddMailService(services);

        // Assert
        Assert.Single(services.Where(sr => sr.ServiceType.IsAssignableFrom(typeof(FluidParser))));
    }

    [Fact]
    public void AddMailService_Calls_Given_Action_To_Configure_Parser()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        bool called = false;
        void configureParser(IServiceProvider _, FluidParser __) => called = true;

        // Act
        MailServiceExtensions.AddMailService(services, configureParser);

        // Assert
        _ = services.BuildServiceProvider().GetService<FluidParser>(); // Ensure parser is created
        Assert.Single(services.Where(sr => sr.ServiceType.IsAssignableFrom(typeof(FluidParser))));
        Assert.True(called);
    }
}