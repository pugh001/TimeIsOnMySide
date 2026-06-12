<script setup lang="ts">
import { ref, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'
import { createLocation } from '@/api/locations'
import { CreateLocationRequestSchema, DayHoursSchema } from '@/api/schemas'
import type { CreateLocationResponse } from '@/api/schemas'

type Status = 'idle' | 'loading' | 'success' | 'error'
type DayKey = 'monday' | 'tuesday' | 'wednesday' | 'thursday' | 'friday' | 'saturday' | 'sunday'

const DAYS: { key: DayKey; label: string }[] = [
  { key: 'monday', label: 'Monday' },
  { key: 'tuesday', label: 'Tuesday' },
  { key: 'wednesday', label: 'Wednesday' },
  { key: 'thursday', label: 'Thursday' },
  { key: 'friday', label: 'Friday' },
  { key: 'saturday', label: 'Saturday' },
  { key: 'sunday', label: 'Sunday' },
]

const router = useRouter()
const auth = useAuthStore()

const name = ref('')
const address = ref('')
const nameError = ref('')
const addressError = ref('')
const status = ref<Status>('idle')
const apiError = ref('')
const result = ref<CreateLocationResponse | null>(null)

const dayEnabled = reactive<Record<DayKey, boolean>>({
  monday: false,
  tuesday: false,
  wednesday: false,
  thursday: false,
  friday: false,
  saturday: false,
  sunday: false,
})

const dayHours = reactive<Record<DayKey, { openTime: string; closeTime: string }>>({
  monday: { openTime: '08:00', closeTime: '17:00' },
  tuesday: { openTime: '08:00', closeTime: '17:00' },
  wednesday: { openTime: '08:00', closeTime: '17:00' },
  thursday: { openTime: '08:00', closeTime: '17:00' },
  friday: { openTime: '08:00', closeTime: '17:00' },
  saturday: { openTime: '08:00', closeTime: '17:00' },
  sunday: { openTime: '08:00', closeTime: '17:00' },
})

const dayTimeErrors = reactive<Record<DayKey, string>>({
  monday: '',
  tuesday: '',
  wednesday: '',
  thursday: '',
  friday: '',
  saturday: '',
  sunday: '',
})

function validateForm(): boolean {
  let valid = true
  nameError.value = ''
  addressError.value = ''

  const parsed = CreateLocationRequestSchema.safeParse({ name: name.value, address: address.value })
  if (!parsed.success) {
    nameError.value = parsed.error.issues.find((i) => i.path[0] === 'name')?.message ?? ''
    addressError.value = parsed.error.issues.find((i) => i.path[0] === 'address')?.message ?? ''
    valid = false
  }

  for (const { key } of DAYS) {
    dayTimeErrors[key] = ''
    if (!dayEnabled[key]) continue
    const result = DayHoursSchema.safeParse(dayHours[key])
    if (result.success) continue
    dayTimeErrors[key] = result.error.issues[0]?.message ?? 'Invalid hours'
    valid = false
  }

  return valid
}

function buildOpeningHours() {
  const hours: Record<string, { openTime: string; closeTime: string } | null> = {}
  for (const { key } of DAYS) {
    hours[key] = dayEnabled[key] ? { ...dayHours[key] } : null
  }
  return hours
}

async function onSubmit(): Promise<void> {
  if (!validateForm()) return
  if (!auth.adminToken || !auth.adminUserId) {
    apiError.value = 'Admin credentials missing — please log in again'
    return
  }

  status.value = 'loading'
  apiError.value = ''

  try {
    result.value = await createLocation(
      { name: name.value, address: address.value, openingHours: buildOpeningHours() },
      auth.adminToken,
      auth.adminUserId,
    )
    status.value = 'success'
  } catch (err) {
    status.value = 'error'
    apiError.value = err instanceof Error ? err.message : 'Something went wrong'
  }
}

function onAddAnother(): void {
  name.value = ''
  address.value = ''
  nameError.value = ''
  addressError.value = ''
  apiError.value = ''
  result.value = null
  status.value = 'idle'
  for (const { key } of DAYS) {
    dayEnabled[key] = false
    dayTimeErrors[key] = ''
  }
}

function onBack(): void {
  router.push('/admin')
}
</script>

<template>
  <div class="add-location-view">
    <div class="page-card">
      <div class="page-header">
        <button class="back-link" data-testid="back-btn" @click="onBack">← Back</button>
        <h1 class="page-title">Add location</h1>
      </div>

      <div v-if="status === 'success' && result" class="success-state" data-testid="success-state">
        <div class="success-icon">✓</div>
        <p class="success-message">
          Location <strong>{{ result.slug }}</strong> created successfully.
        </p>
        <div class="success-actions">
          <button class="btn btn-secondary" data-testid="add-another-btn" @click="onAddAnother">
            Add another
          </button>
          <button class="btn btn-primary" data-testid="back-to-dashboard-btn" @click="onBack">
            Back to Dashboard
          </button>
        </div>
      </div>

      <form v-else data-testid="add-location-form" class="location-form" @submit.prevent="onSubmit">
        <div class="field">
          <label for="loc-name" class="field-label">Name</label>
          <input
            id="loc-name"
            v-model="name"
            data-testid="name-input"
            type="text"
            class="field-input"
            :class="{ 'field-input--error': nameError }"
            placeholder="e.g. Cape Town Branch"
          />
          <span v-if="nameError" data-testid="name-error" class="field-error">{{ nameError }}</span>
        </div>

        <div class="field">
          <label for="loc-address" class="field-label">Address</label>
          <input
            id="loc-address"
            v-model="address"
            data-testid="address-input"
            type="text"
            class="field-input"
            :class="{ 'field-input--error': addressError }"
            placeholder="e.g. 1 Adderley St, Cape Town"
          />
          <span v-if="addressError" data-testid="address-error" class="field-error">{{
            addressError
          }}</span>
        </div>

        <fieldset class="hours-fieldset">
          <legend class="hours-legend">
            Opening hours <span class="optional">(optional)</span>
          </legend>
          <div
            v-for="day in DAYS"
            :key="day.key"
            class="day-row"
            :data-testid="`day-row-${day.key}`"
          >
            <label class="day-toggle">
              <input
                type="checkbox"
                :data-testid="`toggle-${day.key}`"
                :checked="dayEnabled[day.key]"
                @change="dayEnabled[day.key] = ($event.target as HTMLInputElement).checked"
              />
              <span class="day-label">{{ day.label }}</span>
            </label>
            <template v-if="dayEnabled[day.key]">
              <input
                v-model="dayHours[day.key].openTime"
                type="time"
                class="time-input"
                :data-testid="`open-${day.key}`"
                aria-label="`${day.label} opening time`"
              />
              <span class="time-sep">–</span>
              <input
                v-model="dayHours[day.key].closeTime"
                type="time"
                class="time-input"
                :data-testid="`close-${day.key}`"
                aria-label="`${day.label} closing time`"
              />
              <span
                v-if="dayTimeErrors[day.key]"
                :data-testid="`time-error-${day.key}`"
                class="field-error"
                >{{ dayTimeErrors[day.key] }}</span
              >
            </template>
            <span v-else class="closed-label">Closed</span>
          </div>
        </fieldset>

        <span v-if="apiError" data-testid="api-error" class="api-error">{{ apiError }}</span>

        <button
          type="submit"
          data-testid="submit-btn"
          class="btn btn-primary submit-btn"
          :disabled="status === 'loading'"
        >
          {{ status === 'loading' ? 'Creating…' : 'Create location' }}
        </button>
      </form>
    </div>
  </div>
</template>

<style scoped>
.add-location-view {
  min-height: 100vh;
  background: #f8fafc;
  padding: 2rem;
  display: flex;
  justify-content: center;
}

.page-card {
  width: 100%;
  max-width: 36rem;
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
  color: #0f172a;
  margin: 0;
}

.location-form {
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
}

.field {
  display: flex;
  flex-direction: column;
  gap: 0.375rem;
}

.field-label {
  font-size: 0.875rem;
  font-weight: 500;
  color: #0f172a;
}

.field-input {
  width: 100%;
  padding: 0.625rem 0.75rem;
  border: 1px solid #cbd5e1;
  border-radius: 0.375rem;
  font-size: 1rem;
  color: #0f172a;
  background: #fff;
  outline: none;
  box-sizing: border-box;
  transition: border-color 0.15s ease;
}

.field-input:focus {
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.15);
}

