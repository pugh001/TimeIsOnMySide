<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'
import { createUser } from '@/api/users'
import { getLocation } from '@/api/locations'
import { CreateUserRequestSchema, DAY_KEYS } from '@/api/schemas'
import type { CreateUserResponse, DayKey, Location } from '@/api/schemas'

type Status = 'idle' | 'loading' | 'success' | 'error'
type LocationStatus = 'loading' | 'ready' | 'error'

const DAYS: { key: DayKey; label: string }[] = [
  { key: 'monday',    label: 'Monday' },
  { key: 'tuesday',   label: 'Tuesday' },
  { key: 'wednesday', label: 'Wednesday' },
  { key: 'thursday',  label: 'Thursday' },
  { key: 'friday',    label: 'Friday' },
  { key: 'saturday',  label: 'Saturday' },
  { key: 'sunday',    label: 'Sunday' },
]

const router = useRouter()
const route = useRoute()
const auth = useAuthStore()

const firstName = ref('')
const lastName = ref('')
const password = ref('')
const confirmPassword = ref('')

const location = ref<Location | null>(null)
const locationStatus = ref<LocationStatus>('loading')
const locationLoadError = ref('')

type DayShift = { enabled: boolean; shiftStart: string; shiftEnd: string }
const dayShifts = reactive<Record<DayKey, DayShift>>(
  Object.fromEntries(DAY_KEYS.map(k => [k, { enabled: false, shiftStart: '09:00', shiftEnd: '17:00' }])) as Record<DayKey, DayShift>
)

const firstNameError = ref('')
const lastNameError = ref('')
const passwordError = ref('')
const confirmPasswordError = ref('')
const workingTimesError = ref('')
const apiError = ref('')

const status = ref<Status>('idle')
const result = ref<CreateUserResponse | null>(null)
const showUsernameModal = ref(false)

const openDays = computed<DayKey[]>(() =>
  DAY_KEYS.filter(k => location.value?.openingHours?.[k] != null)
)

function dayOpenTime(key: DayKey): string {
  return location.value?.openingHours?.[key]?.openTime ?? '00:00'
}

function dayCloseTime(key: DayKey): string {
  return location.value?.openingHours?.[key]?.closeTime ?? '23:59'
}

function applyLocationDefaults(): void {
  for (const key of DAY_KEYS) {
    const hours = location.value?.openingHours?.[key]
    if (hours) {
      dayShifts[key].shiftStart = hours.openTime
      dayShifts[key].shiftEnd = hours.closeTime
    }
    dayShifts[key].enabled = false
  }
}

function validateForm(): boolean {
  let valid = true

  firstNameError.value = ''
  lastNameError.value = ''
  passwordError.value = ''
  confirmPasswordError.value = ''
  workingTimesError.value = ''

  const workingTimes = openDays.value
    .filter(k => dayShifts[k].enabled)
    .map(k => ({ day: k, shiftStart: dayShifts[k].shiftStart, shiftEnd: dayShifts[k].shiftEnd }))

  const parsed = CreateUserRequestSchema.safeParse({
    firstName: firstName.value,
    lastName: lastName.value,
    password: password.value,
    locationId: location.value?.id ?? '',
    workingTimes,
  })

  if (!parsed.success) {
    const errorMap: Record<string, { value: string }> = {
      firstName: firstNameError,
      lastName: lastNameError,
      password: passwordError,
      workingTimes: workingTimesError,
    }
    for (const issue of parsed.error.issues) {
      const ref = errorMap[issue.path[0] as string]
      if (ref) ref.value = issue.message
    }
    valid = false
  }

  if (password.value !== confirmPassword.value) {
    confirmPasswordError.value = 'Passwords do not match'
    valid = false
  }

  return valid
}

async function onSubmit(): Promise<void> {
  if (!validateForm()) return
  if (!auth.adminToken || !auth.adminUserId) {
    apiError.value = 'Admin credentials missing — please log in again'
    return
  }

  status.value = 'loading'
  apiError.value = ''

  const workingTimes = openDays.value
    .filter(k => dayShifts[k].enabled)
    .map(k => ({ day: k, shiftStart: dayShifts[k].shiftStart, shiftEnd: dayShifts[k].shiftEnd }))

  try {
    result.value = await createUser(
      {
        firstName: firstName.value,
        lastName: lastName.value,
        password: password.value,
        locationId: location.value!.id,
        workingTimes,
      },
      auth.adminToken,
      auth.adminUserId,
    )
    status.value = 'success'
    showUsernameModal.value = true
  } catch (err) {
    status.value = 'error'
    apiError.value = err instanceof Error ? err.message : 'Something went wrong'
  }
}

