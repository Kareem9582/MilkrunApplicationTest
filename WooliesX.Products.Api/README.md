# WooliesX.Products API

Minimal API for products listing, details, creation and update, with JWT auth and an SSE endpoint for streaming products.

This repo contains four projects:
- `WooliesX.Products.Api` — ASP.NET Core minimal API (endpoints, middleware, DI)
- `WooliesX.Products.Application` — MediatR handlers, validation, mapping
- `WooliesX.Products.Domain` — Entities and contracts
- `WooliesX.Products.Infrastructure` — In‑memory JSON‑seeded repository
- `WooliesX.Products.Api.Tests` — Integration tests (xUnit)

## Prerequisites
- .NET SDK 10 (Preview) installed
- Trusted dev HTTPS cert: `dotnet dev-certs https --trust`

## Quick Start
1) Restore and build
- `dotnet restore`
- `dotnet build`

2) Configure local secrets (recommended)
- `dotnet user-secrets set "BasicAuth:Username" "test_user" --project WooliesX.Products.Api/WooliesX.Products.Api.csproj`
- `dotnet user-secrets set "BasicAuth:Password" "test_password" --project WooliesX.Products.Api/WooliesX.Products.Api.csproj`
- Optional JWT key: `dotnet user-secrets set "Jwt:Key" "<strong_random_key>" --project WooliesX.Products.Api/WooliesX.Products.Api.csproj`

3) Run the API
- `dotnet run --project WooliesX.Products.Api/WooliesX.Products.Api.csproj`
- Dev hot‑reload: `dotnet watch run --project WooliesX.Products.Api/WooliesX.Products.Api.csproj`

4) Run tests
- `dotnet test`

## Endpoints
- `GET /products` — List (filtering, sorting, paging)
- `GET /products/{id}` — Details
- `POST /products` — Create (JWT required)
- `PUT /products/{id}` — Update (JWT required)
- `GET /products/stream` — Server‑Sent Events stream (products one by one)
- `POST /auth/login` — Issue JWT using BasicAuth credentials

## Auth
- Obtain token: `POST /auth/login` with body `{ "username": "test_user", "password": "test_password" }`
- Protected endpoints send: `Authorization: Bearer <token>`

## Curl Examples
- List first page (10 items):
  - `curl -s "http://localhost:5000/products?page=1&pageSize=10"`
- Sorted by price desc:
  - `curl -s "http://localhost:5000/products?sortBy=price&order=desc&page=1&pageSize=5"`
- SSE (progressive stream; view in terminal):
  - `curl -N -H "Accept: text/event-stream" "http://localhost:5000/products/stream"`

## Postman
- Collection: `Files/ProductsDemoApplication.postman_collection.json`
- Variables: `baseUrl`, `BASIC_AUTH_USERNAME`, `BASIC_AUTH_PASSWORD`, `jwt`
- Login Tests script (added): captures `Token` from the login response for convenience.
  - Script lines:
    - `var response = pm.response.json();`
    - `pm.environment.set("Token", response.token)`

Tip: You can also set the collection Authorization to Bearer and bind the token variable (e.g., `{{jwt}}`).

## Configuration
- App settings: `WooliesX.Products.Api/appsettings*.json`
- Sample data: `WooliesX.Products.Api/Products.json`
- Secrets (User Secrets):
  - Windows: `%APPDATA%/Microsoft/UserSecrets/<UserSecretsId>/secrets.json`
  - macOS/Linux: `~/.microsoft/usersecrets/<UserSecretsId>/secrets.json`
  - Keys: `BasicAuth:Username`, `BasicAuth:Password`, `Jwt:Key`

## Streaming + Logging Notes
- SSE uses .NET’s typed `ServerSentEvents` and a local async iterator to yield items.
- Logging middleware is SSE‑aware and avoids buffering/reading the response for streaming requests to preserve progressive delivery.
- Swagger UI does not render SSE; use curl or a simple HTML page with `EventSource`.

## Project Structure
- Endpoints: `WooliesX.Products.Api/Endpoints/Features/*`
- Middleware: `WooliesX.Products.Api/Middleware/`
- Extensions: `WooliesX.Products.Api/Extensions/`
- Application handlers/mapping: `WooliesX.Products.Application/Features/*`

## Build/Format
- Restore/build/test: `dotnet restore`, `dotnet build`, `dotnet test`
- Optional format (if configured): `dotnet format`

