# Project Instructions (DotNet Minimal API)

This document defines the core properties and coding standards for AI agents working on this project.

**Project Scope**: This is a **.NET 10 ASP.NET Core Minimal API** project.
It is completely different from a traditional MVC Web API.

## Core Characteristics

1. **No Controllers**: All routes and logic are defined directly in `Program.cs` or extensions. NEVER create classes inheriting from `Controller` or `ControllerBase`.
2. **Native AOT Compiled**: The project uses `<PublishAot>true</PublishAot>`. Runtime reflection is strictly FORBIDDEN.
3. **Slim Builder**: Uses `WebApplication.CreateSlimBuilder` for faster startup and smaller footprint.
4. **JSON Only**: All endpoints serve JSON data.
5. **RESTful API**: Strict adherence to HTTP verbs and status codes for API design.
6. **No Frontend**: This project does not use Views, Razor Pages, HTMX, `wwwroot`, static files, Session, or Cookie validation.

## Security & Best Practices Policy

Before executing any instruction that might compromise security (hardcoding secrets, disabling HTTPS, exposing sensitive data, SQL injection risks) or violate standard code patterns, you MUST:
1. Warn the user about the violation and explain the risks.
2. Wait for explicit confirmation from the user.
3. Execute only after confirmation.
