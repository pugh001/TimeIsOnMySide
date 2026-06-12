import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useSlots } from '../useSlots'
import { useBookingStore } from '@/stores/bookingStore'
import * as bookingApi from '@/api/booking'

vi.mock('@/api/booking')

const mockSlots = [
  { id: 'a-2026-05-26-09:00', date: '2026-05-26', startTime: '09:00', endTime: '09:30', status: 'available' as const, locationId: 'a' },
  { id: 'a-2026-05-27-09:00', date: '2026-05-27', startTime: '09:00', endTime: '09:30', status: 'available' as const, locationId: 'a' },
]

describe('useSlots', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.mocked(bookingApi.fetchSlots).mockResolvedValue(mockSlots)
  })

  it('returns weekDates with 7 entries', () => {
    const { weekDates } = useSlots()
    expect(weekDates.value).toHaveLength(7)
  })

  it('weekDates are consecutive starting from today', () => {
    const { weekDates } = useSlots()
    const today = new Date()
    const todayStr = today.toISOString().split('T')[0]
    expect(weekDates.value[0]?.iso).toBe(todayStr)
  })

  it('selectedDate starts as today', () => {
    const { selectedDate } = useSlots()
    const todayStr = new Date().toISOString().split('T')[0]
    expect(selectedDate.value).toBe(todayStr)
  })

  it('setDate updates selectedDate', () => {
    const { selectedDate, setDate } = useSlots()
    setDate('2026-05-27')
    expect(selectedDate.value).toBe('2026-05-27')
  })

  it('slotsForDate returns slots matching selectedDate', async () => {
    const { slotsForDate, setDate } = useSlots()
    const store = useBookingStore()
    store.slots = mockSlots
    setDate('2026-05-26')
    expect(slotsForDate.value).toEqual([mockSlots[0]])
  })

  it('slotsForDate is empty when no slots match selectedDate', () => {
    const { slotsForDate, setDate } = useSlots()
    const store = useBookingStore()
    store.slots = mockSlots
    setDate('2026-06-01')
    expect(slotsForDate.value).toHaveLength(0)
  })

  it('weekDates first entry matches today when system time is fixed', () => {
    vi.useFakeTimers()
    vi.setSystemTime(new Date('2026-01-05T12:00:00Z'))
    const { weekDates } = useSlots()
    expect(weekDates.value[0]?.iso).toBe('2026-01-05')
    vi.useRealTimers()
  })
})
