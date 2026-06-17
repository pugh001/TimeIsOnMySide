<script setup lang="ts">
import { ref } from 'vue'
import { fetchUserBookings } from '@/api/users'
import BookingCard from './BookingCard.vue'
import type { UserSummary, StaffBookingResponse } from '@/api/schemas'

const props = defineProps<{
  user: UserSummary
  adminToken: string
  adminUserId: string
}>()

type BookingsStatus = 'idle' | 'loading' | 'success' | 'error'

const showBookings = ref(false)
const bookingsStatus = ref<BookingsStatus>('idle')
const bookings = ref<StaffBookingResponse[]>([])
const bookingsError = ref('')

async function toggleBookings(): Promise<void> {
  if (showBookings.value) {
    showBookings.value = false
    return
  }
  showBookings.value = true
  if (bookingsStatus.value === 'success') return
  bookingsStatus.value = 'loading'
  try {
    bookings.value = await fetchUserBookings(props.user.id, props.adminToken, props.adminUserId)
    bookingsStatus.value = 'success'
  } catch (err) {
    bookingsError.value = err instanceof Error ? err.message : 'Failed to load bookings'
    bookingsStatus.value = 'error'
  }
}
</script>

<template>
  <li class="user-item" data-testid="user-row">
    <div class="user-header">
      <div class="user-info">
        <span class="user-name">{{ user.fullName }}</span>
        <span class="user-schedule">
          <span
            v-for="wt in user.workingTimes"
            :key="wt.day"
            class="user-day-shift"
          >{{ wt.day.slice(0, 3) }} {{ wt.shiftStart }}–{{ wt.shiftEnd }}</span>
        </span>
      </div>
      <button
        class="btn-bookings"
        data-testid="show-bookings-btn"
        :aria-expanded="showBookings"
        @click="toggleBookings"
      >
        {{ showBookings ? 'Hide Bookings' : 'Show Bookings' }}
      </button>
    </div>

    <div v-if="showBookings" class="bookings-section">
      <div v-if="bookingsStatus === 'loading'" data-testid="bookings-loading" class="bookings-state">
        Loading bookings…
      </div>

      <div v-else-if="bookingsStatus === 'error'" data-testid="bookings-error" class="bookings-state bookings-state--error">
        {{ bookingsError }}
      </div>

      <div v-else-if="bookingsStatus === 'success' && bookings.length === 0" data-testid="bookings-empty" class="bookings-state">
        No upcoming bookings.
      </div>

      <div v-else-if="bookingsStatus === 'success'" data-testid="bookings-list" class="bookings-list">
        <BookingCard
          v-for="booking in bookings"
          :key="booking.bookingRef"
          :booking="booking"
        />
      </div>
    </div>
  </li>
</template>

<style scoped>
.user-item {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  font-size: 0.875rem;
  color: #0f172a;
  padding: 0.5rem 0;
  border-bottom: 1px solid #f1f5f9;
}

.user-item:last-child {
  border-bottom: none;
}

.user-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 0.75rem;
}

.user-info {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  flex: 1;
  min-width: 0;
}

.user-name {
  font-weight: 500;
}

.user-schedule {
  display: flex;
  flex-wrap: wrap;
  gap: 0.25rem 0.75rem;
}

.user-day-shift {
  font-size: 0.8125rem;
  color: #64748b;
  text-transform: capitalize;
}

.btn-bookings {
  padding: 0.25rem 0.625rem;
  font-size: 0.75rem;
  font-weight: 600;
  border: 1px solid #cbd5e1;
  border-radius: 0.25rem;
  background: #f8fafc;
  color: #0f172a;
  cursor: pointer;
  white-space: nowrap;
  flex-shrink: 0;
  transition: background 0.15s ease;
}

.btn-bookings:hover {
  background: #e2e8f0;
}

.bookings-section {
  padding-left: 0.5rem;
  border-left: 2px solid #e2e8f0;
}

.bookings-state {
  font-size: 0.8125rem;
  color: #64748b;
  padding: 0.375rem 0;
}

.bookings-state--error {
  color: #dc2626;
}

.bookings-list {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  padding: 0.25rem 0;
}
</style>
