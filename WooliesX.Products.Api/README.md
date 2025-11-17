# WooliesX.Products API

Minimal API for products (list, details, create, update) with JWT auth and Server‑Sent Events (SSE) streaming. Built with CQRS and MediatR.

## Tech Stack
- ASP.NET Core Minimal APIs (routing, DI, TypedResults)
- CQRS + MediatR (commands/queries + streaming)
- AutoMapper (entity → DTO mapping)
- FluentValidation (request validation)
- Serilog (structured logging)
- JWT Bearer Authentication
- SSE (System.Net.ServerSentEvents)

## Architecture & Patterns
- Layers:
  - `WooliesX.Products.Api` — endpoints, middleware, DI
  - `WooliesX.Products.Application` — CQRS handlers, validation, mapping
  - `WooliesX.Products.Infrastructure` — repository (JSON‑seeded in‑memory)
  - `WooliesX.Products.Domain` — entities and contracts
  - `WooliesX.Products.Api.Tests` — integration tests (xUnit)
- CQRS:
  - Queries: `GetProducts`, `GetProductById`, `GetAllProducts`
  - Commands: `CreateProduct`, `UpdateProduct`
  - Streaming: MediatR `IStreamRequest<T>` pattern (for product streams)
- Mapping: `ProductMappingProfile` centralizes DTO maps (queries + command responses)
- Validation: FluentValidation validators per command/query
- Middleware: correlation ID, exception handling, logging (SSE‑aware)

## NuGet Packages
- `Microsoft.AspNetCore.OpenApi (10.0.0)` — OpenAPI
- `Microsoft.AspNetCore.Authentication.JwtBearer (8.0.7)` — JWT auth
- `MediatR (12.2.0)` — CQRS + streaming
- `AutoMapper.Extensions.Microsoft.DependencyInjection (12.0.1)` — AutoMapper DI
- `FluentValidation (11.9.2)` + `FluentValidation.DependencyInjectionExtensions (11.9.2)` — validation
- `Serilog.AspNetCore (8.0.2)` + `Serilog.Sinks.File (6.0.0)` + `Serilog.Settings.Configuration (8.0.2)` — logging

## .NET 10 Features Used
- Target Framework: `net10.0` — builds on ASP.NET Core 10 packages
- Minimal API typed patterns with `TypedResults` across endpoints
- Primary constructors used in middleware classes (modern C# features)
- C# `field` keyword in property accessors (C# 13) for clear backing‑field assignment semantics
- C# Extension Members (preview) to enrich types via extensions in `Extensions/*` for a cleaner minimal‑API composition

## .NET 9 Features Used
- Server‑Sent Events built‑in primitives: `System.Net.ServerSentEvents` + `TypedResults.ServerSentEvents(...)`
- Minimal APIs async streaming with `IAsyncEnumerable<T>` in route handlers

## Quick Start
- `dotnet restore`
- `dotnet build`
- Configure secrets (local dev):
  - `dotnet user-secrets set "BasicAuth:Username" "test_user" --project WooliesX.Products.Api/WooliesX.Products.Api.csproj`
  - `dotnet user-secrets set "BasicAuth:Password" "test_password" --project WooliesX.Products.Api/WooliesX.Products.Api.csproj`
  - Optional: `dotnet user-secrets set "Jwt:Key" "<strong_random_key>" --project WooliesX.Products.Api/WooliesX.Products.Api.csproj`
- Run: `dotnet run --project WooliesX.Products.Api/WooliesX.Products.Api.csproj`
- Tests: `dotnet test`

## Endpoints
- `GET /products` — list (filter/sort/page)
- `GET /products/{id}` — details
- `POST /products` — create (JWT required)
- `PUT /products/{id}` — update (JWT required)
- `GET /products/stream` — SSE stream (one product per event)
- `POST /auth/login` — issue JWT (from BasicAuth credentials)

## Curl
- List: `curl -s "http://localhost:5000/products?page=1&pageSize=10"`
- Sort: `curl -s "http://localhost:5000/products?sortBy=price&order=desc&page=1&pageSize=5"`
- SSE: `curl -N -H "Accept: text/event-stream" "http://localhost:5000/products/stream"`

## Postman
- Collection: `Files/ProductsDemoApplication.postman_collection.json`
- Login Tests script captures an environment token variable:
  - `var response = pm.response.json();`
  - `pm.environment.set("Token", response.token)`

## Configuration
- App settings: `WooliesX.Products.Api/appsettings*.json`
- Sample data: `WooliesX.Products.Api/Products.json`
- User secrets: `BasicAuth:Username`, `BasicAuth:Password`, `Jwt:Key`

## Streaming + Logging
- SSE endpoint uses typed `ServerSentEvents` with an async iterator for progressive delivery.
- Logging middleware is SSE‑aware and skips response buffering/logging for streaming requests.
