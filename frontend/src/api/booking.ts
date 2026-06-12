import { z } from 'zod'
import { LocationSchema, SlotSchema, BookingRequestSchema, BookingResponseSchema } from './schemas'
import type { Location, Slot, BookingRequest, BookingResponse } from './schemas'

const BASE = import.meta.env.VITE_API_BASE_URL ?? ''

export async function fetchLocations(): Promise<Location[]> {
  const res = await fetch(`${BASE}/api/locations`)
  if (!res.ok) throw new Error(`fetchLocations failed: ${res.status}`)
  return z.object({ locations: z.array(LocationSchema) }).parse(await res.json()).locations
}

export async function fetchSlots(date: string, locationId: string): Promise<Slot[]> {
  const res = await fetch(
    `${BASE}/api/slots?date=${encodeURIComponent(date)}&locationId=${encodeURIComponent(locationId)}`,
  )
  if (!res.ok) throw new Error(`fetchSlots failed: ${res.status}`)
  return z.object({ slots: z.array(SlotSchema) }).parse(await res.json()).slots
}

export async function createBooking(request: BookingRequest): Promise<BookingResponse> {
  const res = await fetch(`${BASE}/api/bookings`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(BookingRequestSchema.parse(request)),
  })
  if (res.status === 409) throw new Error('This slot is no longer available.')
  if (res.status === 401) throw new Error('Booking is not available. Please try again.')
  if (!res.ok) throw new Error('Booking failed. Please try again later.')
  return BookingResponseSchema.parse(await res.json())
}
