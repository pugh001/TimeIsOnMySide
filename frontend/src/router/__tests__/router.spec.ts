import { describe, it, expect, beforeEach } from 'vitest'
import { createRouter, createMemoryHistory } from 'vue-router'
import { createPinia, setActivePinia } from 'pinia'
import { useAuthStore } from '@/stores/authStore'

const TODAY_UTC = new Date().toISOString().slice(0, 10)
const YESTERDAY_UTC = new Date(Date.now() - 86_400_000).toISOString().slice(0, 10)

function buildRouter() {
  return createRouter({
    history: createMemoryHistory(),
    routes: [
      { path: '/', name: 'booking', component: { template: '<div />' } },
      { path: '/login', name: 'login', component: { template: '<div />' } },
      {
        path: '/admin',
        name: 'admin',
        component: { template: '<div />' },
        meta: { requiresRole: 'admin' },
      },
      {
        path: '/locations/new',
        name: 'add-location',
        component: { template: '<div />' },
        meta: { requiresRole: 'admin' },
      },
      {
        path: '/users/new',
        name: 'add-user',
        component: { template: '<div />' },
        meta: { requiresRole: 'admin' },
      },
      {
        path: '/dashboard',
        name: 'dashboard',
        component: { template: '<div />' },
        meta: { requiresAuth: true },
      },
    ],
  })
}

function addGuard(router: ReturnType<typeof buildRouter>) {
  router.beforeEach((to) => {
    const auth = useAuthStore()
    if (to.meta.requiresRole === 'admin') {
      if (auth.role !== 'admin' || !auth.isTokenFresh) {
        auth.logout()
        return { name: 'login' }
      }
    }
    if (to.meta.requiresAuth) {
      if (!auth.isLoggedIn || !auth.isTokenFresh) {
        auth.logout()
        return { name: 'login' }
      }
    }
    if (to.name === 'booking' && auth.isLoggedIn) {
      if (auth.role === 'admin') return { name: 'admin' }
      if (auth.role === 'staff') return { name: 'dashboard' }
    }
  })
}

beforeEach(() => {
  setActivePinia(createPinia())
})

describe('router guard', () => {
  it('redirects unauthenticated user from /admin to /login', async () => {
    const router = buildRouter()
    addGuard(router)
    await router.push('/admin')
    expect(router.currentRoute.value.name).toBe('login')
  })

  it('allows admin to navigate to /admin', async () => {
    const auth = useAuthStore()
    auth.employeeName = 'Admin'
    auth.role = 'admin'
    auth.tokenDate = TODAY_UTC
    const router = buildRouter()
    addGuard(router)
    await router.push('/admin')
    expect(router.currentRoute.value.name).toBe('admin')
  })

  it('redirects non-admin staff from /admin to /login', async () => {
    const auth = useAuthStore()
    auth.employeeName = 'Jane'
    auth.role = 'staff'
    const router = buildRouter()
    addGuard(router)
    await router.push('/admin')
    expect(router.currentRoute.value.name).toBe('login')
  })

  it('redirects unauthenticated user from /locations/new to /login', async () => {
    const router = buildRouter()
    addGuard(router)
    await router.push('/locations/new')
    expect(router.currentRoute.value.name).toBe('login')
  })

  it('redirects unauthenticated user from /users/new to /login', async () => {
    const router = buildRouter()
    addGuard(router)
    await router.push('/users/new')
    expect(router.currentRoute.value.name).toBe('login')
  })

  it('redirects non-admin staff from /locations/new to /login', async () => {
    const auth = useAuthStore()
    auth.employeeName = 'Jane'
    auth.role = 'staff'
    const router = buildRouter()
    addGuard(router)
    await router.push('/locations/new')
    expect(router.currentRoute.value.name).toBe('login')
  })

  it('allows admin to navigate to /locations/new', async () => {
    const auth = useAuthStore()
    auth.employeeName = 'Admin'
    auth.role = 'admin'
    auth.tokenDate = TODAY_UTC
    const router = buildRouter()
    addGuard(router)
    await router.push('/locations/new')
    expect(router.currentRoute.value.name).toBe('add-location')
  })

  it('allows admin to navigate to /users/new', async () => {
    const auth = useAuthStore()
    auth.employeeName = 'Admin'
    auth.role = 'admin'
    auth.tokenDate = TODAY_UTC
    const router = buildRouter()
    addGuard(router)
    await router.push('/users/new')
    expect(router.currentRoute.value.name).toBe('add-user')
  })

  it('redirects unauthenticated user from /dashboard to /login', async () => {
    const router = buildRouter()
    addGuard(router)
    await router.push('/dashboard')
    expect(router.currentRoute.value.name).toBe('login')
  })

  it('allows logged-in staff to navigate to /dashboard', async () => {
    const auth = useAuthStore()
    auth.employeeName = 'Jane'
    auth.role = 'staff'
    auth.tokenDate = TODAY_UTC
    const router = buildRouter()
    addGuard(router)
    await router.push('/dashboard')
    expect(router.currentRoute.value.name).toBe('dashboard')
  })

  it('allows unauthenticated user to navigate to / (public route)', async () => {
    const router = buildRouter()
    addGuard(router)
    await router.push('/')
    expect(router.currentRoute.value.path).toBe('/')
  })

  it('redirects logged-in admin from / to /admin', async () => {
    const auth = useAuthStore()
    auth.employeeName = 'Admin'
    auth.role = 'admin'
    auth.tokenDate = TODAY_UTC
    const router = buildRouter()
    addGuard(router)
    await router.push('/')
    expect(router.currentRoute.value.name).toBe('admin')
  })

  it('redirects logged-in staff from / to /dashboard', async () => {
    const auth = useAuthStore()
    auth.employeeName = 'Jane'
    auth.role = 'staff'
    auth.tokenDate = TODAY_UTC
    const router = buildRouter()
    addGuard(router)
    await router.push('/')
    expect(router.currentRoute.value.name).toBe('dashboard')
  })

  // token freshness guards
  it('redirects admin with stale token from /admin to /login', async () => {
    const auth = useAuthStore()
    auth.employeeName = 'Admin'
    auth.role = 'admin'
    auth.adminToken = 'old-tok'
    auth.tokenDate = YESTERDAY_UTC
    const router = buildRouter()
    addGuard(router)
    await router.push('/admin')
    expect(router.currentRoute.value.name).toBe('login')
  })

  it('allows admin with fresh token to navigate to /admin', async () => {
    const auth = useAuthStore()
    auth.employeeName = 'Admin'
    auth.role = 'admin'
    auth.adminToken = 'tok'
    auth.tokenDate = TODAY_UTC
    const router = buildRouter()
    addGuard(router)
    await router.push('/admin')
    expect(router.currentRoute.value.name).toBe('admin')
  })

  it('redirects admin with stale token from /locations/new to /login', async () => {
    const auth = useAuthStore()
    auth.employeeName = 'Admin'
    auth.role = 'admin'
    auth.tokenDate = YESTERDAY_UTC
    const router = buildRouter()
    addGuard(router)
    await router.push('/locations/new')
    expect(router.currentRoute.value.name).toBe('login')
  })

  it('redirects staff with stale token from /dashboard to /login', async () => {
    const auth = useAuthStore()
    auth.employeeName = 'Jane'
    auth.role = 'staff'
    auth.tokenDate = YESTERDAY_UTC
    const router = buildRouter()
    addGuard(router)
    await router.push('/dashboard')
    expect(router.currentRoute.value.name).toBe('login')
  })
})
