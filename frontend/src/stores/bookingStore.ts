import { defineStore } from 'pinia'
import { ref } from 'vue'
import { fetchSlots, createBooking, fetchLocations } from '@/api/booking'
import type { Slot, BookingRequest, BookingResponse, Location } from '@/api/schemas'

export const useBookingStore = defineStore('booking', () => {
  const slots = ref<Slot[]>([])
  const selectedSlot = ref<Slot | null>(null)
  const locations = ref<Location[]>([])
  const selectedLocation = ref<Location | null>(null)
  const bookingConfirmation = ref<BookingResponse | null>(null)

  const locationsStatus = ref<'idle' | 'loading' | 'error'>('idle')
  const locationsError = ref<string | null>(null)
  const slotsStatus = ref<'idle' | 'loading' | 'error'>('idle')
  const slotsError = ref<string | null>(null)
  const bookingStatus = ref<'idle' | 'loading' | 'success' | 'error'>('idle')
  const bookingError = ref<string | null>(null)

  async function loadLocations(): Promise<void> {
    locationsStatus.value = 'loading'
    locationsError.value = null
    try {
      locations.value = await fetchLocations()
      locationsStatus.value = 'idle'
    } catch (err) {
      locationsStatus.value = 'error'
      locationsError.value = err instanceof Error ? err.message : 'An unknown error occurred'
    }
  }

  function selectLocation(loc: Location): void {
    selectedLocation.value = loc
    slots.value = []
  }

  async function loadSlots(date: string, locationId: string): Promise<void> {
    slotsStatus.value = 'loading'
    slotsError.value = null
    try {
      slots.value = await fetchSlots(date, locationId)
      slotsStatus.value = 'idle'
    } catch (err) {
      slotsStatus.value = 'error'
      slotsError.value = err instanceof Error ? err.message : 'An unknown error occurred'
    }
  }

  function resetStatus(): void {
    bookingStatus.value = 'idle'
  }

  function selectSlot(slot: Slot): void {
    selectedSlot.value = slot
  }

  function clearSelectedSlot(): void {
    selectedSlot.value = null
  }

  async function submitBooking(request: BookingRequest): Promise<void> {
    bookingStatus.value = 'loading'
    bookingError.value = null
    try {
      bookingConfirmation.value = await createBooking(request)
      bookingStatus.value = 'success'
    } catch (err) {
      bookingStatus.value = 'error'
      bookingError.value = err instanceof Error ? err.message : 'An unknown error occurred'
    }
  }

  return {
    slots, selectedSlot, locations, selectedLocation, bookingConfirmation,
    locationsStatus, locationsError, slotsStatus, slotsError, bookingStatus, bookingError,
    loadLocations, selectLocation,
    loadSlots, selectSlot, clearSelectedSlot, submitBooking, resetStatus,
  }
})
