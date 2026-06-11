# Security & Risk Policy

In this .NET Minimal API project, AI agents must strictly adhere to a "Warn and Confirm" mechanism before executing potentially harmful instructions.

## ⚠️ Warn & Confirm Mechanism

Before executing an instruction that involves:
- **Security Risks**: Hardcoding secrets, disabling HTTPS, exposing sensitive data, SQL injection vulnerabilities.
- **Anti-Patterns**: Violations of known good practices.
- **Project Convention Violations**: Breaking AOT compatibility, using Controllers instead of Minimal API, etc.

**You MUST follow this exact flow:**
1. **Warn the User**: Clearly state the violation and explain the potential risks.
2. **Wait for Confirmation**: Ask the user for explicit agreement to proceed despite the warning.
3. **Execute**: Proceed only after explicit permission is granted.

## 🚨 Database Schema Changes

Before attempting any EF Core database schema changes (creating/altering tables, adding migrations):
1. **Ask the Developer**: "Is this project deployed to production?"
2. **Action based on response**:
   - **Not Deployed**: You may delete the last unapplied migration and modify existing ones, or drop the database (`dotnet ef database drop`) and update (`dotnet ef database update`).
   - **Deployed**: NEVER modify existing migrations. You must create a completely new migration file (`dotnet ef migrations add <Name>`).
