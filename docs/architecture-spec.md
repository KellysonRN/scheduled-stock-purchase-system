# Architecture Specification — Scheduled Stock Purchase System

This document describes the current architecture and design decisions for the repository after the recent refactor to a Vertical Slice style.

## Goals
- High feature cohesion and minimal coupling
- Clear separation between Domain (business rules), Features (application slices), Infrastructure (technical concerns), and Shared primitives

## High-level overview
- API: Minimal API project (`src/Scheduled.Stock.Purchase.Api`) organized with vertical slices under `Features/`.
- Domain: Centralized domain model (`src/Scheduled.Stock.Purchase.Domain`) containing entities and value objects.
- Shared: Lightweight primitives (`src/Scheduled.Stock.Purchase.Shared`) used across layers (e.g. `Result`, `Error`).
- Infrastructure: Persistence, messaging and external integrations (`src/Scheduled.Stock.Purchase.Infrastructure`).

## Vertical Slice pattern (how to add a feature)

Place each use case under `src/Scheduled.Stock.Purchase.Api/Features/{Aggregate}/{UseCase}/` with the following files:

- `Endpoint.cs` — minimal API endpoint mapping (implements `IApiEndpoint`).
- `Request.cs` / `Response.cs` — DTOs for input and output.
- `Handler.cs` — application handler implementing `IHandler<TRequest, Result<TResponse>>`.
- `Validator.cs` — FluentValidation rules (if needed).

The API automatically registers endpoints and handlers at startup via reflection:

- `ServiceCollectionExtensions.RegisterApiEndpointsFromAssembly(...)`
- `ServiceCollectionExtensions.AddHandlersFromAssembly(...)`

## Domain placement guidance

- Keep entities, aggregates and value objects inside `src/Scheduled.Stock.Purchase.Domain`.
- Only move an object into a feature when it belongs exclusively to a single bounded context and will not be reused.

## Shared primitives

- `Result` and `Result<T>` live in `src/Scheduled.Stock.Purchase.Shared` and provide ergonomic success/failure handling and implicit conversions.
- `Error` is a simple record containing a `Code` and `Message` and serves as the canonical error representation.

## HTTP mapping

- The API contains small HTTP helpers (`Api.Extensions.ResultExtensions`) which map `Result<T>` to `IResult` responses. Keep the mapping logic small and deterministic.

## Build & test

Run the solution build and tests with:

```bash
dotnet restore
dotnet build
dotnet test
```

## Recommended next steps

- Add README for `Shared` with usage examples (Result/Errors).
- Add automated CI to run build and tests.
- Add more feature slices under `Features/` to cover primary domain workflows (Contributions, Purchases, Distribution).

---

This spec is a living document — update when design decisions change.
