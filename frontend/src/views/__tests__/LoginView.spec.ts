import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import { setActivePinia, createPinia } from 'pinia'
import { createRouter, createWebHistory } from 'vue-router'
import LoginView from '../LoginView.vue'
import { useAuthStore } from '@/stores/authStore'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', component: { template: '<div />' } },
    { path: '/login', component: { template: '<div />' } },
    { path: '/admin', component: { template: '<div />' } },
    { path: '/dashboard', component: { template: '<div />' } },
  ],
})

function mountView() {
  return mount(LoginView, {
    global: { plugins: [router, createPinia()] },
  })
}

beforeEach(() => {
  setActivePinia(createPinia())
  vi.restoreAllMocks()
})

describe('LoginView', () => {
  it('renders a username input', () => {
    const wrapper = mountView()
    expect(wrapper.find('[data-testid="username-input"]').exists()).toBe(true)
  })

  it('renders a password input', () => {
    const wrapper = mountView()
    expect(wrapper.find('[data-testid="password-input"]').exists()).toBe(true)
  })

  it('password input type is password by default', () => {
    const wrapper = mountView()
    expect(wrapper.find('[data-testid="password-input"]').attributes('type')).toBe('password')
  })

  it('toggle button switches password to text', async () => {
    const wrapper = mountView()
    await wrapper.find('[data-testid="password-toggle"]').trigger('click')
    expect(wrapper.find('[data-testid="password-input"]').attributes('type')).toBe('text')
  })

  it('toggle button switches text back to password', async () => {
    const wrapper = mountView()
    await wrapper.find('[data-testid="password-toggle"]').trigger('click')
    await wrapper.find('[data-testid="password-toggle"]').trigger('click')
    expect(wrapper.find('[data-testid="password-input"]').attributes('type')).toBe('password')
  })

  it('shows error when username is empty on submit', async () => {
    const wrapper = mountView()
    await wrapper.find('[data-testid="login-form"]').trigger('submit')
    expect(wrapper.find('[data-testid="username-error"]').text()).toBeTruthy()
  })

  it('accepts generated username like ewr0001 without error', async () => {
    const pinia = createPinia()
    setActivePinia(pinia)
    const store = useAuthStore()
    const spy = vi.spyOn(store, 'login').mockResolvedValueOnce()
    const wrapper = mount(LoginView, { global: { plugins: [router, pinia] } })
    await wrapper.find('[data-testid="username-input"]').setValue('ewr0001')
    await wrapper.find('[data-testid="password-input"]').setValue('Pass1!')
    await wrapper.find('[data-testid="login-form"]').trigger('submit')
    await flushPromises()
    expect(wrapper.find('[data-testid="username-error"]').exists()).toBe(false)
    expect(spy).toHaveBeenCalledWith('ewr0001', 'Pass1!')
  })

  it('accepts long generated username like christopher0001 without error', async () => {
    const pinia = createPinia()
    setActivePinia(pinia)
    const store = useAuthStore()
    const spy = vi.spyOn(store, 'login').mockResolvedValueOnce()
    const wrapper = mount(LoginView, { global: { plugins: [router, pinia] } })
    await wrapper.find('[data-testid="username-input"]').setValue('christopher0001')
    await wrapper.find('[data-testid="password-input"]').setValue('Pass1!')
    await wrapper.find('[data-testid="login-form"]').trigger('submit')
    await flushPromises()
    expect(wrapper.find('[data-testid="username-error"]').exists()).toBe(false)
    expect(spy).toHaveBeenCalledWith('christopher0001', 'Pass1!')
  })

  it('accepts admin username without error', async () => {
    const pinia = createPinia()
    setActivePinia(pinia)
    const store = useAuthStore()
    const spy = vi.spyOn(store, 'login').mockResolvedValueOnce()
    const wrapper = mount(LoginView, { global: { plugins: [router, pinia] } })
    await wrapper.find('[data-testid="username-input"]').setValue('admin')
    await wrapper.find('[data-testid="password-input"]').setValue('Pass1!')
    await wrapper.find('[data-testid="login-form"]').trigger('submit')
    await flushPromises()
    expect(wrapper.find('[data-testid="username-error"]').exists()).toBe(false)
    expect(spy).toHaveBeenCalledWith('admin', 'Pass1!')
  })

  it('shows error when password has no uppercase', async () => {
    const wrapper = mountView()
    await wrapper.find('[data-testid="username-input"]').setValue('jason')
    await wrapper.find('[data-testid="password-input"]').setValue('pass1!')
    await wrapper.find('[data-testid="login-form"]').trigger('submit')
    expect(wrapper.find('[data-testid="password-error"]').text()).toBeTruthy()
  })

  it('shows error when password has no lowercase', async () => {
    const wrapper = mountView()
    await wrapper.find('[data-testid="username-input"]').setValue('jason')
    await wrapper.find('[data-testid="password-input"]').setValue('PASS1!')
    await wrapper.find('[data-testid="login-form"]').trigger('submit')
    expect(wrapper.find('[data-testid="password-error"]').text()).toBeTruthy()
  })

  it('shows error when password has no digit', async () => {
    const wrapper = mountView()
    await wrapper.find('[data-testid="username-input"]').setValue('jason')
    await wrapper.find('[data-testid="password-input"]').setValue('Password!')
    await wrapper.find('[data-testid="login-form"]').trigger('submit')
    expect(wrapper.find('[data-testid="password-error"]').text()).toBeTruthy()
  })

  it('shows error when password has no special character', async () => {
    const wrapper = mountView()
    await wrapper.find('[data-testid="username-input"]').setValue('jason')
    await wrapper.find('[data-testid="password-input"]').setValue('Password1')
    await wrapper.find('[data-testid="login-form"]').trigger('submit')
    expect(wrapper.find('[data-testid="password-error"]').text()).toBeTruthy()
  })

  it('calls authStore.login on valid submit', async () => {
    const pinia = createPinia()
    setActivePinia(pinia)
    const store = useAuthStore()
    const spy = vi.spyOn(store, 'login').mockResolvedValueOnce()
    const wrapper = mount(LoginView, { global: { plugins: [router, pinia] } })
    await wrapper.find('[data-testid="username-input"]').setValue('jason')
    await wrapper.find('[data-testid="password-input"]').setValue('Pass1!')
    await wrapper.find('[data-testid="login-form"]').trigger('submit')
    await flushPromises()
    expect(spy).toHaveBeenCalledWith('jason', 'Pass1!')
  })

  it('redirects to /admin on successful admin login', async () => {
    const pinia = createPinia()
    setActivePinia(pinia)
    const store = useAuthStore()
    vi.spyOn(store, 'login').mockImplementationOnce(async () => {
      store.employeeName = 'Admin'
      store.role = 'admin'
    })
    const wrapper = mount(LoginView, { global: { plugins: [router, pinia] } })
    await wrapper.find('[data-testid="username-input"]').setValue('jason')
    await wrapper.find('[data-testid="password-input"]').setValue('Pass1!')
    await wrapper.find('[data-testid="login-form"]').trigger('submit')
    await flushPromises()
    expect(router.currentRoute.value.path).toBe('/admin')
  })

  it('redirects to /dashboard after staff login', async () => {
    const pinia = createPinia()
    setActivePinia(pinia)
    const store = useAuthStore()
    vi.spyOn(store, 'login').mockImplementationOnce(async () => {
      store.employeeName = 'Jane'
      store.role = 'staff'
    })
    const wrapper = mount(LoginView, { global: { plugins: [router, pinia] } })
    await wrapper.find('[data-testid="username-input"]').setValue('jane0001')
    await wrapper.find('[data-testid="password-input"]').setValue('Pass1!')
    await wrapper.find('[data-testid="login-form"]').trigger('submit')
    await flushPromises()
    expect(router.currentRoute.value.path).toBe('/dashboard')
  })

  it('shows API error message on failed login', async () => {
    const pinia = createPinia()
    setActivePinia(pinia)
    const store = useAuthStore()
    vi.spyOn(store, 'login').mockRejectedValueOnce(new Error('Invalid credentials'))
    const wrapper = mount(LoginView, { global: { plugins: [router, pinia] } })
    await wrapper.find('[data-testid="username-input"]').setValue('jason')
    await wrapper.find('[data-testid="password-input"]').setValue('Pass1!')
    await wrapper.find('[data-testid="login-form"]').trigger('submit')
    await flushPromises()
    expect(wrapper.find('[data-testid="api-error"]').text()).toContain('Invalid credentials')
  })

  it('renders a submit button', () => {
    const wrapper = mountView()
    expect(wrapper.find('[data-testid="submit-btn"]').exists()).toBe(true)
  })
})
