# Testing CampusLibraryApi

This document describes the test strategy used in the CampusLibraryApi teaching project.

The goal is not only to verify correctness, but also to show how different test levels fit together in a Clean Architecture / DDD-style Web API.

## Current status

The current test suite contains 63 tests.

```text
dotnet test

Test summary: total: 63, failed: 0, succeeded: 63, skipped: 0
```

## Test levels

The test project is structured