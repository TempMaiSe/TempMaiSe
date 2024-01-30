# TempMaiSe

TempMaiSe is an open-source library designed to simplify the process of sending templated emails using the [Fluid](https://github.com/sebastienros/fluid) templating engine and the [FluentEmail](https://github.com/lukencode/FluentEmail) sender. It provides a convenient and intuitive way to generate dynamic email content.

## Features

-   Easy integration with FluentEmail sender
-   Support for Fluid templating engine
-   Simplified email template retrieval with custom repository implementation

## Installation

To use the TempMaiSe library in your project, follow these steps:

1. Add a reference to the [`TempMaiSe.Mailer`](https://www.nuget.org/packages/TempMaiSe.Mailer) NuGet package.

## Usage

To implement a custom repository and use the TempMaiSe library, follow these steps:

1. Create a class that implements the `ITemplateRepository` interface.
2. Override the `GetTemplateAsync` method to retrieve the template based on the provided ID.
3. Register the custom repository in your project's `Startup.cs` file.
4. Register the necessary components in your `Startup.cs` file, including the custom repository, FluentEmail setup, and the default IMailService implementation.
5. Utilize the provided API sample to send an email using the configured mail service.

Please note that additional authentication and authorization features may need to be implemented according to your specific requirements.

For more information, refer to the [TempMaiSe documentation](https://github.com/hangy/solid-train).

### Custom Repository

The solid-train distribution does not come with a default repository. However, implementing a custom repository is straightforward.

To implement a custom repository, follow these steps:

1. Create a class that implements the `ITemplateRepository` interface.
2. Override the `GetTemplateAsync` method to retrieve the template based on the provided ID.
3. Register the custom repository in your project's `Startup.cs` file.

Here's an example of a custom repository that only returns one `Template`.

```csharp
public class TemplateRepository : ITemplateRepository
{
    private static readonly s_theOneTemplate = new Template
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
                    };

    /// <inheritdoc />
    public Task<Template?> GetTemplateAsync(int id, CancellationToken cancellationToken = default)
    {
        Template? result;
        if (id == s_theOneTemplate.Id)
        {
            result = s_theOneTemplate;
        }

        return Task.FromResult(result);
    }
}
```

### Registering Components in `Startup.cs`

To use the TempMaiSe library in your project, you need to register the necessary components in your `Startup.cs` file. Here's how you can do it:

```csharp
// Your custom repository
builder.Services.AddScoped<ITemplateRepository, TemplateRepository>();

// Use the usual FluentEmail setup to register IFluentEmail
builder.Services.AddFluentEmail(config);

// Register the default IMailService implementation
builder.Services.AddMailService();
```

### Example REST-API

To send an email using the configured mail service, you can utilize the following API sample. It is important to note that this sample does not include authentication or authorization, so it is recommended to implement those features according to your specific requirements.

```csharp
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
```

## Contributing

Contributions are welcome! If you encounter any issues or have suggestions for improvements, please open an issue or submit a pull request on the [TempMaiSe GitHub repository](https://github.com/hangy/solid-train).

## License

Solid-Train is released under the [MIT License](https://opensource.org/licenses/MIT).
