<script setup lang="ts">
import { ref, nextTick, onMounted, onUnmounted } from 'vue'
import { useBookingStore } from '@/stores/bookingStore'
import { useBookingForm } from '@/composables/useBookingForm'
import type { Slot } from '@/api/schemas'

const props = defineProps<{ slotData: Slot | null }>()
const emit = defineEmits<{ close: [] }>()

const store = useBookingStore()
const { name, email, phone, notes, errors, isSubmitting, submit } = useBookingForm()

const DIALOG_TITLE_ID = 'booking-modal-title'
const firstInput = ref<HTMLInputElement | null>(null)

function onKeydown(event: KeyboardEvent): void {
  if (event.key === 'Escape') emit('close')
}

onMounted(() => {
  document.addEventListener('keydown', onKeydown)
  nextTick(() => firstInput.value?.focus())
})
onUnmounted(() => document.removeEventListener('keydown', onKeydown))
</script>

<template>
  <div
    class="modal-overlay"
    role="dialog"
    aria-modal="true"
    :aria-labelledby="DIALOG_TITLE_ID"
  >
    <div class="modal">
      <header class="modal-header">
        <h2 :id="DIALOG_TITLE_ID">Book a Consultation</h2>
        <p class="slot-time">{{ slotData ? `${slotData.startTime} – ${slotData.endTime}` : '' }}</p>
      </header>

      <div v-if="store.bookingStatus === 'success'" class="success-state">
        <p>Your booking is confirmed!</p>
        <p v-if="store.bookingConfirmation" class="confirmation-detail">
          {{ store.bookingConfirmation.date }} at {{ store.bookingConfirmation.startTime }}
        </p>
        <button class="btn-primary" @click="emit('close')">Done</button>
      </div>

      <form v-else class="modal-form" @submit.prevent="submit">
        <div class="field">
          <label for="name">Name</label>
          <input
            id="name"
            ref="firstInput"
            v-model="name"
            name="name"
            type="text"
            placeholder="Jane Doe"
          />
          <span v-if="errors.name" class="field-error">{{ errors.name }}</span>
        </div>

        <div class="field">
          <label for="email">Email</label>
          <input
            id="email"
            v-model="email"
            name="email"
            type="email"
            placeholder="jane@example.com"
          />
          <span v-if="errors.email" class="field-error">{{ errors.email }}</span>
        </div>

        <div class="field">
          <label for="phone">Phone</label>
          <input id="phone" v-model="phone" name="phone" type="tel" placeholder="0831231234" />
          <span v-if="errors.phone" class="field-error">{{ errors.phone }}</span>
        </div>

        <div class="field">
          <label for="notes">Notes (optional)</label>
          <textarea
            id="notes"
            v-model="notes"
            name="notes"
            placeholder="Brief topic or question…"
            rows="3"
          />
        </div>

        <div v-if="store.bookingStatus === 'error'" class="form-error">
          {{ store.bookingError ?? 'Something went wrong. Please try again.' }}
        </div>

        <div class="modal-actions">
          <button type="button" class="cancel" @click="emit('close')">Cancel</button>
          <button type="submit" class="btn-primary" :disabled="isSubmitting">
            {{ isSubmitting ? 'Booking…' : 'Confirm Booking' }}
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(15, 23, 42, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 100;
  padding: 1rem;
}

.modal {
  background: var(--color-card, #fff);
  border-radius: var(--radius-card, 0.5rem);
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.2);
  width: 100%;
  max-width: 28rem;
  padding: 2rem;
}

.modal-header {
  margin-bottom: 1.5rem;
}

.modal-header h2 {
  font-size: 1.25rem;
  font-weight: 700;
  color: var(--color-text-primary, #0f172a);
  margin: 0 0 0.25rem;
}

.slot-time {
  font-size: 0.95rem;
  color: var(--color-brand, #0d9488);
  font-weight: 600;
  margin: 0;
}

.modal-form {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.field {
  display: flex;
  flex-direction: column;
  gap: 0.3rem;
}

label {
  font-size: 0.85rem;
  font-weight: 500;
  color: var(--color-text-primary, #0f172a);
}

input,
textarea {
  padding: 0.6rem 0.75rem;
  border: 1.5px solid var(--color-border, #e2e8f0);
  border-radius: var(--radius-card, 0.5rem);
  font-size: 0.95rem;
  color: var(--color-text-primary, #0f172a);
  background: var(--color-surface, #f8fafc);
  transition: border-color 0.15s;
}

input:focus-visible,
textarea:focus-visible {
  outline: none;
  border-color: var(--color-brand, #0d9488);
}

.field-error {
  font-size: 0.8rem;
  color: #ef4444;
}

.form-error {
  padding: 0.75rem;
  background: #fef2f2;
  border: 1px solid #fecaca;
  border-radius: var(--radius-card, 0.5rem);
  font-size: 0.875rem;
  color: #dc2626;
}

.modal-actions {
  display: flex;
  gap: 0.75rem;
  justify-content: flex-end;
  margin-top: 0.5rem;
}

.cancel {
  padding: 0.6rem 1.25rem;
  border: 1.5px solid var(--color-border, #e2e8f0);
  border-radius: var(--radius-card, 0.5rem);
  background: transparent;
  color: var(--color-text-muted, #64748b);
  cursor: pointer;
  font-size: 0.95rem;
}

.cancel:hover {
  border-color: var(--color-text-muted, #64748b);
}

.btn-primary {
  padding: 0.6rem 1.25rem;
  background: var(--color-brand, #0d9488);
  color: #fff;
  border: none;
  border-radius: var(--radius-card, 0.5rem);
  font-size: 0.95rem;
  font-weight: 600;
  cursor: pointer;
  transition: background 0.15s;
}

.btn-primary:hover:not(:disabled) {
  background: var(--color-brand-hover, #0f766e);
}

.btn-primary:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.success-state {
  text-align: center;
  padding: 1.5rem 0;
  display: flex;
  flex-direction: column;
  gap: 1rem;
  align-items: center;
}

.success-state p {
  font-size: 1.1rem;
  font-weight: 600;
  color: var(--color-brand, #0d9488);
}

.confirmation-detail {
  font-size: 0.95rem;
  font-weight: 400;
  color: var(--color-text-muted, #64748b);
}
</style>
