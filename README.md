# TimeIsOnMySide

A slot-booking platform where customers book 30-minute consultation sessions with advisors at branch locations. Staff can view their assigned bookings. Admins manage locations and staff.

---

## Quick Start

> **Requires:** Docker Desktop (or Docker Engine + Compose plugin). No other tools needed.

```bash
./start-all
```

That's it. The script will:
1. Load the pre-built image (or build from source if not present)
2. Start the app + mail catcher containers
3. Wait for the app to be healthy
4. Open three browser tabs automatically

| Tab opened | URL | Description |
|-----------|-----|-------------|
| Booking page | http://localhost:8080 | Customer booking view |
| Admin login | http://localhost:8080/login | Admin / staff login |
| Mail UI | http://localhost:8025 | Mailpit — all outbound emails |

### Stop / reset

```bash
docker compose down        # stop (data preserved)
docker compose down -v     # stop and wipe all data
```

---

## Sharing this demo

To share the demo with someone who has no developer tools:

**Developer (you) — run once:**
```bash
./build-image              # builds image and saves timeisonymyside.tar.gz
zip -r demo.zip . -x '*.git*' -x 'node_modules/*'
# share demo.zip
```

**Recipient:**
```bash
unzip demo.zip
./start-all                # loads the image, starts everything, opens browser
```

The recipient needs only Docker Desktop — no Node.js, .NET SDK, or internet access required.

---

## Developer workflow

```bash
# Build image from source (requires internet)
docker compose build

# Start with live logs
docker compose up

# Start detached
docker compose up -d
```

---

## Credentials

| Role | Username | Password | Notes |
|------|----------|----------|-------|
| Admin | `admin` | `Admin1234!` | Pre-seeded on first boot |
| Staff | *(shown after creation)* | *(set when creating)* | e.g. `jane0001` |
| Client | — | — | No login required to book |

---

## End-to-End Walkthrough

Follow these flows in order to exercise every role and feature.

### Step 1 — Admin: Login

```mermaid
flowchart TD
    A([Open http://localhost:8080]) --> B[Click Login in the header]
    B --> C[Enter username: admin]
    C --> D["Enter password: Admin1234!"]
    D --> E[Click Sign In]
    E --> F[Redirected to Admin Dashboard → Locations page]
```

### Step 2 — Admin: Create a location

```mermaid
flowchart TD
    F[Admin Dashboard] --> G[Click + Add Location]
    G --> H[Fill in Name e.g. City Branch]
    H --> I[Fill in Address e.g. 1 Main St]
    I --> J["Enable Monday → Friday\nSet opening hours e.g. 09:00 – 17:00 each day"]
    J --> K[Click Create location]
    K --> L["Success screen shows the location slug\ne.g. city-branch"]
    L --> M[Click Back to Dashboard]
    M --> N[Location card is now visible on the dashboard]
```

> **Note:** Slots only appear if the location has opening hours set **and** at least one staff member whose shift covers the time.

### Step 3 — Admin: Add a staff member

```mermaid
flowchart TD
    N[Admin Dashboard — location card visible] --> O[Click + Add User on the location card]
    O --> P["Fill in First name e.g. Jane\nFill in Last name e.g. Doe"]
    P --> Q["Set Password e.g. MyPass1!\nConfirm password"]
    Q --> R["Tick the working days e.g. Monday – Friday\nAdjust shift times if needed"]
    R --> S[Click Create user]
    S --> T["Modal shows the generated username\ne.g. jane0001\n⚠ Note this down — it cannot be changed"]
    T --> U[Click Done]
    U --> V[Click user menu top-right → Logout]
```

### Step 4 — Client: Book a slot

```mermaid
flowchart TD
    W([Open http://localhost:8080]) --> X[Select your location from the dropdown]
    X --> Y[Pick a weekday date on the date strip]
    Y --> Z["Click a green available slot\ne.g. 09:00"]
    Z --> AA[Booking form opens]
    AA --> AB["Fill in Name, Email, Phone\nAdd optional Notes"]
    AB --> AC[Click Book / Confirm]
    AC --> AD["Confirmation screen appears\nNote the booking reference e.g. bk-ab12cd34"]
```

