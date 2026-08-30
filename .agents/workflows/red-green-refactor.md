---
description: Test-Driven Development (TDD) is a technique for building software that guides software development by writing tests. In essence, follow three simple steps repeatedly.
---

When building a new feature, follow the red-green-refactor cycle:
- Write a test for the next bit of functionality you want to add.
- Write the functional code until the test passes.
- Refactor both new and old code to make it well structured.

Keep in mind Robert C. Martin's Three Laws of Test-Driven Development:
- You are not allowed to write any production code unless it is to make a failing unit test pass.
- You are not allowed to write any more of a unit test than is sufficient to fail; and compilation failures are failures.
- You are not allowed to write any more production code than is sufficient to pass the one failing unit test.