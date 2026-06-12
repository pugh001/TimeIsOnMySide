# Frontend Code Review — Fixes

Reviewed against: `frontend/src/`  
Severity scale: **Critical** (blocks production use) → **Moderate** (correctness/UX bug) → **Minor** (quality/maintainability)

---

## Critical

### 1. Mock API is the only implementation — no real HTTP client

**File:** `src/api/booking.ts`

All three exported functions (`fetchLocations`, `fetchSlots`, `createBooking`) are in-memory stubs. There is no `fetch()` call to the backend. The frontend cannot connect to a real server as-is.

**Fix:**

Replace the three functions with real HTTP calls and move the mock to a separate file for tests:

```ts
// src/api/booking.ts  (real implementation)
const BASE = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5005'

export async function fetchLocations(): Promise<Location[]> {
  const res = await fetch(`${BASE}/api/locations`)
  if (!res.ok) throw new Error(`fetchLocations failed: ${res.status}`)
  const data = await res.json()
  return z.object({ locations: z.array(LocationSchema) }).parse(data).locations
}

export async function fetchSlots(date: string, locationId: string): Promise<Slot[]> {
  const res = await fetch(`${BASE}/api/slots?date=${date}&locationId=${locationId}`)
  if (!res.ok) throw new Error(`fetchSlots failed: ${res.status}`)
  const data = await res.json()
  return z.object({ slots: z.array(SlotSchema) }).parse(data).slots
}

export async function createBooking(request: BookingRequest): Promise<BookingResponse> {
  const res = await fetch(`${BASE}/api/bookings`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(BookingRequestSchema.parse(request)),
  })
  if (res.status === 409) throw new Error('This slot is no longer available.')
  if (!res.ok) throw new Error(`createBooking failed: ${res.status}`)
  return BookingResponseSchema.parse(await res.json())
}
```

Move the current stub content to `src/api/booking.mock.ts` and import it in tests.

Add to `vite.config.ts`:

```ts
server: {
  proxy: {
    '/api': 'http://localhost:5005',
  },
}
```

---

### 2. `useSlots` state is re-created on every call — not a singleton

**File:** `src/composables/useSlots.ts`

`selectedDate` and `today` are declared as local `ref`s inside the composable function body. Each component that calls `useSlots()` gets its own independent reactive state. `BookingView` and `SlotGrid` are currently coupled via a `watch(() => props.date, setDate)` workaround in `SlotGrid`, which is fragile — it works today but will silently break if the prop name changes or if a third consumer is added.

**Fix — Option A (preferred):** Lift `selectedDate` into `bookingStore`:

```ts
// in bookingStore.ts
const selectedDate = ref(todayIso())
function setDate(date: string) { selectedDate.value = date }
```

Then `useSlots` just reads from the store and no prop-passing is needed in `SlotGrid`.

**Fix — Option B:** Declare refs at module level outside the function:

```ts
const selectedDate = ref(todayIso())
const today = ref(todayIso())

export function useSlots() {
  // selectedDate and today are now shared across all callers
  ...
}
```

---

### 3. Single `status` ref shared across slot loading and booking submission

**File:** `src/stores/bookingStore.ts`

`status` and `error` are used by both `loadSlots` and `submitBooking`. If a slot reload triggers while a booking is submitting, they overwrite each other. `locationsStatus` was correctly split out — the same pattern needs to be applied to the remaining two operations.

**Fix:**

```ts
const slotsStatus = ref<'idle' | 'loading' | 'error'>('idle')
const bookingStatus = ref<'idle' | 'loading' | 'success' | 'error'>('idle')
const slotsError = ref<string | null>(null)
const bookingError = ref<string | null>(null)
```

Update `loadSlots` to use `slotsStatus`/`slotsError` and `submitBooking` to use `bookingStatus`/`bookingError`. Update `useBookingForm` to read from `bookingStatus` and `bookingError`.

---

## Moderate

### 4. `BookingModal` has no focus trap