function onCloseModal(): void {
  showUsernameModal.value = false
  onAddAnother()
}

function onAddAnother(): void {
  firstName.value = ''
  lastName.value = ''
  password.value = ''
  confirmPassword.value = ''
  firstNameError.value = ''
  lastNameError.value = ''
  passwordError.value = ''
  confirmPasswordError.value = ''
  workingTimesError.value = ''
  apiError.value = ''
  result.value = null
  status.value = 'idle'
  applyLocationDefaults()
}

onMounted(async () => {
  const locationId = route.query.locationId as string | undefined
  if (!locationId) {
    locationLoadError.value = 'No location selected — please go back and select a location first'
    locationStatus.value = 'error'
    return
  }
  try {
    location.value = await getLocation(locationId, auth.adminToken ?? '', auth.adminUserId ?? '')
    applyLocationDefaults()
    locationStatus.value = 'ready'
  } catch (err) {
    locationLoadError.value = err instanceof Error ? err.message : 'Failed to load location'
    locationStatus.value = 'error'
  }
})

function onBack(): void {
  router.push('/admin')
}
</script>

<template>
  <div class="add-user-view">
    <div class="page-card">
      <div class="page-header">
        <button class="back-link" data-testid="back-btn" @click="onBack">← Back</button>
        <h1 class="page-title">Add user</h1>
        <p v-if="location" class="page-subtitle" data-testid="location-name">
          {{ location.name }}
        </p>
      </div>

      <!-- Username modal -->
      <div v-if="showUsernameModal && result" class="modal-overlay" data-testid="username-modal" role="dialog" aria-modal="true" aria-labelledby="modal-title">
        <div class="modal-card">
          <div class="modal-icon">✓</div>
          <h2 id="modal-title" class="modal-title">User created</h2>
          <p class="modal-body">The login username for this user is:</p>
          <div class="modal-username-display" data-testid="modal-username">{{ result.username }}</div>
          <p class="modal-hint">Please note this down — the username cannot be changed later.</p>
          <button class="btn btn-primary modal-btn" data-testid="modal-close-btn" @click="onCloseModal">
            Done
          </button>
        </div>
      </div>

      <div v-if="locationStatus === 'loading'" data-testid="location-loading" class="state-msg">
        Loading location…
      </div>

      <div v-else-if="locationStatus === 'error'" data-testid="location-load-error" class="api-error">
        {{ locationLoadError }}
      </div>

      <form v-else data-testid="add-user-form" class="user-form" @submit.prevent="onSubmit">
        <div class="form-grid">
          <!-- Personal details column -->
          <section class="form-section">
            <h2 class="section-title">Personal details</h2>

            <div class="field">
              <label for="first-name" class="field-label">First name</label>
              <input
                id="first-name"
                v-model="firstName"
                data-testid="first-name-input"
                type="text"
                class="field-input"
                :class="{ 'field-input--error': firstNameError }"
                placeholder="e.g. Jane"
              />
              <span v-if="firstNameError" data-testid="first-name-error" class="field-error">{{ firstNameError }}</span>
            </div>

            <div class="field">
              <label for="last-name" class="field-label">Last name</label>
              <input
                id="last-name"
                v-model="lastName"
                data-testid="last-name-input"
                type="text"
                class="field-input"
                :class="{ 'field-input--error': lastNameError }"
                placeholder="e.g. Doe"
              />
              <span v-if="lastNameError" data-testid="last-name-error" class="field-error">{{ lastNameError }}</span>
            </div>
          </section>

          <!-- Credentials column -->
          <section class="form-section">
            <h2 class="section-title">Credentials</h2>

            <div class="field">
              <label for="password" class="field-label">Password</label>
              <input
                id="password"
                v-model="password"
                data-testid="password-input"
                type="password"
                class="field-input"
                :class="{ 'field-input--error': passwordError }"
                autocomplete="new-password"
              />
              <span v-if="passwordError" data-testid="password-error" class="field-error">{{ passwordError }}</span>
            </div>

            <div class="field">
              <label for="confirm-password" class="field-label">Confirm password</label>
              <input
                id="confirm-password"
                v-model="confirmPassword"
                data-testid="confirm-password-input"
                type="password"
                class="field-input"
                :class="{ 'field-input--error': confirmPasswordError }"
                autocomplete="new-password"
              />
              <span v-if="confirmPasswordError" data-testid="confirm-password-error" class="field-error">{{ confirmPasswordError }}</span>
            </div>
          </section>
        </div>

        <!-- Working days & shifts -->
        <section class="form-section">
          <h2 class="section-title">Working days & shift times</h2>

          <div v-if="openDays.length === 0" class="field-hint">
            This location has no opening hours configured.
          </div>

          <div v-else class="day-shifts">
            <div
              v-for="day in openDays"
              :key="day"
              class="day-shift-row"
              :class="{ 'day-shift-row--active': dayShifts[day].enabled }"
            >
              <label class="day-toggle">
                <input
                  v-model="dayShifts[day].enabled"
                  :data-testid="`work-day-${day}`"
                  type="checkbox"
                  class="day-checkbox"
                />
                <span class="day-name">{{ day.charAt(0).toUpperCase() + day.slice(1) }}</span>
                <span class="day-hours-hint">{{ dayOpenTime(day) }}–{{ dayCloseTime(day) }}</span>
              </label>

              <div v-if="dayShifts[day].enabled" class="shift-inputs">
                <div class="shift-field">
                  <label :for="`shift-start-${day}`" class="field-label">Start</label>
                  <input
                    :id="`shift-start-${day}`"
                    v-model="dayShifts[day].shiftStart"
                    :data-testid="`shift-start-${day}`"
                    type="time"
                    class="field-input time-input"
                    :min="dayOpenTime(day)"
                    :max="dayCloseTime(day)"
                  />
                </div>
                <span class="time-sep">–</span>
                <div class="shift-field">
                  <label :for="`shift-end-${day}`" class="field-label">End</label>
                  <input
                    :id="`shift-end-${day}`"
                    v-model="dayShifts[day].shiftEnd"
                    :data-testid="`shift-end-${day}`"
                    type="time"
                    class="field-input time-input"
                    :min="dayOpenTime(day)"
                    :max="dayCloseTime(day)"
                  />
                </div>
              </div>
            </div>
          </div>

          <span v-if="workingTimesError" data-testid="working-times-error" class="field-error">{{ workingTimesError }}</span>
        </section>

        <span v-if="apiError" data-testid="api-error" class="api-error">{{ apiError }}</span>

        <button
          type="submit"
          data-testid="submit-btn"
          class="btn btn-primary submit-btn"
          :disabled="status === 'loading'"
        >
          {{ status === 'loading' ? 'Creating…' : 'Create user' }}
        </button>
      </form>
    </div>
  </div>
