import { describe, it, expect, vi, beforeEach } from 'vitest'
import { loginUser } from '../auth'

const mockFetch = vi.fn<() => unknown>()
vi.stubGlobal('fetch', mockFetch)

beforeEach(() => {
  mockFetch.mockReset()
})

describe('loginUser', () => {
  it('resolves with employeeName and role on 200', async () => {
    mockFetch.mockResolvedValueOnce({
      ok: true,
      json: async () => ({ employeeName: 'Jason Pugh', role: 'admin' }),
    })

    const result = await loginUser('jason', 'Pass1!')
    expect(result).toMatchObject({ employeeName: 'Jason Pugh', role: 'admin' })
    expect(mockFetch).toHaveBeenCalledWith(
      expect.stringContaining('/api/auth/login'),
      expect.objectContaining({ method: 'POST' }),
    )
  })

  it('rejects with error message on 401', async () => {
    mockFetch.mockResolvedValueOnce({
      ok: false,
      status: 401,
      json: async () => ({ error: 'Invalid credentials' }),
    })

    await expect(loginUser('jason', 'wrong')).rejects.toThrow('Invalid credentials')
  })

  it('rejects with fallback message when error field is absent', async () => {
    mockFetch.mockResolvedValueOnce({
      ok: false,
      status: 500,
      json: async () => ({}),
    })

    await expect(loginUser('jason', 'Pass1!')).rejects.toThrow('Login failed')
  })

  it('includes adminToken and adminUserId for admin role', async () => {
    mockFetch.mockResolvedValueOnce({
      ok: true,
      json: async () => ({
        employeeName: 'Jason Pugh',
        role: 'admin',
        adminToken: 'tok123',
        adminUserId: 'uid-abc',
      }),
    })

    const result = await loginUser('jason', 'Pass1!')
    expect(result.adminToken).toBe('tok123')
    expect(result.adminUserId).toBe('uid-abc')
  })

  it('adminToken and adminUserId are absent for staff role', async () => {
    mockFetch.mockResolvedValueOnce({
      ok: true,
      json: async () => ({ employeeName: 'Jane', role: 'staff' }),
    })

    const result = await loginUser('jane', 'Pass1!')
    expect(result.adminToken).toBeUndefined()
    expect(result.adminUserId).toBeUndefined()
  })

  it('throws a ZodError when response shape is invalid', async () => {
    mockFetch.mockResolvedValueOnce({
      ok: true,
      json: async () => ({ wrongField: 'oops' }),
    })

    await expect(loginUser('jason', 'Pass1!')).rejects.toThrow(/ZodError|invalid/i)
  })
})
