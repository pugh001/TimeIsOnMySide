import { describe, it, expect, vi, beforeEach } from 'vitest'
import { createLocation } from '../locations'

const mockFetch = vi.fn<() => unknown>()
vi.stubGlobal('fetch', mockFetch)

const validRequest = {
  name: 'Main Branch',
  address: '123 Main St',
}

const adminToken = 'tok123'
const adminUserId = 'uid-abc'

function mockResponse(overrides: object) {
  return { json: async () => ({}), ...overrides }
}

beforeEach(() => {
  mockFetch.mockReset()
})

describe('createLocation', () => {
  it('resolves with slug and id on 201', async () => {
    mockFetch.mockResolvedValueOnce(mockResponse({ ok: true, status: 201, json: async () => ({ slug: 'main-branch', id: 'guid-1' }) }))
    const result = await createLocation(validRequest, adminToken, adminUserId)
    expect(result).toEqual({ slug: 'main-branch', id: 'guid-1' })
  })

  it('sends X-Admin-Token and X-Admin-UserId headers', async () => {
    mockFetch.mockResolvedValueOnce(mockResponse({ ok: true, status: 201, json: async () => ({ slug: 'main-branch', id: 'guid-1' }) }))
    await createLocation(validRequest, adminToken, adminUserId)
    expect(mockFetch).toHaveBeenCalledWith(
      expect.stringContaining('/api/locations'),
      expect.objectContaining({
        method: 'POST',
        headers: expect.objectContaining({
          'X-Admin-Token': 'tok123',
          'X-Admin-UserId': 'uid-abc',
        }),
      }),
    )
  })

  it('throws Unauthorized on 401', async () => {
    mockFetch.mockResolvedValueOnce(mockResponse({ ok: false, status: 401 }))
    await expect(createLocation(validRequest, adminToken, adminUserId)).rejects.toThrow('Unauthorized')
  })

  it('throws validation error message on 422', async () => {
    mockFetch.mockResolvedValueOnce(mockResponse({ ok: false, status: 422, json: async () => ({ error: 'Name too short' }) }))
    await expect(createLocation(validRequest, adminToken, adminUserId)).rejects.toThrow('Name too short')
  })

  it('throws generic error on other non-ok status', async () => {
    mockFetch.mockResolvedValueOnce(mockResponse({ ok: false, status: 500 }))
    await expect(createLocation(validRequest, adminToken, adminUserId)).rejects.toThrow('createLocation failed: 500')
  })

  it('throws ZodError when response shape is invalid', async () => {
    mockFetch.mockResolvedValueOnce(mockResponse({ ok: true, status: 201, json: async () => ({ wrongField: 'oops' }) }))
    await expect(createLocation(validRequest, adminToken, adminUserId)).rejects.toThrow(/ZodError|invalid/i)
  })

  it('includes openingHours in request body when provided', async () => {
    mockFetch.mockResolvedValueOnce(mockResponse({ ok: true, status: 201, json: async () => ({ slug: 'main-branch', id: 'guid-1' }) }))

    await createLocation(
      { ...validRequest, openingHours: { monday: { openTime: '08:00', closeTime: '17:00' } } },
      adminToken,
      adminUserId,
    )

    const [, init] = mockFetch.mock.calls[0] as unknown as [string, RequestInit]
    const body = JSON.parse(init.body as string)
    expect(body.openingHours.monday).toEqual({ openTime: '08:00', closeTime: '17:00' })
  })
})
