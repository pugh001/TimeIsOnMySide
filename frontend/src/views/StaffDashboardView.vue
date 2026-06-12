<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useAuthStore } from '@/stores/authStore'
import { fetchBookings } from '@/api/bookings'
import AppHeader from '@/components/AppHeader.vue'
import CalendarIcon from '@/components/CalendarIcon.vue'
import type { StaffBookingResponse } from '@/api/schemas'

type Status = 'loading' | 'success' | 'error'
type Tab = 'future' | 'past'

const auth = useAuthStore()
const status = ref<Status>('loading')
const bookings = ref<StaffBookingResponse[]>([])
const apiError = ref('')
const activeTab = ref<Tab>('future')

onMounted(async () => {
  if (!auth.staffUserId || !auth.staffToken) {
    apiError.value = 'Missing credentials — please log in again'
    status.value = 'error'
    return
  }
  try {
    bookings.value = await fetchBookings(auth.staffUserId, auth.staffToken)
    status.value = 'success'
  } catch (err) {
    apiError.value = err instanceof Error ? err.message : 'Failed to load bookings'
    status.value = 'error'
  }
})

const splitBookings = computed(() => {
  const now = new Date()
  const y = now.getFullYear()
  const m = String(now.getMonth() + 1).padStart(2, '0')
  const d = String(now.getDate()).padStart(2, '0')
  const todayIso = `${y}-${m}-${d}`
  const nowTime = now.toTimeString().slice(0, 5)
  const isFuture = (b: StaffBookingResponse) =>
    b.date > todayIso || (b.date === todayIso && b.startTime >= nowTime)
  return {
    future: bookings.value.filter(isFuture),
    past: bookings.value.filter((b) => !isFuture(b)),
  }
})
const futureBookings  = computed(() => splitBookings.value.future)
const pastBookings    = computed(() => splitBookings.value.past)
const visibleBookings = computed(() => activeTab.value === 'future' ? futureBookings.value : pastBookings.value)
</script>

<template>
  <div class="dashboard-view">
    <AppHeader :employee-name="auth.employeeName" :role="auth.role" />

    <main class="main-content">
      <h1 class="page-title">My Bookings</h1>

      <div v-if="status === 'loading'" data-testid="loading-state" class="state-msg">
        Loading bookings…
      </div>

      <span v-else-if="status === 'error'" data-testid="api-error" class="api-error">
        {{ apiError }}
      </span>

      <div
        v-else-if="status === 'success' && bookings.length === 0"
        data-testid="empty-state"
        class="empty-state"
      >
        <CalendarIcon class="empty-icon" />
        <p class="empty-heading">No bookings assigned yet</p>
        <p class="empty-subtext">No bookings have been assigned to you yet. Check back later.</p>
      </div>

      <template v-else-if="status === 'success'">
        <div class="tabs" role="tablist">
          <button
            data-testid="tab-future"
            role="tab"
            :aria-selected="activeTab === 'future'"
            class="tab"
            :class="{ active: activeTab === 'future' }"
            @click="activeTab = 'future'"
          >
            Future
          </button>
          <button
            data-testid="tab-past"
            role="tab"
            :aria-selected="activeTab === 'past'"
            class="tab"
            :class="{ active: activeTab === 'past' }"
            @click="activeTab = 'past'"
          >
            Past
          </button>
        </div>

        <div
          v-if="visibleBookings.length === 0"
          data-testid="tab-empty-state"
          class="tab-empty-state"
        >
          <p class="empty-heading">No {{ activeTab }} bookings</p>
          <p class="empty-subtext">
            {{ activeTab === 'future' ? 'No upcoming bookings assigned to you.' : 'No past bookings on record.' }}
          </p>
        </div>

        <div v-else data-testid="booking-list" class="booking-list">
          <div
            v-for="booking in visibleBookings"
            :key="booking.bookingRef"
            class="booking-card"
          >
            <div class="booking-ref">{{ booking.bookingRef }}</div>
            <div class="booking-detail">
              <span class="booking-date">{{ booking.date }}</span>
              <span class="booking-time">{{ booking.startTime }}–{{ booking.endTime }}</span>
            </div>
            <div class="booking-customer">{{ booking.customerName }}</div>
          </div>
        </div>
      </template>
    </main>
  </div>
</template>

<style scoped>
.dashboard-view {
  min-height: 100vh;
  background: var(--color-surface, #f8fafc);
}

.main-content {
  max-width: 48rem;
  margin: 0 auto;
  padding: 2rem;
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.page-title {
  font-size: 1.5rem;
  font-weight: 700;
  color: var(--color-text, #0f172a);
  margin: 0;
}

.state-msg {
  color: var(--color-text-muted, #64748b);
  font-size: 0.95rem;
  padding: 1rem 0;
}

.api-error {
  font-size: 0.875rem;
  color: #dc2626;
}

.tabs {
  display: flex;
  gap: 0;
  border-bottom: 2px solid var(--color-border, #e2e8f0);
}

.tab {
  padding: 0.625rem 1.25rem;
  font-size: 0.9rem;
  font-weight: 600;
  color: var(--color-text-muted, #64748b);
  background: transparent;
  border: none;
  border-bottom: 2px solid transparent;
  margin-bottom: -2px;
  cursor: pointer;
  transition: color 0.15s, border-color 0.15s;
}

.tab:hover {
  color: var(--color-text, #0f172a);
}

.tab.active {
  color: var(--color-brand, #0d9488);
  border-bottom-color: var(--color-brand, #0d9488);
}

.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.5rem;
  padding: 3rem 1rem;
  text-align: center;
  color: var(--color-text-muted, #64748b);
}

.tab-empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.375rem;
  padding: 2.5rem 1rem;
  text-align: center;
  color: var(--color-text-muted, #64748b);
}

.empty-icon {
  color: #94a3b8;
  margin-bottom: 0.25rem;
}

.empty-heading {
  margin: 0;
  font-size: 1rem;
  font-weight: 600;
  color: var(--color-text, #1e293b);
}

.empty-subtext {
  margin: 0;
  font-size: 0.875rem;
  max-width: 28rem;
}

.booking-list {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.booking-card {
  background: #ffffff;
  border: 1px solid #e2e8f0;
  border-radius: 0.5rem;
  padding: 1rem 1.25rem;
  display: grid;
  grid-template-columns: 1fr auto;
  grid-template-rows: auto auto;
  gap: 0.25rem 1rem;
}

.booking-ref {
  grid-column: 1;
  grid-row: 1;
  font-size: 0.75rem;
  font-weight: 600;
  color: var(--color-text-muted, #64748b);
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.booking-detail {
  grid-column: 2;
  grid-row: 1 / 3;
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 0.125rem;
}

.booking-date {
  font-size: 0.875rem;
  font-weight: 600;
  color: var(--color-text, #0f172a);
}

.booking-time {
  font-size: 0.8125rem;
  color: var(--color-text-muted, #64748b);
}

.booking-customer {
  grid-column: 1;
  grid-row: 2;
  font-size: 0.9375rem;
  font-weight: 500;
  color: var(--color-text, #0f172a);
}
</style>
