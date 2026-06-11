# Tooling, Linting, and Scripts Policy

## 1. Development Tools
- Use the `dotnet CLI` (`dotnet run`, `dotnet watch`) for development with Hot Reload.
- Ensure `appsettings.json` and `appsettings.Development.json` are correctly configured before running.
- Test APIs using `.http` files (e.g., `DotNetMinimalAPI.http`) or via Scalar UI (`/scalar/v1`).

## 2. AOT Publish Verification
- Before officially releasing or finalizing a significant feature, you may verify AOT compilation using:
  `dotnet publish -r osx-arm64 -c Release` (Adjust the Runtime Identifier `-r` as needed).
- If there are AOT warnings during publish, you MUST report them to the developer. Do not ignore them.

## 3. Warning & Lint Suppression Policy
- **NEVER** add `#pragma warning disable` without explicit instruction from the user.
- If a compiler warning is encountered:
  1. Report it to the user.
  2. Wait for explicit instructions.
  3. If instructed to disable, add the suppression directive along with a justification comment.

## 4. No Scripts for Refactoring
- **ABSOLUTELY FORBIDDEN**: Do not use shell scripts (`sed`, `awk`, `bash`, `powershell`) to automate code refactoring or modifications. Scripts do not understand C# semantics, scope, or `using` directives.
- ✅ **ALLOWED**: Use AI file modification tools (e.g., `replace_file_content`, `multi_replace_file_content`) to accurately modify C# code. Always verify `using` declarations and build status after modifications.