> **Note:** Past slots (before the current time on today's date) are shown as disabled.

### Step 5 — Staff: View bookings

```mermaid
flowchart TD
    AE([Open http://localhost:8080]) --> AF[Click Login in the header]
    AF --> AG["Enter the staff username e.g. jane0001\nEnter the password you set"]
    AG --> AH[Click Sign In]
    AH --> AI[Redirected to My Bookings dashboard]
    AI --> AJ[Future tab shows upcoming bookings including the one just made]
    AJ --> AK[Click Past tab to see historical bookings]
```

---

## Architecture

### Container layout

`docker compose up` starts two containers:

```
┌─────────────────────────────────────────────────────────────────────┐
│  Docker Compose                                                     │
│                                                                     │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │  app container (port 8080)                                   │  │
│  │                                                              │  │
│  │  ┌──────────┐   proxy /api   ┌──────────────────────────┐   │  │
│  │  │  nginx   │ ─────────────▶ │  .NET 8 Backend          │   │  │
│  │  │  :8080   │                │  ASP.NET Core :5005       │   │  │
│  │  │  SPA +   │                │  EF Core 8 + MailKit      │   │  │
│  │  │  proxy   │                └──────┬────────────────────┘   │  │
│  │  └──────────┘                       │ DB          │ SMTP      │  │
│  │                                     ▼             ▼           │  │
│  │                        ┌──────────────────┐       │           │  │
│  │                        │  PostgreSQL 16   │       │           │  │
│  │                        │  :5432           │       │           │  │
│  │                        └──────────────────┘       │           │  │
│  └───────────────────────────────────────────────────┼───────────┘  │
│                                                       │              │
│  ┌────────────────────────────────────────────────────▼──────────┐  │
│  │  mailpit container                                             │  │
│  │  SMTP :1025 (internal)   Web UI :8025 ──────▶ host browser    │  │
│  └────────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────┘
```

### Startup sequence

s6-overlay enforces the dependency chain — each service waits for the previous one before starting:

```mermaid
sequenceDiagram
    participant s6 as s6-overlay
    participant PG as PostgreSQL :5432
    participant BE as .NET Backend :5005
    participant NG as nginx :8080

    s6->>PG: start (initdb on first boot, create DB + user)
    PG-->>s6: accepting connections

    s6->>BE: start
    BE->>PG: wait for pg_ready
    BE->>PG: dotnet migrate + seed admin
    BE-->>s6: listening on :5005

    s6->>NG: start
    NG->>BE: wait for GET /health → 200
    NG-->>s6: serving on :8080
```

### Full request flow

```mermaid
sequenceDiagram
    actor Browser
    participant Nginx
    participant Backend as .NET Backend :5005
    participant DB as PostgreSQL :5432

    Browser->>Nginx: GET / (SPA)
    Nginx-->>Browser: index.html + assets

    Browser->>Nginx: GET /api/locations
    Nginx->>Backend: proxy → GET /api/locations
    Backend->>DB: SELECT locations
    DB-->>Backend: rows
    Backend-->>Nginx: { locations: [] }
    Nginx-->>Browser: { locations: [] }

    Browser->>Nginx: POST /api/auth/login
    Nginx->>Backend: proxy → POST /api/auth/login
    Backend->>DB: SELECT users WHERE username=...
    DB-->>Backend: user row
    Backend-->>Browser: { role, token, userId }

    Browser->>Nginx: POST /api/bookings
    Nginx->>Backend: proxy → POST /api/bookings
    Backend->>DB: find staff, insert booking
    DB-->>Backend: saved
    Backend-->>Browser: { bookingRef, date, startTime }
```

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Frontend | Vue 3 · Vite · Pinia · Vue Router · Zod · TypeScript |
| Backend | ASP.NET Core 8 · EF Core 8 · FluentValidation |
| Database | PostgreSQL 16 |
| Auth | Stateless HMAC daily token (`X-Admin-Token` / `X-Staff-Token`) |
| Container | s6-overlay · nginx · .NET 8 runtime |

---

## API Reference

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| `POST` | `/api/auth/login` | — | Login; returns role + token |
| `GET` | `/api/locations` | — | List all locations |
| `POST` | `/api/locations` | Admin | Create a location |
| `GET` | `/api/locations/{id}` | Admin | Get a single location |
| `GET` | `/api/locations/{id}/users` | Admin | List staff for a location |
| `POST` | `/api/users` | Admin | Create a staff user |
| `GET` | `/api/slots?date&locationId` | — | Get 30-min slots for a date + location |
| `POST` | `/api/bookings` | Rate-limited | Create a booking |
| `GET` | `/api/bookings` | Staff | Get bookings for the logged-in staff member |
| `GET` | `/health` | — | Liveness probe |

### Slot availability rules

A slot is **available** when:
- The location has opening hours for that day
- The slot time falls within those opening hours
- At least one staff member's shift covers the slot
- Not all covering staff are already booked for that slot
- The slot's start time has not yet passed (today only — past slots are `unavailable`)

---

## Local Development

### Prerequisites

| Tool | Version |
|------|---------|
| Node.js | ≥ 20.19 or ≥ 22.12 |
| .NET SDK | 8.0 |
| PostgreSQL | 14+ |

### Backend

```bash
# 1. Create the database
psql -U postgres -c "CREATE USER db_app_user WITH PASSWORD 'your-password';"
psql -U postgres -c "CREATE DATABASE overtime OWNER db_app_user;"

# 2. Add connection string + email config (gitignored)
cat > backend/overtime/appsettings.local.json <<'EOF'
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=overtime;Username=db_app_user;Password=your-password"
  },
  "Email": {
    "SmtpHost": "localhost",
    "SmtpPort": 1025,
    "FromAddress": "noreply@timeisonymyside.local",
    "FromName": "TimeIsOnMySide",
    "DemoRecipientEmail": "dev@example.com"
  }
}
EOF

# Optional: run Mailpit locally to catch outbound emails (http://localhost:8025)
docker run -d -p 1025:1025 -p 8025:8025 axllent/mailpit

# 3. Migrate + seed admin
dotnet run --project backend/overtime -- -m

# 4. Start API (http://localhost:5005, Swagger at /swagger)
dotnet run --project backend/overtime --launch-profile run
```

### Frontend

```bash
cd frontend
npm install
npm run dev   # http://localhost:5173
```

### Tests

```bash
# Backend (unit + integration)
dotnet test backend/

# Frontend unit tests
cd frontend && npm run test:unit

# Frontend E2E against local dev server
cd frontend && npm run test:e2e

# Frontend E2E against a running Docker container
docker run -p 8080:8080 --name tims timeisonymyside
cd frontend && DOCKER_BASE_URL=http://localhost:8080 npx playwright test --project=docker
```
