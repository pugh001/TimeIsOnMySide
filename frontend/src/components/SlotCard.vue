<script setup lang="ts">
import type { Slot } from '@/api/schemas'

defineProps<{ slotData: Slot }>()
const emit = defineEmits<{ select: [] }>()
</script>

<template>
  <button
    class="slot-card"
    :class="slotData.status"
    :disabled="slotData.status === 'unavailable'"
    :aria-disabled="slotData.status === 'unavailable'"
    :aria-label="`${slotData.startTime} — ${slotData.status}`"
    @click="emit('select')"
  >
    <span class="time">{{ slotData.startTime }}</span>
    <span class="status-label">{{ slotData.status === 'available' ? 'Available' : 'Unavailable' }}</span>
  </button>
</template>

<style scoped>
.slot-card {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 0.75rem 1rem;
  border-radius: var(--radius-card, 0.5rem);
  border: 1.5px solid var(--color-border, #e2e8f0);
  background: var(--color-card, #fff);
  cursor: pointer;
  transition: border-color 0.15s, box-shadow 0.15s;
  width: 7.5rem;
  gap: 0.25rem;
}

.slot-card.available {
  border-color: var(--color-brand, #0d9488);
  color: var(--color-text-primary, #0f172a);
}

.slot-card.available:hover {
  box-shadow: var(--shadow-card, 0 1px 3px rgba(0, 0, 0, 0.08));
  border-color: var(--color-brand-hover, #0f766e);
}

.slot-card.unavailable {
  border-color: var(--color-unavailable, #cbd5e1);
  color: var(--color-text-muted, #64748b);
  cursor: not-allowed;
  background: var(--color-surface, #f8fafc);
}

.time {
  font-size: 1rem;
  font-weight: 600;
}

.status-label {
  font-size: 0.7rem;
  text-transform: uppercase;
  letter-spacing: 0.05em;
}
</style>