</template>

<style scoped>
.add-user-view {
  min-height: 100vh;
  background: var(--color-background, #f8fafc);
  padding: 2rem;
  display: flex;
  justify-content: center;
}

.page-card {
  width: 100%;
  max-width: 48rem;
}

.page-header {
  margin-bottom: 1.75rem;
}

.back-link {
  background: none;
  border: none;
  color: #3b82f6;
  font-size: 0.875rem;
  cursor: pointer;
  padding: 0;
  margin-bottom: 0.75rem;
  display: block;
}

.back-link:hover {
  text-decoration: underline;
}

.page-title {
  font-size: 1.5rem;
  font-weight: 700;
  color: var(--color-text-primary, #0f172a);
  margin: 0 0 0.25rem;
}

.page-subtitle {
  font-size: 0.9rem;
  color: var(--color-text-muted, #64748b);
  margin: 0;
}

.state-msg {
  color: var(--color-text-muted, #64748b);
  font-size: 0.95rem;
  padding: 1rem 0;
}

.user-form {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.form-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1.5rem;
}

@media (max-width: 600px) {
  .form-grid {
    grid-template-columns: 1fr;
  }
}

.form-section {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  padding: 1.25rem;
  background: var(--color-card, #fff);
  border: 1px solid var(--color-border, #e2e8f0);
  border-radius: 0.5rem;
}

.section-title {
  font-size: 0.8125rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--color-text-muted, #64748b);
  margin: 0;
}

.field {
  display: flex;
  flex-direction: column;
  gap: 0.375rem;
}

.field-label {
  font-size: 0.875rem;
  font-weight: 500;
  color: var(--color-text-primary, #0f172a);
}

.field-input {
  width: 100%;
  padding: 0.625rem 0.75rem;
  border: 1px solid var(--color-border, #cbd5e1);
  border-radius: 0.375rem;
  font-size: 0.9375rem;
  color: var(--color-text-primary, #0f172a);
  background: var(--color-card, #fff);
  outline: none;
  box-sizing: border-box;
  transition: border-color 0.15s ease;
}

.field-input:focus {
  border-color: var(--color-brand, #0d9488);
  box-shadow: 0 0 0 3px rgba(13, 148, 136, 0.12);
}

.field-input--error {
  border-color: #dc2626;
}

.time-input {
  width: auto;
  min-width: 8rem;
}

.field-error {
  font-size: 0.8125rem;
  color: #dc2626;
}

.field-hint {
  font-size: 0.8125rem;
  color: var(--color-text-muted, #64748b);
  margin: 0;
}

.api-error {
  font-size: 0.875rem;
  color: #dc2626;
}

/* Day-shift rows */
.day-shifts {
  display: flex;
  flex-direction: column;
  gap: 0.625rem;
}

.day-shift-row {
  border: 1px solid var(--color-border, #e2e8f0);
  border-radius: 0.375rem;
  padding: 0.75rem 1rem;
  background: var(--color-surface, #f8fafc);
  transition: border-color 0.15s ease, background 0.15s ease;
}

.day-shift-row--active {
  border-color: var(--color-brand, #0d9488);
  background: #f0fdfa;
}

.day-toggle {
  display: flex;
  align-items: center;
  gap: 0.625rem;
  cursor: pointer;
  user-select: none;
}

.day-checkbox {
  accent-color: var(--color-brand, #0d9488);
  width: 1rem;
  height: 1rem;
  cursor: pointer;
  flex-shrink: 0;
}

.day-name {
  font-size: 0.9375rem;
  font-weight: 500;
  color: var(--color-text-primary, #0f172a);
  min-width: 6.5rem;
}

.day-hours-hint {
  font-size: 0.8125rem;
  color: var(--color-text-muted, #64748b);
}

.shift-inputs {
  display: flex;
  align-items: flex-end;
  gap: 0.75rem;
  margin-top: 0.75rem;
  padding-left: 1.625rem;
}

.shift-field {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.time-sep {
  color: var(--color-text-muted, #64748b);
  font-size: 0.875rem;
  padding-bottom: 0.7rem;
}

/* Buttons */
.btn {
  padding: 0.75rem 1.25rem;
  font-size: 0.9375rem;
  font-weight: 600;
  border: none;
  border-radius: 0.375rem;
  cursor: pointer;
  transition: background 0.15s ease;
}

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-primary {
  background: var(--color-brand, #0d9488);
  color: #fff;
}

.btn-primary:hover:not(:disabled) {
  background: #0f766e;
}

.submit-btn {
  width: 100%;
}

/* Modal */
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(15, 23, 42, 0.55);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 200;
  padding: 1rem;
}

.modal-card {
  background: var(--color-card, #fff);
  border-radius: 0.75rem;
  padding: 2rem 2.5rem;
  max-width: 24rem;
  width: 100%;
  text-align: center;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.2);
}

.modal-icon {
  font-size: 2.5rem;
  color: var(--color-brand, #0d9488);
  margin-bottom: 0.75rem;
}

.modal-title {
  font-size: 1.25rem;
  font-weight: 700;
  color: var(--color-text-primary, #0f172a);
  margin: 0 0 0.5rem;
}

.modal-body {
  font-size: 0.9rem;
  color: var(--color-text-muted, #64748b);
  margin: 0 0 0.75rem;
}

.modal-username-display {
  font-size: 1.5rem;
  font-weight: 700;
  font-family: ui-monospace, monospace;
  color: var(--color-brand, #0d9488);
  background: var(--color-surface, #f0fdfa);
  border: 1.5px solid var(--color-brand, #0d9488);
  border-radius: 0.375rem;
  padding: 0.5rem 1rem;
  margin-bottom: 0.75rem;
  letter-spacing: 0.04em;
}

.modal-hint {
  font-size: 0.8125rem;
  color: var(--color-text-muted, #94a3b8);
  margin: 0 0 1.5rem;
}

.modal-btn {
  width: 100%;
}
</style>
