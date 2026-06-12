import { describe, it, expect, vi, beforeEach } from 'vitest'
import { getLocationUsers } from '../locations'

const mockFetch = vi.fn<() => unknown>()
vi.stubGlobal('fetch', mockFetch)

const locationId = 'loc-uuid-123'
const adminToken = 'tok123'
const adminUserId = 'uid-abc'

function mockResponse(overrides: object) {
  return { json: async () => ({}), ...overrides }
}

beforeEach(() => mockFetch.mockReset())

describe('getLocationUsers', () => {
  it('resolves with users array on 200', async () => {
    const users = [
      { id: 'u1', fullName: 'Jane Doe', workingTimes: [{ day: 'monday', shiftStart: '09:00', shiftEnd: '17:00' }] },
    ]
    mockFetch.mockResolvedValueOnce(mockResponse({ ok: true, status: 200, json: async () => ({ users }) }))
    const result = await getLocationUsers(locationId, adminToken, adminUserId)
    expect(result).toEqual(users)
  })

  it('sends X-Admin-Token and X-Admin-UserId headers', async () => {
    mockFetch.mockResolvedValueOnce(mockResponse({ ok: true, status: 200, json: async () => ({ users: [] }) }))
    await getLocationUsers(locationId, adminToken, adminUserId)
    expect(mockFetch).toHaveBeenCalledWith(
      expect.stringContaining(`/api/locations/${locationId}/users`),
      expect.objectContaining({
        headers: expect.objectContaining({
          'X-Admin-Token': 'tok123',
          'X-Admin-UserId': 'uid-abc',
        }),
      }),
    )
  })

  it('resolves with empty array when users is []', async () => {
    mockFetch.mockResolvedValueOnce(mockResponse({ ok: true, status: 200, json: async () => ({ users: [] }) }))
    const result = await getLocationUsers(locationId, adminToken, adminUserId)
    expect(result).toEqual([])
  })

  it('throws Unauthorized on 401', async () => {
    mockFetch.mockResolvedValueOnce(mockResponse({ ok: false, status: 401 }))
    await expect(getLocationUsers(locationId, adminToken, adminUserId)).rejects.toThrow('Unauthorized')
  })

  it('throws Location not found on 404', async () => {
    mockFetch.mockResolvedValueOnce(mockResponse({ ok: false, status: 404 }))
    await expect(getLocationUsers(locationId, adminToken, adminUserId)).rejects.toThrow('Location not found')
  })

  it('throws on non-ok status', async () => {
    mockFetch.mockResolvedValueOnce(mockResponse({ ok: false, status: 500 }))
    await expect(getLocationUsers(locationId, adminToken, adminUserId)).rejects.toThrow('getLocationUsers failed: 500')
  })

  it('throws ZodError when response shape is invalid', async () => {
    mockFetch.mockResolvedValueOnce(mockResponse({ ok: true, status: 200, json: async () => ({ wrongField: 'oops' }) }))
    await expect(getLocationUsers(locationId, adminToken, adminUserId)).rejects.toThrow(/ZodError|invalid/i)
  })
})
