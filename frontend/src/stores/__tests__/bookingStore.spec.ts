import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useBookingStore } from '../bookingStore'
import * as bookingApi from '@/api/booking'

vi.mock('@/api/booking')

const mockLocations = [
  { id: 'a', slug: 'location-a', name: 'Location A' },
  { id: 'b', slug: 'location-b', name: 'Location B' },
  { id: 'c', slug: 'location-c', name: 'Location C' },
]

const mockSlots = [
  { id: 'a-2026-05-26-09:00', date: '2026-05-26', startTime: '09:00', endTime: '09:30', status: 'available' as const, locationId: 'a' },
  { id: 'a-2026-05-26-09:30', date: '2026-05-26', startTime: '09:30', endTime: '10:00', status: 'available' as const, locationId: 'a' },
]

describe('useBookingStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.mocked(bookingApi.fetchLocations).mockResolvedValue(mockLocations)
    vi.mocked(bookingApi.fetchSlots).mockResolvedValue(mockSlots)
    vi.mocked(bookingApi.createBooking).mockResolvedValue({
      bookingId: 'bk-001',
      slotId: 'a-2026-05-26-09:00',
      startTime: '09:00',
      name: 'Jane',
      date: '2026-05-26',
    })
  })

  it('has correct initial state', () => {
    const store = useBookingStore()
    expect(store.slots).toEqual([])
    expect(store.selectedSlot).toBeNull()
    expect(store.slotsStatus).toBe('idle')
    expect(store.slotsError).toBeNull()
    expect(store.bookingStatus).toBe('idle')
    expect(store.bookingError).toBeNull()
    expect(store.locations).toEqual([])
    expect(store.selectedLocation).toBeNull()
  })

  it('loadLocations populates locations array', async () => {
    const store = useBookingStore()
    await store.loadLocations()
    expect(store.locations).toEqual(mockLocations)
  })

  it('selectLocation sets selectedLocation', () => {
    const store = useBookingStore()
    store.selectLocation(mockLocations[0]!)
    expect(store.selectedLocation).toEqual(mockLocations[0])
  })

  it('selectLocation clears slots', async () => {
    const store = useBookingStore()
    await store.loadSlots('2026-05-26', 'a')
    expect(store.slots).toEqual(mockSlots)
    store.selectLocation(mockLocations[1]!)
    expect(store.slots).toEqual([])
  })

  it('loadSlots passes locationId to fetchSlots', async () => {
    const store = useBookingStore()
    await store.loadSlots('2026-05-26', 'a')
    expect(bookingApi.fetchSlots).toHaveBeenCalledWith('2026-05-26', 'a')
  })

  it('loadSlots populates slots array', async () => {
    const store = useBookingStore()
    await store.loadSlots('2026-05-26', 'a')
    expect(store.slots).toEqual(mockSlots)
  })

  it('loadSlots sets slotsStatus to idle after completion', async () => {
    const store = useBookingStore()
    await store.loadSlots('2026-05-26', 'a')
    expect(store.slotsStatus).toBe('idle')
  })

  it('selectSlot sets selectedSlot', () => {
    const store = useBookingStore()
    store.selectSlot(mockSlots[0]!)
    expect(store.selectedSlot).toEqual(mockSlots[0])
  })

  it('clearSelectedSlot sets selectedSlot to null', () => {
    const store = useBookingStore()
    store.selectSlot(mockSlots[0]!)
    store.clearSelectedSlot()
    expect(store.selectedSlot).toBeNull()
  })

  it('submitBooking transitions bookingStatus to success on success', async () => {
    const store = useBookingStore()
    await store.loadSlots('2026-05-26', 'a')
    store.selectSlot(mockSlots[0]!)
    await store.submitBooking({ slotId: 'a-2026-05-26-09:00', name: 'Jane', email: 'jane@example.com', phone: '123' })
    expect(store.bookingStatus).toBe('success')
  })

  it('submitBooking transitions bookingStatus to error on failure', async () => {
    vi.mocked(bookingApi.createBooking).mockRejectedValue(new Error('Slot already booked'))
    const store = useBookingStore()
    await store.loadSlots('2026-05-26', 'a')
    store.selectSlot(mockSlots[0]!)
    await store.submitBooking({ slotId: 'a-2026-05-26-09:00', name: 'Jane', email: 'jane@example.com', phone: '123' })
    expect(store.bookingStatus).toBe('error')
    expect(store.bookingError).toBe('Slot already booked')
  })

  it('submitBooking calls createBooking API with the request', async () => {
    const store = useBookingStore()
    await store.loadSlots('2026-05-26', 'a')
    const req = { slotId: 'a-2026-05-26-09:00', name: 'Jane', email: 'jane@example.com', phone: '123' }
    await store.submitBooking(req)
    expect(bookingApi.createBooking).toHaveBeenCalledWith(req)
  })

  it('loadLocations sets locationsStatus to error on fetch failure', async () => {
    vi.mocked(bookingApi.fetchLocations).mockRejectedValue(new Error('Network error'))
    const store = useBookingStore()
    await store.loadLocations()
    expect(store.locationsStatus).toBe('error')
  })

  it('loadLocations does not leave locationsStatus as loading on failure', async () => {
    vi.mocked(bookingApi.fetchLocations).mockRejectedValue(new Error('Network error'))
    const store = useBookingStore()
    await store.loadLocations()
    expect(store.locationsStatus).not.toBe('loading')
  })

  it('loadLocations captures the error message in locationsError on failure', async () => {
    vi.mocked(bookingApi.fetchLocations).mockRejectedValue(new Error('Network error'))
    const store = useBookingStore()
    await store.loadLocations()
    expect(store.locationsError).toBe('Network error')
  })

  it('loadLocations resets locationsError to null on success', async () => {
    vi.mocked(bookingApi.fetchLocations).mockRejectedValue(new Error('first error'))
    const store = useBookingStore()
    await store.loadLocations()
    vi.mocked(bookingApi.fetchLocations).mockResolvedValue(mockLocations)
    await store.loadLocations()
    expect(store.locationsError).toBeNull()
  })

  it('loadSlots sets slotsStatus to error on fetch failure', async () => {
    vi.mocked(bookingApi.fetchSlots).mockRejectedValue(new Error('Slots unavailable'))
    const store = useBookingStore()
    await store.loadSlots('2026-05-26', 'a')
    expect(store.slotsStatus).toBe('error')
    expect(store.slotsError).toBe('Slots unavailable')
  })

  it('loadSlots does not leave slotsStatus as loading on failure', async () => {
    vi.mocked(bookingApi.fetchSlots).mockRejectedValue(new Error('Slots unavailable'))
    const store = useBookingStore()
    await store.loadSlots('2026-05-26', 'a')
    expect(store.slotsStatus).not.toBe('loading')
  })

  it('resetStatus sets bookingStatus to idle', () => {
    const store = useBookingStore()
    store.bookingStatus = 'success'
    store.resetStatus()
    expect(store.bookingStatus).toBe('idle')
  })
})
