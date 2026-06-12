# TimeIsOnMySide — Frontend

A single-page appointment booking application that allows customers to select a branch location, pick a date, choose a 30-minute consultation slot, and confirm their booking with contact details.

**Stack:** Vue 3 · TypeScript 6 · Pinia · Vue Router 5 · Zod 4 · Vite 8 · Vitest 4 · Playwright

---

## Architecture

The frontend is structured in strict layers with no circular dependencies:

```
src/
├── api/
│   ├── schemas.ts        Zod schemas + inferred TypeScript types (Location, Slot, BookingRequest, BookingResponse)
│   └── booking.ts        Mock API functions (fetchLocations, fetchSlots, createBooking) — swap-ready for real HTTP
├── stores/
│   └── bookingStore.ts   Single Pinia store: locations, slots, selected slot/location, async actions
├── composables/
│   ├── useSlots.ts        Date window logic — selectedDate, weekDates, slotsForDate (filtered from store)
│   └── useBookingForm.ts  Form state, Zod validation, submit action wired to the store
├── components/
│   ├── AppHeader.vue      Static branding header
│   ├── LocationPicker.vue Pill button row for branch selection
│   ├── DateStrip.vue      7-day scrollable date picker
│   ├── SlotGrid.vue       Grid of SlotCard components filtered to the selected date
│   ├── SlotCard.vue       Individual time slot — available/unavailable states, emits select
│   └── BookingModal.vue   Overlay form for name/email/phone/notes; shows success or error state
├── views/
│   └── BookingView.vue    Orchestrates the full booking flow; owns event wiring between components
└── router/
    └── index.ts           Single route: / → BookingView
```

### Key design decisions

- **Zod at the API boundary.** All data entering the app is parsed through Zod schemas (`schemas.ts`). A shape mismatch throws immediately rather than silently propagating bad data.
- **Swap-ready API layer.** The mock (`api/booking.ts`) exports the same function signatures the store depends on. Replacing the mock with a real HTTP client requires only changing one import in the store.
- **Composable separation.** Date logic (`useSlots`) and form logic (`useBookingForm`) are isolated from both the store and the view, making each unit independently testable.
- **CSS custom properties.** All colours and radii are defined as CSS variables in `assets/base.css`, making theming straightforward without a CSS-in-JS dependency.

---

## Prerequisites

| Requirement | Version |
|-------------|---------|
| Node.js | `^20.19.0` or `>=22.12.0` |
| npm | bundled with Node |

---

## Quick start

```bash
cd frontend
npm install
npm run dev
```

Open [http://localhost:5173](http://localhost:5173).

---

## Available scripts

| Script | Description |
|--------|-------------|
| `npm run dev` | Start the Vite dev server with HMR |
| `npm run build` | Type-check then produce a production build in `dist/` |
| `npm run preview` | Serve the production build locally |
| `npm run test:unit` | Run all Vitest unit tests |
| `npm run test:e2e` | Run Playwright end-to-end tests (requires a running dev or preview server) |
| `npm run type-check` | Run `vue-tsc --build` without emitting output |
| `npm run lint` | Run oxlint then ESLint with auto-fix |
| `npm run format` | Run Prettier over `src/` |

---

## Running tests

### Unit tests (Vitest)

```bash
npm run test:unit
```

Tests are co-located with source under `__tests__/` directories. Coverage spans:
- Zod schema validation (`api/__tests__/schemas.spec.ts`)
- Mock API behaviour including pre-seeded unavailable slots and double-booking rejection (`api/__tests__/booking.spec.ts`)
- Pinia store actions and state transitions (`stores/__tests__/bookingStore.spec.ts`)
- Composable logic for date filtering and form validation (`composables/__tests__/`)
- All presentational components and the booking view (`components/__tests__/`, `views/__tests__/`)

### End-to-end tests (Playwright)

E2e tests require the app to be running. In CI the preview server is used; locally the dev server is reused if already running.

```bash
# Terminal 1
npm run dev

# Terminal 2
npm run test:e2e
```

The Playwright suite targets Chromium, Firefox, and WebKit. Test files live in `e2e/`.

### Type checking

```bash
npm run type-check
```

Uses `vue-tsc` in project-references mode. `noUncheckedIndexedAccess` is enabled, so array/object lookups require null guards.

---

## Project structure (annotated)

```
frontend/
├── src/                  Application source (see Architecture above)
├── e2e/                  Playwright end-to-end tests
├── public/               Static assets served at /
├── index.html            HTML entry point
├── vite.config.ts        Vite + Vue plugin configuration; `@` path alias → src/
├── tsconfig.json         Project-references root; delegates to app/node/vitest configs
├── tsconfig.app.json     DOM compilation settings, noUncheckedIndexedAccess, @ alias
├── tsconfig.vitest.json  Vitest-specific TS settings
├── vitest.config.ts      Vitest configuration
├── playwright.config.ts  Playwright cross-browser configuration
└── eslint.config.ts      ESLint + oxlint + Prettier integration
```

---

## API contract

The frontend consumes three functions from `src/api/booking.ts`:

```typescript
fetchLocations(): Promise<Location[]>
fetchSlots(date: string, locationId: string): Promise<Slot[]>
createBooking(request: BookingRequest): Promise<BookingResponse>
```

The current implementation is a fully in-memory mock that simulates realistic behaviour:
- Returns three locations (A, B, C)
- Returns 16 slots per weekday (09:00–16:30, 30-minute intervals); returns `[]` on weekends
- Pre-seeds one slot per location as unavailable to demonstrate the unavailable state
- Persists bookings in a module-level `Map` for the session lifetime

All mock data is parsed through the same Zod schemas used in production, so a schema mismatch will surface as a test failure rather than a silent runtime bug.

**Swapping to the real backend** — see `../backend.md` for the full HTTP API contract. When the backend is ready, add `src/api/bookingHttp.ts` wrapping `fetch` calls and swap the import in `src/stores/bookingStore.ts`:

```typescript
// Before
import { fetchSlots, createBooking, fetchLocations } from '@/api/booking'

// After
import { fetchSlots, createBooking, fetchLocations } from '@/api/bookingHttp'
```

---

## Building for production

```bash
npm run build
```

Output is written to `dist/`. The build is a standard static SPA — serve `dist/` from any web server or CDN. Example with Node:

```bash
npm run preview   # serves dist/ on http://localhost:4173
```

For containerised deployment, the static build artefact from `dist/` is served alongside the backend. The Dockerfile belongs with the full-stack project; see the backend documentation for the combined Docker setup.

---

## Known limitations and next steps

- **Mock API only.** All data is in-memory and resets on page reload. The backend HTTP API is specified in `../backend.md` and ready to be implemented.
- **No authentication.** The booking flow is intentionally public (v1 requirement). Auth would be added at the router guard level.
- **Single timezone.** Dates and times are rendered in the browser's local timezone. The backend will own timezone normalisation (store UTC, display local).
- **No pagination or search.** The location list is hardcoded to three entries. A real implementation would fetch from the API.
- **No Dockerfile for the frontend alone.** The frontend builds to static files and is best served as part of a combined Docker image with the backend; see backend documentation.
