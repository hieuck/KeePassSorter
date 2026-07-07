```markdown
# KeePassSorter Development Patterns

> Auto-generated skill from repository analysis

## Overview
This skill covers the development patterns and conventions used in the KeePassSorter C# codebase. You'll learn how to structure files, write imports and exports, follow commit message guidelines, and manage testing. This guide also outlines common workflows and provides suggested commands to streamline your development process.

## Coding Conventions

### File Naming
- Use **PascalCase** for all file names.
  - Example: `KeePassSorter.cs`, `EntrySorter.cs`

### Import Style
- Use **relative imports** for referencing other files within the project.
  - Example:
    ```csharp
    using KeePassSorter.Models;
    using KeePassSorter.Utils;
    ```

### Export Style
- Use **named exports** for classes and methods.
  - Example:
    ```csharp
    public class EntrySorter
    {
        // ...
    }
    ```

### Commit Messages
- Follow the **conventional commit** pattern.
- Use the `feat` prefix for new features.
- Keep commit messages concise (average 54 characters).
  - Example:  
    ```
    feat: add sorting by creation date
    ```

## Workflows

### Feature Development
**Trigger:** When adding a new feature to KeePassSorter  
**Command:** `/feature`

1. Create a new branch for your feature.
2. Implement the feature using PascalCase file naming and relative imports.
3. Write or update tests in `*.test.*` files.
4. Commit changes using the `feat` prefix in your commit message.
5. Open a pull request for review.

### Code Import/Export
**Trigger:** When creating or updating modules/classes  
**Command:** `/module-update`

1. Name new files/classes using PascalCase.
2. Use `using` statements for relative imports.
3. Export classes and methods with `public` access modifiers.

### Testing
**Trigger:** When writing or updating tests  
**Command:** `/test`

1. Create or update test files matching the pattern `*.test.*`.
2. Write tests for new or modified features.
3. Run tests using the project's preferred test runner.

## Testing Patterns

- Test files follow the `*.test.*` naming convention.
- The specific testing framework is not detected; use the project's standard approach.
- Place test files alongside or within a dedicated test directory as appropriate.
- Example test file: `EntrySorter.test.cs`

## Commands
| Command        | Purpose                                      |
|----------------|----------------------------------------------|
| /feature       | Start a new feature development workflow     |
| /module-update | Add or update modules/classes                |
| /test          | Write or update and run tests                |
```