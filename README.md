# Mark Smith — Portfolio

A personal portfolio and playground built with **Blazor Server** on **.NET 10**, deployed to Azure. It's part CV, part side-project showcase — live API integrations, a dark/light mode toggle, and a handful of genuinely useful dashboards rather than lorem-ipsum filler content.

**Live:** [marksmith-portfolio.azurewebsites.net](https://marksmith-portfolio.azurewebsites.net)

---

## Pages

| Page | What it does |
|---|---|
| **Home** | Hero intro, quick stats, links to the rest |
| **Projects** | GitHub repos pulled live from the API + side-project cards |
| **Skills** | Work history timeline, education, tech badges |
| **Contact** | Email form (SMTP via Gmail app password) |
| **NASA APOD** | Astronomy Picture of the Day — date picker + random |
| **SpaceX** | Upcoming & past launches with mission patch images |
| **Weather** | Current conditions for a few cities, °C/°F toggle |
| **F1 Dashboard** | Driver standings, constructor standings, last race results, calendar — season selector |

---

## Tech Stack

- **Blazor Server** — .NET 10, C#
- **Bootstrap 5** + custom CSS design system (CSS variables, dark mode)
- **Font Awesome** icons
- **IMemoryCache** — thread-safe, TTL-based caching across all services
- **DotNetEnv** — `.env` file for local secrets (API keys)
- **Azure App Service** — hosting
- **GitHub Actions** — CI/CD (build + deploy on push to `master`)

### External APIs used

| API | Auth | Used for |
|---|---|---|
| GitHub REST API | None (public) | Projects page repos |
| NASA APOD | API key (env var) | NASA page |
| SpaceX API (r/SpaceX) | None | SpaceX page |
| Jolpica / Ergast F1 | None | F1 Dashboard |
| Weather API | API key (env var) | Weather page |

---

## Running Locally

**Prerequisites:** .NET 10 SDK

```bash
git clone https://github.com/marksmith1994/MyPortfolio.git
cd MyPortfolio
```

Create a `.env` file in the project root:

```
NASA_API_KEY=your_nasa_key
WEATHER_API_KEY=your_weather_key
GMAIL_APP_PASSWORD=your_gmail_app_password
GMAIL_FROM_ADDRESS=you@gmail.com
```

Then run:

```bash
dotnet run
```

NASA and Weather API keys are free — get them at [api.nasa.gov](https://api.nasa.gov) and [weatherapi.com](https://www.weatherapi.com).

---

## Project Structure

```
MyPortfolio/
├── Components/
│   ├── Layout/          # MainLayout, NavMenu
│   ├── Pages/           # One .razor file per page
│   └── Shared/          # Timeline, LoadingSkeleton, etc.
├── Services/            # One service per API (models co-located)
├── wwwroot/
│   ├── css/             # base.css + per-page CSS files
│   ├── js/              # theme.js (dark/light toggle)
│   └── data/            # skills-data.json
├── plan/
│   └── TODO.md          # Ongoing improvement backlog
└── .github/workflows/   # GitHub Actions CI/CD
```

---

## Deployment

Every push to `master` triggers the GitHub Actions workflow:
1. Build with `dotnet build --configuration Release`
2. Publish with `dotnet publish`
3. Deploy to **Azure App Service** (`marksmith-portfolio`) via OIDC (no stored credentials)
