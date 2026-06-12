import { SlotSchema, BookingRequestSchema, BookingResponseSchema, LocationSchema } from './schemas'
import type { Slot, BookingRequest, BookingResponse, Location } from './schemas'

const SLOT_TIMES = [
  '09:00', '09:30', '10:00', '10:30', '11:00', '11:30',
  '12:00', '12:30', '13:00', '13:30', '14:00', '14:30',
  '15:00', '15:30', '16:00', '16:30',
]

function addThirtyMinutes(time: string): string {
  const parts = time.split(':').map(Number)
  const h = parts[0] ?? 0
  const m = parts[1] ?? 0
  const total = h * 60 + m + 30
  return `${String(Math.floor(total / 60)).padStart(2, '0')}:${String(total % 60).padStart(2, '0')}`
}

function isWeekend(dateStr: string): boolean {
  const day = new Date(dateStr + 'T12:00:00').getDay()
  return day === 0 || day === 6
}

const MOCK_LOCATIONS: Location[] = [
  LocationSchema.parse({ id: 'a', slug: 'location-a', name: 'Location A' }),
  LocationSchema.parse({ id: 'b', slug: 'location-b', name: 'Location B' }),
  LocationSchema.parse({ id: 'c', slug: 'location-c', name: 'Location C' }),
]

const PRESEED: Record<string, string> = { a: '09:00', b: '10:00', c: '10:30' }

const bookedSlots = new Map<string, boolean>()

export function resetMockStore(): void {
  bookedSlots.clear()
}

export async function fetchLocations(): Promise<Location[]> {
  return MOCK_LOCATIONS
}

export async function fetchSlots(date: string, locationId: string): Promise<Slot[]> {
  if (isWeekend(date)) return []
  return SLOT_TIMES.map((startTime) => {
    const id = `${locationId}-${date}-${startTime}`
    const preseeded = PRESEED[locationId] === startTime
    return SlotSchema.parse({
      id,
      date,
      startTime,
      endTime: addThirtyMinutes(startTime),
      locationId,
      status: bookedSlots.get(id) || preseeded ? 'unavailable' : 'available',
    })
  })
}

export async function createBooking(request: BookingRequest): Promise<BookingResponse> {
  const parsed = BookingRequestSchema.parse(request)
  if (bookedSlots.get(parsed.slotId)) {
    throw new Error('Slot already booked')
  }
  bookedSlots.set(parsed.slotId, true)
  const startTime = parsed.slotId.replace(/^.+-(\d{2}:\d{2})$/, '$1')
  const date = parsed.slotId.replace(/^.+?-(\d{4}-\d{2}-\d{2})-.+$/, '$1')
  return BookingResponseSchema.parse({
    bookingId: `bk-${Date.now()}`,
    slotId: parsed.slotId,
    startTime,
    name: parsed.name,
    date,
  })
}
