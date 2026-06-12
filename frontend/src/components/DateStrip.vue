<script setup lang="ts">
import type { DateEntry } from '@/composables/useSlots'

defineProps<{ dates: DateEntry[]; selectedDate: string; today?: string }>()
const emit = defineEmits<{ 'select-date': [date: string] }>()
</script>

<template>
  <div class="date-strip">
    <button
      v-for="date in dates"
      :key="date.iso"
      class="date-btn"
      :class="{ active: date.iso === selectedDate, past: today && date.iso < today }"
      :disabled="today ? date.iso < today : false"
      @click="emit('select-date', date.iso)"
    >
      <span class="day-name">{{ date.dayName }}</span>
      <span class="day-num">{{ date.dayNum }}</span>
    </button>
  </div>
</template>

<style scoped>
.date-strip {
  display: flex;
  gap: 0.5rem;
  overflow-x: auto;
  padding: 0.5rem 0;
}

.date-btn {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 0.6rem 1rem;
  border-radius: var(--radius-card, 0.5rem);
  border: 1.5px solid var(--color-border, #e2e8f0);
  background: var(--color-card, #fff);
  cursor: pointer;
  min-width: 3.5rem;
  gap: 0.2rem;
  transition: border-color 0.15s;
}

.date-btn.active {
  border-color: var(--color-brand, #0d9488);
  background: var(--color-brand, #0d9488);
  color: #fff;
}

.date-btn:hover:not(.active):not(:disabled) {
  border-color: var(--color-brand, #0d9488);
}

.date-btn.past,
.date-btn:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

.day-name {
  font-size: 0.7rem;
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.day-num {
  font-size: 1.1rem;
  font-weight: 600;
}
</style>
