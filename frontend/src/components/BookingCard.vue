<script setup lang="ts">
import type { StaffBookingResponse } from '@/api/schemas'

defineProps<{ booking: StaffBookingResponse }>()
</script>

<template>
  <div class="booking-card" data-testid="booking-card">
    <div class="booking-ref">{{ booking.bookingRef }}</div>
    <div class="booking-detail">
      <span class="booking-date">{{ booking.date }}</span>
      <span class="booking-time">{{ booking.startTime }}–{{ booking.endTime }}</span>
    </div>
    <div class="booking-customer">{{ booking.customerName }}</div>
  </div>
</template>

<style scoped>
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
