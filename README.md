# solid-train

The goal of this tool is to provide a REST API that allows programs to send [Fluid](https://github.com/sebastienros/fluid) templated emails using a [FluentEmail](https://github.com/lukencode/FluentEmail) sender. It is currently a **WORK IN PROGRESS**.

## Planned Features

- [ ] REST API that combines configured templates with passed data (JSON schema compatible) and sends the final mail
- [ ] Razor Pages for WYSIWYG template editing, allowing users to use dynamic tokens as placeholders, based on the JSON schema stored for the template
- [ ] ASP.NET Identity based user management with RBAC for template editing
- [ ] API keys for applications, with fine-grained permissions for access to templates
