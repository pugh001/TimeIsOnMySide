import { ref } from 'vue'
import { useBookingStore } from '@/stores/bookingStore'
import { BookingRequestSchema } from '@/api/schemas'

type FormErrors = Partial<Record<'name' | 'email' | 'phone' | 'notes', string>>

export function useBookingForm() {
  const store = useBookingStore()

  const name = ref('')
  const email = ref('')
  const phone = ref('')
  const notes = ref('')
  const errors = ref<FormErrors>({})
  const isSubmitting = ref(false)

  function reset(): void {
    name.value = ''
    email.value = ''
    phone.value = ''
    notes.value = ''
    errors.value = {}
  }

  async function submit(): Promise<void> {
    errors.value = {}

    const result = BookingRequestSchema.safeParse({
      slotId: store.selectedSlot?.id ?? '',
      name: name.value,
      email: email.value,
      phone: phone.value,
      notes: notes.value || undefined,
    })

    if (!result.success) {
      const fieldErrors: FormErrors = {}
      const formFields = new Set<string>(['name', 'email', 'phone', 'notes'])
      for (const issue of result.error.issues) {
        const key = issue.path[0]
        if (typeof key === 'string' && formFields.has(key)) {
          fieldErrors[key as keyof FormErrors] = issue.message
        }
      }
      errors.value = fieldErrors
      return
    }

    isSubmitting.value = true
    await store.submitBooking(result.data)
    isSubmitting.value = false

    if (store.bookingStatus === 'success') {
      reset()
    }
  }

  return { name, email, phone, notes, errors, isSubmitting, submit, reset }
}
