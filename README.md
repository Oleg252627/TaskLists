# TaskLists

TaskLists is an ASP.NET Core API for managing task lists with a layered split into `Api / Application / Infrastructure / Domain`.

## Implemented features

- CRUD for task lists
- retrieving accessible task lists with creation-time sorting and pagination
- sharing a task list with users
- retrieving and removing share links
- access rules:
  - a list is visible to the owner or a shared user
  - reading, updating, and sharing are allowed for the owner or a shared user
  - deleting a list is allowed only for the owner
  - one list can be shared with at most 3 users
- DTO validation with FluentValidation
- exception handling through a single handler
- logging with Serilog
- Swagger / OpenAPI
- unit tests and integration tests

## Architecture

- **TaskLists.Api** — HTTP endpoints, Swagger, middleware, current user header
- **TaskLists.Application** — business logic, DTOs, validation, services
- **TaskLists.Infrastructure** — PostgreSQL + EF Core + repositories
- **TaskLists.Domain** — entities and domain rules
- **tests/** — unit and integration tests

## Requirements

- .NET SDK 10
- Docker Desktop
- PostgreSQL (if you do not run through `docker compose`)

## How to run locally

### 1. With Docker Compose

```bash
docker compose up -d --build
```

The API will be available at:

- `http://localhost:8080`

PostgreSQL:

- `localhost:5432`

### 2. Without Docker

First start PostgreSQL and set the connection string:

```bash
export ConnectionStrings__Database="Host=localhost;Port=5432;Database=TaskLists;Username=postgres;Password=postgres"
dotnet run --project src/TaskLists.Api/TaskLists.Api.csproj
```

## How to run tests

This solution contains:

- unit tests (`TaskLists.Application.Tests`)
- integration tests (`TaskLists.Api.Tests`)

Integration tests use **Testcontainers** and start a real **PostgreSQL container in Docker** for the test run, then stop and remove it automatically.

Before running integration tests, make sure Docker Desktop is running.

```bash
dotnet test TaskLists.sln
```

Run only integration tests:

```bash
dotnet test tests/TaskLists.Api.Tests/TaskLists.Api.Tests.csproj
```

## Swagger

After the API is running, Swagger is available at:

```text
/swagger/index.html
/swagger
```

## Required header

All business requests require the header:

```text
X-User-Id: <guid>
```

Without it, the request will return a validation error.

## Useful endpoints

- `POST /task-lists`
- `PUT /task-lists/{id}`
- `DELETE /task-lists/{id}`
- `GET /task-lists/{id}`
- `GET /task-lists?page=1&pageSize=20`
- `POST /task-lists/{id}/shares/{targetUserId}`
- `GET /task-lists/{id}/shares`
- `DELETE /task-lists/{id}/shares/{targetUserId}`
