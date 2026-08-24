---
description: Unit test creation and maintenance standards for NUnit test work in this repository.
applyTo: "test/**/*.cs"
---

# Unit Test Instructions: NUnit

## Purpose and Scope
These instructions define how GitHub Copilot should create and maintain unit tests in this repository.

Scope restriction:
- These instructions apply only to creating and maintaining unit tests and their direct test support files.
- These instructions do not apply to production feature development, refactoring, or infrastructure tasks.

Hard constraints:
- Do not modify any class in a code project while building or updating unit tests.
- Use NUnit for all unit tests.
- Every file must contain only one class.

## How This Instructions File Is Used
When a request involves creating, updating, or validating unit tests, Copilot must:
1. Follow this file as the primary standard for test project structure, naming, implementation style, and validation.
2. Generate any missing unit test classes and methods required by these rules.
3. Validate test execution and fix test-project issues until tests run successfully (without changing code-project classes).
4. Keep all test artifacts aligned with these requirements over time.

## Project-to-Test-Project Mapping Rules
For each code project, there is a related testing project.

Rules:
- The testing project name starts with the related code project name and ends with `.Tests`.
- Example: `PilotApi.Shared` -> `PilotApi.Shared.Tests`.
- The testing project contains:
  - Unit test classes.
  - Test doubles and other files needed to support unit tests.

## Test Project Directory Structure Rules
The arrangement of unit test classes must mirror the arrangement of files in the related code project.

Requirements:
- Directory trees for test classes and source classes must match.
- If a code class is at `SourceProject/FolderA/FolderB/MyClass.cs`, its test class must be in the matching path under the related testing project.

Each testing project must include a special `Testing` directory with:
- `Testing/Doubles`: supporting test doubles classes.
- `Testing/Resources`: files loaded by tests (for example, test configuration files).
- Additional support directories named by function (for example, `Testing/Utilities`).

## Class-Level Coverage Requirements
Each code class in a code project must have a matching unit test class.

Rules:
- If a matching unit test class does not exist, create it.
- Validate the created test class by running tests.
- Keep the test class in the mirrored directory structure.

## Method and Property Coverage Requirements
Each method or property in a code class must have one or more matching unit test methods.

Rules:
- If a matching unit test method does not exist, create it.
- Unit tests must cover every logic path, including edge cases.
- Validate tests after creating or updating methods.

## Test Class and Method Conventions
All unit test classes must:
- Derive from `PilotApi.TestingShared.TestBase`.

Test methods must:
- Follow Arrange, Act, Assert pattern.
- Follow naming format:
  - `<Name_Of_Class_Under_Test>_<Name_Of_Method_Or_Property_To_Test>_<Expected_Result>_Test`

Variable rules in unit tests:
- Do not use class variables.
- Do not use underscore prefixes on variables.

## Validation and Error Correction
After creating or updating tests, verify all unit tests can run successfully.

Requirements:
- Run the relevant test projects.
- Correct discovered test-project errors.
- Do not alter production code classes to make tests pass.

## Appendix A: Additional Best Practices and Recommendations
These recommendations supplement required rules above.

1. Test design quality
- Keep each test focused on one behavior.
- Prefer one assertion theme per test.
- Use explicit input values to make intent obvious.
- Include happy-path, boundary, null/empty, invalid input, and exception-path tests where applicable.

2. Deterministic tests
- Avoid reliance on current time, random values, external state, network calls, and environment-specific behavior unless fully controlled by doubles or fixtures.
- Use stable test data and controlled setup.

3. Readability and maintainability
- Keep Arrange, Act, Assert sections visually clear.
- Use descriptive variable names without underscore prefixes.
- Avoid unnecessary mocking; prefer simple, behavior-focused tests.

4. Doubles and resources
- Place reusable doubles in `Testing/Doubles`.
- Place helper utilities in function-named folders such as `Testing/Utilities`.
- Keep test resources in `Testing/Resources` and ensure tests reference them predictably.

5. Coverage discipline
- Add tests for both positive and negative outcomes.
- Add regression tests when defects are found.
- Ensure new methods/properties are not left without matching tests.

6. File and class hygiene
- Keep one class per file in all test and support files.
- Keep namespace and folder organization aligned with code project structure.

7. Execution workflow recommendation
- Create or update tests incrementally per class.
- Run tests frequently during changes.
- Resolve failures immediately before moving to the next class.