**File:** `src/components/BookingModal.vue`

The modal renders over the page but does not move focus inside it or prevent keyboard focus from reaching content behind the overlay. This breaks keyboard navigation and screen reader behaviour (WCAG 2.1 — 2.1.2 No Keyboard Trap / 4.1.3 ARIA dialog pattern).

**Fix:**

```ts
// BookingModal.vue <script setup>
import { nextTick, watch } from 'vue'
const firstInput = ref<HTMLInputElement | null>(null)

watch(() => props.visible, (visible) => {
  if (visible) nextTick(() => firstInput.value?.focus())
})
```

Add `ref="firstInput"` to the name `<input>`. For a full focus trap (Tab cycles within the modal), use `focus-trap-vue` or implement `focusin` boundary detection.

---

### 5. Escape key handler on the overlay div does not fire

**File:** `src/components/BookingModal.vue`, line with `@keydown="onOverlayKeydown"`

`keydown` is bound to the overlay `<div>`, but the div is not focusable — focus is on the inputs inside it. The event never reaches the overlay, so pressing Escape does nothing.

**Fix:**

```ts
onMounted(() => document.addEventListener('keydown', onOverlayKeydown))
onUnmounted(() => document.removeEventListener('keydown', onOverlayKeydown))
```

Remove the `@keydown` binding from the template. Alternatively, replace the `<div>` with a native `<dialog>` element which handles Escape natively.

---

### 6. Form fields and errors not reset when modal closes without submitting

**File:** `src/composables/useBookingForm.ts`

If a user partially fills the form, closes the modal without submitting, then reopens it, the stale values and any validation errors from the previous attempt are still shown.

**Fix:**

Expose a `reset()` function from `useBookingForm`:

```ts
function reset(): void {
  name.value = ''
  email.value = ''
  phone.value = ''
  notes.value = ''
  errors.value = {}
}

return { name, email, phone, notes, errors, isSubmitting, submit, reset }
```

Call it in `BookingView.onModalClose`:

```ts
const { reset } = useBookingForm()

function onModalClose(): void {
  reset()
  store.clearSelectedSlot()
  store.resetStatus()
  ...
}
```

---

### 7. `SlotCard` has a redundant `handleClick` guard alongside `:disabled`

**File:** `src/components/SlotCard.vue`

The button has `:disabled="slot.status === 'unavailable'"` which prevents clicks natively. The `handleClick` function then also checks `slot.status` before emitting — the check is never reached for unavailable slots because the browser suppresses click events on disabled buttons.

**Fix:** Remove `handleClick` entirely and bind `@click` directly:

```html
<button
  ...
  :disabled="slot.status === 'unavailable'"
  @click="emit('select')"
>
```

---

## Minor

### 8. `Date.now()` used as booking ID in mock

**File:** `src/api/booking.ts` (mock `createBooking`)

`bookingId: \`bk-${Date.now()}\`` is not unique under concurrent calls. Fine for the mock, but document that the real backend generates a cryptographically random reference.

---

### 9. No `server.proxy` in `vite.config.ts`

**File:** `frontend/vite.config.ts`

When the real API is wired up, the frontend will hit CORS in development unless a proxy is configured. Add:

```ts
server: {
  proxy: {
    '/api': { target: 'http://localhost:5005', changeOrigin: true },
  },
}
```

---

### 10. `SLOT_TIMES` array duplicated between frontend and backend

**File:** `src/api/booking.ts` (mock) and `backend/overtime/Service/SlotService.cs`

Both define the same 16 time slots. Once the real API is used, the frontend no longer needs this list — `fetchSlots` will return slot data from the backend. Remove from the frontend when the mock is replaced.

---

### 11. Empty placeholder test

**File:** `frontend/src/` — no frontend placeholder, but see Backend-fixes.md for the backend equivalent.

---

### 12. `index.html` has default Vite placeholder title

**File:** `frontend/index.html`

The page title and `<meta>` description are the default Vite scaffold values. Update to reflect the application name and purpose.
