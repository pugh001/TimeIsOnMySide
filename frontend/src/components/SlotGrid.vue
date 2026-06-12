<script setup lang="ts">
import { watch } from 'vue'
import { useBookingStore } from '@/stores/bookingStore'
import { useSlots } from '@/composables/useSlots'
import SlotCard from './SlotCard.vue'
import CalendarIcon from './CalendarIcon.vue'
import type { Slot } from '@/api/schemas'

const props = defineProps<{ date: string }>()
const emit = defineEmits<{ 'select-slot': [slot: Slot] }>()

const store = useBookingStore()
const { slotsForDate, setDate } = useSlots()

watch(() => props.date, setDate, { immediate: true })
</script>

<template>
  <div class="slot-grid">
    <div v-if="store.slotsStatus === 'loading'" class="state-msg">Loading slots…</div>
    <div v-else-if="slotsForDate.length === 0" data-testid="no-slots-empty-state" class="empty-state">
      <CalendarIcon class="empty-icon">
        <line x1="9" y1="15" x2="15" y2="15" />
      </CalendarIcon>
      <p class="empty-heading">No slots available</p>
      <p class="empty-subtext">There are no available slots for this date. Try selecting a different date.</p>
    </div>
    <div v-else class="grid">
      <SlotCard
        v-for="slot in slotsForDate"
        :key="slot.id"
        :slot-data="slot"
        @select="emit('select-slot', slot)"
      />
    </div>
  </div>
</template>

<style scoped>
.slot-grid {
  padding: 1rem 0;
}

.grid {
  display: flex;
  flex-wrap: wrap;
  gap: 0.75rem;
}

.state-msg {
  color: var(--color-text-muted, #64748b);
  font-size: 0.95rem;
  padding: 1rem 0;
}

.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.5rem;
  padding: 2.5rem 1rem;
  text-align: center;
  color: var(--color-text-muted, #64748b);
}

.empty-icon {
  color: var(--color-text-muted, #94a3b8);
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
</style>
