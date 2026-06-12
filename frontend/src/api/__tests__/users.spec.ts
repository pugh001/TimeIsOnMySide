import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { createUser } from '../users'

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
