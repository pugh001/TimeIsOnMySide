import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { createUser, fetchUserBookings } from '../users'

const validRequest = {
  firstName: 'Jane',
  lastName: 'Doe',
  password: 'secret99',
  locationId: 'loc-abc',
  workingTimes: [{ day: 'monday' as const, shiftStart: '09:00', shiftEnd: '17:00' }],
}

const validResponse = { userId: 'uid-001', username: 'jane1234' }

beforeEach(() => {
  vi.stubGlobal('fetch', vi.fn())
})

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('createUser', () => {
  it('calls POST /api/users with admin headers and body', async () => {
    vi.mocked(fetch).mockResolvedValueOnce(
      new Response(JSON.stringify(validResponse), { status: 201 }),
    )
    await createUser(validRequest, 'tok123', 'uid-admin')
    expect(fetch).toHaveBeenCalledWith(
      expect.stringContaining('/api/users'),
      expect.objectContaining({
        method: 'POST',
        headers: expect.objectContaining({
          'X-Admin-Token': 'tok123',
          'X-Admin-UserId': 'uid-admin',
        }),
      }),
    )
  })

  it('returns parsed response on 201', async () => {
    vi.mocked(fetch).mockResolvedValueOnce(
      new Response(JSON.stringify(validResponse), { status: 201 }),
    )
    const result = await createUser(validRequest, 'tok123', 'uid-admin')
    expect(result.userId).toBe('uid-001')
    expect(result.username).toBe('jane1234')
  })

  it('throws Unauthorized on 401', async () => {
    vi.mocked(fetch).mockResolvedValueOnce(new Response('', { status: 401 }))
    await expect(createUser(validRequest, 'bad', 'bad')).rejects.toThrow(/unauthorized/i)
  })

  it('throws "Username already taken" on 409', async () => {
    vi.mocked(fetch).mockResolvedValueOnce(
      new Response(JSON.stringify({ error: 'Username already taken' }), { status: 409 }),
    )
    await expect(createUser(validRequest, 'tok123', 'uid-admin')).rejects.toThrow(/username already taken/i)
  })
})

const mockBooking = {
  bookingRef: 'bk-abc123',
  date: '2026-12-10',
  startTime: '10:00',
  endTime: '10:30',
  customerName: 'Alice Smith',
}

describe('fetchUserBookings', () => {
  it('calls GET /api/users/{id}/bookings with admin headers', async () => {
    vi.mocked(fetch).mockResolvedValueOnce(
      new Response(JSON.stringify({ bookings: [mockBooking] }), { status: 200 }),
    )
    await fetchUserBookings('uid-999', 'tok-admin', 'uid-admin')
    expect(fetch).toHaveBeenCalledWith(
      expect.stringContaining('/api/users/uid-999/bookings'),
      expect.objectContaining({
        headers: expect.objectContaining({
          'X-Admin-Token': 'tok-admin',
          'X-Admin-UserId': 'uid-admin',
        }),
      }),
    )
  })

  it('returns parsed bookings array on 200', async () => {
    vi.mocked(fetch).mockResolvedValueOnce(
      new Response(JSON.stringify({ bookings: [mockBooking] }), { status: 200 }),
    )
    const result = await fetchUserBookings('uid-999', 'tok-admin', 'uid-admin')
    expect(result).toHaveLength(1)
    expect(result[0]!.bookingRef).toBe('bk-abc123')
    expect(result[0]!.customerName).toBe('Alice Smith')
  })

  it('returns empty array when no bookings', async () => {
    vi.mocked(fetch).mockResolvedValueOnce(
      new Response(JSON.stringify({ bookings: [] }), { status: 200 }),
    )
    const result = await fetchUserBookings('uid-999', 'tok-admin', 'uid-admin')
    expect(result).toHaveLength(0)
  })

  it('throws Unauthorized on 401', async () => {
    vi.mocked(fetch).mockResolvedValueOnce(new Response('', { status: 401 }))
    await expect(fetchUserBookings('uid-999', 'bad', 'bad')).rejects.toThrow(/unauthorized/i)
  })
})
