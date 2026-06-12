import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { fetchBookings } from '../bookings'

const validBooking = {
  bookingRef: 'bk-001',
  date: '2026-06-10',
  startTime: '09:00',
  endTime: '09:30',
  customerName: 'Alice Smith',
}

beforeEach(() => {
  vi.stubGlobal('fetch', vi.fn())
})

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('fetchBookings', () => {
  it('calls GET /api/bookings with staff headers', async () => {
    vi.mocked(fetch).mockResolvedValueOnce(
      new Response(JSON.stringify({ bookings: [validBooking] }), { status: 200 }),
    )
    await fetchBookings('uid-staff', 'tok-staff')
    expect(fetch).toHaveBeenCalledWith(
      expect.stringContaining('/api/bookings'),
      expect.objectContaining({
        headers: expect.objectContaining({
          'X-Staff-UserId': 'uid-staff',
          'X-Staff-Token': 'tok-staff',
        }),
      }),
    )
  })

  it('resolves with parsed bookings array on 200', async () => {
    vi.mocked(fetch).mockResolvedValueOnce(
      new Response(JSON.stringify({ bookings: [validBooking] }), { status: 200 }),
    )
    const result = await fetchBookings('uid-staff', 'tok-staff')
    expect(result).toHaveLength(1)
    expect(result[0]!.bookingRef).toBe('bk-001')
    expect(result[0]!.customerName).toBe('Alice Smith')
  })

  it('resolves with empty array when bookings is []', async () => {
    vi.mocked(fetch).mockResolvedValueOnce(
      new Response(JSON.stringify({ bookings: [] }), { status: 200 }),
    )
    const result = await fetchBookings('uid-staff', 'tok-staff')
    expect(result).toEqual([])
  })

  it('throws Unauthorized on 401', async () => {
    vi.mocked(fetch).mockResolvedValueOnce(new Response('', { status: 401 }))
    await expect(fetchBookings('uid-staff', 'bad-tok')).rejects.toThrow(/unauthorized/i)
  })

  it('throws on non-ok status', async () => {
    vi.mocked(fetch).mockResolvedValueOnce(new Response('', { status: 500 }))
    await expect(fetchBookings('uid-staff', 'tok-staff')).rejects.toThrow()
  })

  it('throws ZodError when response shape is invalid', async () => {
    vi.mocked(fetch).mockResolvedValueOnce(
      new Response(JSON.stringify({ bookings: [{ bad: 'shape' }] }), { status: 200 }),
    )
    await expect(fetchBookings('uid-staff', 'tok-staff')).rejects.toThrow()
  })
})
