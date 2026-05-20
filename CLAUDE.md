# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Run locally (hot-reload)
dotnet run

# Build (release)
dotnet build --configuration Release

# Publish
dotnet publish -c Release -o ./publish
```

There are no automated tests in this project. `TreatWarningsAsErrors` is set to `true` in the `.csproj`, so build warnings become errors — keep the build clean.

## Local secrets

API keys are loaded from a `.env` file in the project root via `DotNetEnv`. The `.env` values are mapped to `IConfiguration` using these keys (see `appsettings.json` for the structure):

| Env var | Config key | Used by |
|---|---|---|
| `NASA_API_KEY` | `Nasa:ApiKey` | `NasaService` |
| `WEATHER_API_KEY` | `Weather:ApiKey` | `WeatherService` |
| `GMAIL_APP_PASSWORD` | `Email:GmailPassword` | `EmailService` |
| `GMAIL_FROM_ADDRESS` | `Email:GmailAddress` | `EmailService` |

Without these, NASA and Weather pages silently return no data; the Contact form throws.

## Architecture

**Blazor Server** on .NET 10. Each HTTP request keeps a persistent SignalR connection; UI updates are server-side.

### Service pattern

Every external API has one service class in `Services/`. The pattern is:

1. Check `IMemoryCache` with a typed cache key (e.g. `"f1:drivers:{year}"`).
2. Fetch via `HttpClient` (typed client, registered in `Program.cs`).
3. Cache the result with a TTL (current-season data: 1 h; historical data: 30 days; GitHub repos: 24 h; weather: 1 h).
4. On any exception, log via `ILogger` and return an empty list or `null` — pages handle missing data gracefully.

Response models are co-located with their service file (e.g. `F1Service.cs` contains all F1 model classes). The only standalone models are `ContactModel` and `GitHubRepository` in `Models/`.

Services are registered in `Program.cs` as both a typed `HttpClient` **and** a scoped service — this is intentional for named-client resolution.

### CSS architecture

`wwwroot/css/base.css` defines all CSS custom properties (variables) for the design system and the `[data-theme="dark"]` overrides. Each page loads its own CSS file at the **bottom** of the `.razor` file via `<link href="css/foo.css" rel="stylesheet">` — this is the established pattern, not a bug.

### Dark/light theme

Implemented entirely in JS (`wwwroot/js/theme.js`). Two globals — `applyStoredTheme()` and `toggleTheme()` — toggle `data-theme` on `<html>` and persist to `localStorage`. `MainLayout.razor` calls these via `IJSRuntime` on first render and on toggle click.

### Skills data

`wwwroot/data/skills-data.json` is a static JSON file that drives the Skills page (work history, education, tech badges). Edit this file to update CV content — no C# changes required.

### Deployment

Pushing to `master` triggers `.github/workflows/master_marksmith-portfolio.yml`, which builds → publishes → deploys to Azure App Service (`marksmith-portfolio`) via OIDC (no stored credentials). Secrets are stored as GitHub Actions secrets.
