import { describe, it, expect, vi, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useAuthStore } from '../authStore'
import * as authApi from '@/api/auth'

vi.mock('@/api/auth')

const TODAY_UTC = new Date().toISOString().slice(0, 10)
const YESTERDAY_UTC = new Date(Date.now() - 86_400_000).toISOString().slice(0, 10)

beforeEach(() => {
  sessionStorage.clear()
  setActivePinia(createPinia())
  vi.mocked(authApi.loginUser).mockReset()
})

describe('authStore', () => {
  it('isLoggedIn is false initially', () => {
    const store = useAuthStore()
    expect(store.isLoggedIn).toBe(false)
    expect(store.employeeName).toBeNull()
    expect(store.role).toBeNull()
    expect(store.adminToken).toBeNull()
    expect(store.adminUserId).toBeNull()
    expect(store.staffToken).toBeNull()
    expect(store.staffUserId).toBeNull()
  })

  it('login() sets employeeName and role on success', async () => {
    vi.mocked(authApi.loginUser).mockResolvedValueOnce({ employeeName: 'Jason Pugh', role: 'admin' })
    const store = useAuthStore()
    await store.login('jason', 'Pass1!')
    expect(store.employeeName).toBe('Jason Pugh')
    expect(store.role).toBe('admin')
    expect(store.isLoggedIn).toBe(true)
  })

  it('login() stores adminToken and adminUserId for admin', async () => {
    vi.mocked(authApi.loginUser).mockResolvedValueOnce({
      employeeName: 'Jason Pugh',
      role: 'admin',
      adminToken: 'tok123',
      adminUserId: 'uid-abc',
    })
    const store = useAuthStore()
    await store.login('jason', 'Pass1!')
    expect(store.adminToken).toBe('tok123')
    expect(store.adminUserId).toBe('uid-abc')
  })

  it('login() stores staffToken and staffUserId for staff', async () => {
    vi.mocked(authApi.loginUser).mockResolvedValueOnce({
      employeeName: 'Jane',
      role: 'staff',
      staffToken: 'staff-tok',
      staffUserId: 'staff-uid',
    })
    const store = useAuthStore()
    await store.login('jane', 'Pass1!')
    expect(store.staffToken).toBe('staff-tok')
    expect(store.staffUserId).toBe('staff-uid')
    expect(store.adminToken).toBeNull()
    expect(store.adminUserId).toBeNull()
  })

  it('login() leaves staffToken and staffUserId null for admin', async () => {
    vi.mocked(authApi.loginUser).mockResolvedValueOnce({
      employeeName: 'Jason Pugh',
      role: 'admin',
      adminToken: 'tok123',
      adminUserId: 'uid-abc',
    })
    const store = useAuthStore()
    await store.login('jason', 'Pass1!')
    expect(store.staffToken).toBeNull()
    expect(store.staffUserId).toBeNull()
  })

  it('login() calls loginUser with username and password', async () => {
    vi.mocked(authApi.loginUser).mockResolvedValueOnce({ employeeName: 'Jason Pugh', role: 'staff' })
    const store = useAuthStore()
    await store.login('jason', 'Pass1!')
    expect(authApi.loginUser).toHaveBeenCalledWith('jason', 'Pass1!')
  })

  it('login() throws and leaves employeeName null on API error', async () => {
    vi.mocked(authApi.loginUser).mockRejectedValueOnce(new Error('Invalid credentials'))
    const store = useAuthStore()
    await expect(store.login('jason', 'wrong')).rejects.toThrow('Invalid credentials')
    expect(store.employeeName).toBeNull()
    expect(store.role).toBeNull()
    expect(store.isLoggedIn).toBe(false)
  })

  it('logout() clears all auth state', async () => {
    vi.mocked(authApi.loginUser).mockResolvedValueOnce({
      employeeName: 'Jane',
      role: 'staff',
      staffToken: 'staff-tok',
      staffUserId: 'staff-uid',
    })
    const store = useAuthStore()
    await store.login('jane', 'Pass1!')
    store.logout()
    expect(store.employeeName).toBeNull()
    expect(store.role).toBeNull()
    expect(store.adminToken).toBeNull()
    expect(store.adminUserId).toBeNull()
    expect(store.staffToken).toBeNull()
    expect(store.staffUserId).toBeNull()
    expect(store.isLoggedIn).toBe(false)
  })

  it('login() persists admin auth to sessionStorage', async () => {
    vi.mocked(authApi.loginUser).mockResolvedValueOnce({
      employeeName: 'Jason Pugh',
      role: 'admin',
      adminToken: 'tok123',
      adminUserId: 'uid-abc',
    })
    const store = useAuthStore()
    await store.login('jason', 'Pass1!')
    const saved = JSON.parse(sessionStorage.getItem('auth') ?? 'null')
    expect(saved).toMatchObject({
      employeeName: 'Jason Pugh',
      role: 'admin',
      adminToken: 'tok123',
      adminUserId: 'uid-abc',
      staffToken: null,
      staffUserId: null,
    })
  })

  it('login() persists staffToken and staffUserId to sessionStorage', async () => {
    vi.mocked(authApi.loginUser).mockResolvedValueOnce({
      employeeName: 'Jane',
      role: 'staff',
      staffToken: 'staff-tok',
      staffUserId: 'staff-uid',
    })
    const store = useAuthStore()
    await store.login('jane', 'Pass1!')
    const saved = JSON.parse(sessionStorage.getItem('auth') ?? 'null')
    expect(saved).toMatchObject({
      employeeName: 'Jane',
      role: 'staff',
      staffToken: 'staff-tok',
      staffUserId: 'staff-uid',
    })
  })

  it('store restores staffToken and staffUserId from sessionStorage', () => {
    sessionStorage.setItem('auth', JSON.stringify({
      employeeName: 'Jane',
      role: 'staff',
      adminToken: null,
      adminUserId: null,
      staffToken: 'staff-tok',
      staffUserId: 'staff-uid',
      tokenDate: TODAY_UTC,
    }))
    setActivePinia(createPinia())
    const store = useAuthStore()
    expect(store.staffToken).toBe('staff-tok')
    expect(store.staffUserId).toBe('staff-uid')
  })

  it('logout() removes auth from sessionStorage', async () => {
    vi.mocked(authApi.loginUser).mockResolvedValueOnce({
      employeeName: 'Jason Pugh',
      role: 'admin',
      adminToken: 'tok123',
      adminUserId: 'uid-abc',
    })
    const store = useAuthStore()
    await store.login('jason', 'Pass1!')
    store.logout()
    expect(sessionStorage.getItem('auth')).toBeNull()
  })

  it('store restores state from sessionStorage on init', () => {
    sessionStorage.setItem('auth', JSON.stringify({
      employeeName: 'Jane',
      role: 'staff',
      adminToken: null,
      adminUserId: null,
      tokenDate: TODAY_UTC,
    }))
    setActivePinia(createPinia())
    const store = useAuthStore()
    expect(store.employeeName).toBe('Jane')
    expect(store.role).toBe('staff')
    expect(store.isLoggedIn).toBe(true)
  })

  it('store starts empty when sessionStorage has no auth entry', () => {
    setActivePinia(createPinia())
    const store = useAuthStore()
    expect(store.isLoggedIn).toBe(false)
    expect(store.employeeName).toBeNull()
  })

  // tokenDate / isTokenFresh
  it('isTokenFresh is false when tokenDate is yesterday', () => {
    sessionStorage.setItem('auth', JSON.stringify({
      employeeName: 'Admin',
      role: 'admin',
      adminToken: 'tok',
      adminUserId: 'uid',
      staffToken: null,
      staffUserId: null,
      tokenDate: YESTERDAY_UTC,
    }))
    setActivePinia(createPinia())
    const store = useAuthStore()
    expect(store.isTokenFresh).toBe(false)
  })

  it('isTokenFresh is true when tokenDate is today', () => {
    sessionStorage.setItem('auth', JSON.stringify({
      employeeName: 'Admin',
      role: 'admin',
      adminToken: 'tok',
      adminUserId: 'uid',
      staffToken: null,
      staffUserId: null,
      tokenDate: TODAY_UTC,
    }))
    setActivePinia(createPinia())
    const store = useAuthStore()
    expect(store.isTokenFresh).toBe(true)
  })

  it('isTokenFresh is false when tokenDate is null', () => {
    sessionStorage.setItem('auth', JSON.stringify({
      employeeName: 'Admin',
      role: 'admin',
      adminToken: 'tok',
      adminUserId: 'uid',
      staffToken: null,
      staffUserId: null,
      tokenDate: null,
    }))
    setActivePinia(createPinia())
    const store = useAuthStore()
    expect(store.isTokenFresh).toBe(false)
  })

  it('login() stores tokenDate as today UTC in store and sessionStorage', async () => {
    vi.mocked(authApi.loginUser).mockResolvedValueOnce({
      employeeName: 'Jason Pugh',
      role: 'admin',
      adminToken: 'tok123',
      adminUserId: 'uid-abc',
    })
    const store = useAuthStore()
    await store.login('jason', 'Pass1!')
    expect(store.tokenDate).toBe(TODAY_UTC)
    const saved = JSON.parse(sessionStorage.getItem('auth') ?? 'null')
    expect(saved.tokenDate).toBe(TODAY_UTC)
  })

  it('store auto-logs-out on hydration when tokenDate is stale', () => {
    sessionStorage.setItem('auth', JSON.stringify({
      employeeName: 'Admin',
      role: 'admin',
      adminToken: 'tok',
      adminUserId: 'uid',
      staffToken: null,
      staffUserId: null,
      tokenDate: YESTERDAY_UTC,
    }))
    setActivePinia(createPinia())
    const store = useAuthStore()
    expect(store.isLoggedIn).toBe(false)
    expect(store.employeeName).toBeNull()
    expect(sessionStorage.getItem('auth')).toBeNull()
  })

  it('logout() clears tokenDate', async () => {
    vi.mocked(authApi.loginUser).mockResolvedValueOnce({
      employeeName: 'Jason Pugh',
      role: 'admin',
      adminToken: 'tok123',
      adminUserId: 'uid-abc',
    })
    const store = useAuthStore()
    await store.login('jason', 'Pass1!')
    store.logout()
    expect(store.tokenDate).toBeNull()
  })
})
