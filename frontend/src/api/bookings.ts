import { StaffBookingsEnvelopeSchema } from './schemas'
import type { StaffBookingResponse } from './schemas'

const BASE = import.meta.env.VITE_API_BASE_URL ?? ''

export async function fetchBookings(
  staffUserId: string,
  staffToken: string,
): Promise<StaffBookingResponse[]> {
  const res = await fetch(`${BASE}/api/bookings`, {
    headers: {
      'X-Staff-UserId': staffUserId,
      'X-Staff-Token': staffToken,
    },
  })
  if (res.status === 401) throw new Error('Unauthorized — staff access required')
  if (!res.ok) throw new Error(`fetchBookings failed: ${res.status}`)
  return StaffBookingsEnvelopeSchema.parse(await res.json()).bookings
}