.field-input--error {
  border-color: #dc2626;
}

.field-error {
  font-size: 0.8125rem;
  color: #dc2626;
}

.hours-fieldset {
  border: 1px solid #e2e8f0;
  border-radius: 0.375rem;
  padding: 1rem;
  margin: 0;
}

.hours-legend {
  font-size: 0.875rem;
  font-weight: 500;
  color: #0f172a;
  padding: 0 0.25rem;
}

.optional {
  color: #64748b;
  font-weight: 400;
}

.day-row {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.5rem 0;
  border-bottom: 1px solid #f1f5f9;
}

.day-row:last-child {
  border-bottom: none;
}

.day-toggle {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  cursor: pointer;
  min-width: 8rem;
}

.day-label {
  font-size: 0.875rem;
  color: #0f172a;
}

.time-input {
  padding: 0.25rem 0.5rem;
  border: 1px solid #cbd5e1;
  border-radius: 0.25rem;
  font-size: 0.875rem;
  color: #0f172a;
  width: 7rem;
}

.time-sep {
  color: #64748b;
  font-size: 0.875rem;
}

.closed-label {
  font-size: 0.8125rem;
  color: #94a3b8;
}

.api-error {
  font-size: 0.875rem;
  color: #dc2626;
}

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
  background: #3b82f6;
  color: #fff;
}

.btn-primary:hover:not(:disabled) {
  background: #2563eb;
}

.btn-secondary {
  background: #f1f5f9;
  color: #0f172a;
}

.btn-secondary:hover {
  background: #e2e8f0;
}

.submit-btn {
  width: 100%;
  margin-top: 0.25rem;
}

.success-state {
  text-align: center;
  padding: 2rem 0;
}

.success-icon {
  font-size: 3rem;
  color: #16a34a;
  margin-bottom: 1rem;
}

.success-message {
  font-size: 1rem;
  color: #0f172a;
  margin-bottom: 1.5rem;
}

.success-actions {
  display: flex;
  gap: 0.75rem;
  justify-content: center;
}
</style>
