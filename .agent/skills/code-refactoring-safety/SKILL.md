---
name: code-refactoring-safety
description: Safety rules for code refactoring and tool usage.
---

# code-refactoring-safety

When refactoring code in this project, you must adhere to the following safety rules:

## 1. No Script-Based Refactoring
- NEVER use bash scripts (`sed`, `awk`, `grep`), PowerShell, or other text-manipulation scripts to mass-refactor C# code.
- Scripts do not understand C# syntax, variable scopes, or `using` namespace directives and frequently break the code.

## 2. Proper Tool Usage
- Use the dedicated file manipulation tools (`replace_file_content`, `multi_replace_file_content`) to accurately patch or rewrite code.
- Ensure that the resulting code remains syntactically valid and all necessary `using` directives are present.

## 3. Post-Refactor Verification
- After making significant changes, run `dotnet build` to ensure the project still compiles without errors.
- Do not leave the project in a broken state after your turn.
