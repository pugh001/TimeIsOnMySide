<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { useBookingStore } from '@/stores/bookingStore'
import { useAuthStore } from '@/stores/authStore'
import { useSlots } from '@/composables/useSlots'
import { useBookingForm } from '@/composables/useBookingForm'
import AppHeader from '@/components/AppHeader.vue'
import DateStrip from '@/components/DateStrip.vue'
import SlotGrid from '@/components/SlotGrid.vue'
import BookingModal from '@/components/BookingModal.vue'
import LocationPicker from '@/components/LocationPicker.vue'
import type { Location, Slot } from '@/api/schemas'

const store = useBookingStore()
const auth = useAuthStore()
const { today, selectedDate, weekDates, setDate } = useSlots()
const { reset } = useBookingForm()

const modalVisible = computed(() => store.selectedSlot !== null)

onMounted(() => store.loadLocations())

function onLocationSelect(loc: Location): void {
  store.selectLocation(loc)
  store.loadSlots(selectedDate.value, loc.id)
}

function onDateSelect(date: string): void {
  setDate(date)
  if (store.selectedLocation) {
    store.loadSlots(date, store.selectedLocation.id)
  }
}

function onSlotSelect(slot: Slot): void {
  store.selectSlot(slot)
}

function onModalClose(): void {
  const wasSuccess = store.bookingStatus === 'success'
  reset()
  store.clearSelectedSlot()
  store.resetStatus()
  if (wasSuccess && store.selectedLocation) {
    store.loadSlots(selectedDate.value, store.selectedLocation.id)
  }
}
</script>

<template>
  <div class="booking-view">
    <AppHeader :employee-name="auth.employeeName" :role="auth.role" />

    <main class="main-content">
      <section class="location-section">
        <h2 class="section-label">Select a location</h2>
        <LocationPicker
          :locations="store.locations"
          :selected-location="store.selectedLocation"
          :status="store.locationsStatus"
          @select-location="onLocationSelect"
        />
      </section>

      <template v-if="store.selectedLocation">
        <section class="date-section">
          <h2 class="section-label">Select a date</h2>
          <DateStrip :dates="weekDates" :selected-date="selectedDate" :today="today" @select-date="onDateSelect" />
        </section>

        <section class="slots-section">
          <h2 class="section-label">Available slots</h2>
          <SlotGrid :date="selectedDate" @select-slot="onSlotSelect" />
        </section>
      </template>
    </main>

    <BookingModal
      v-if="modalVisible"
      :slot-data="store.selectedSlot"
      @close="onModalClose"
    />
  </div>
</template>

<style scoped>
.booking-view {
  min-height: 100vh;
  width: 100%;
  background: var(--color-surface, #f8fafc);
}

.main-content {
  max-width: 56rem;
  width: 100%;
  margin: 0 auto;
  padding: 2rem;
  display: flex;
  flex-direction: column;
  align-items: stretch;
  gap: 2rem;
}

.section-label {
  font-size: 0.8rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.08em;
  color: var(--color-text-muted, #64748b);
  margin: 0 0 0.75rem;
}
</style>
