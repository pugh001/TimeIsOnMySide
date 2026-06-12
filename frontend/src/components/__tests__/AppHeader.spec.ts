import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import { setActivePinia, createPinia } from 'pinia'
import { createRouter, createWebHistory } from 'vue-router'
import AppHeader from '../AppHeader.vue'
import { useAuthStore } from '@/stores/authStore'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', component: { template: '<div />' } },
    { path: '/login', component: { template: '<div />' } },
  ],
})

function mountHeader(
  employeeName: string | null = null,
  role: 'admin' | 'staff' | null = null,
) {
  return mount(AppHeader, {
    props: { employeeName, role },
    global: { plugins: [router, createPinia()] },
  })
}

beforeEach(() => {
  setActivePinia(createPinia())
})

describe('AppHeader', () => {
  it('renders the brand name', () => {
    const wrapper = mountHeader()
    expect(wrapper.text()).toContain('Time Is On My Side - Booking System')
  })

  it('does not render a hamburger button', () => {
    const wrapper = mountHeader()
    expect(wrapper.find('[data-testid="hamburger"]').exists()).toBe(false)
  })

  it('renders a login link when not logged in', () => {
    const wrapper = mountHeader(null)
    const link = wrapper.find('[data-testid="login-link"]')
    expect(link.exists()).toBe(true)
    expect(link.text()).toContain('Login')
  })

  it('does not render login link when logged in', () => {
    const wrapper = mountHeader('Jason Pugh', 'staff')
    expect(wrapper.find('[data-testid="login-link"]').exists()).toBe(false)
  })

  it('renders employee name toggle button when logged in', () => {
    const wrapper = mountHeader('Jason Pugh', 'staff')
    const toggle = wrapper.find('[data-testid="user-menu-toggle"]')
    expect(toggle.exists()).toBe(true)
    expect(toggle.text()).toContain('Jason Pugh')
  })

  it('does not render user menu toggle when not logged in', () => {
    const wrapper = mountHeader(null)
    expect(wrapper.find('[data-testid="user-menu-toggle"]').exists()).toBe(false)
  })

  it('dropdown is hidden by default when logged in', () => {
    const wrapper = mountHeader('Jason Pugh', 'staff')
    expect(wrapper.find('[data-testid="user-dropdown"]').exists()).toBe(false)
  })

  it('clicking user menu toggle opens the dropdown', async () => {
    const wrapper = mountHeader('Jason Pugh', 'staff')
    await wrapper.find('[data-testid="user-menu-toggle"]').trigger('click')
    expect(wrapper.find('[data-testid="user-dropdown"]').exists()).toBe(true)
  })

  it('dropdown contains a logout button', async () => {
    const wrapper = mountHeader('Jason Pugh', 'staff')
    await wrapper.find('[data-testid="user-menu-toggle"]').trigger('click')
    expect(wrapper.find('[data-testid="logout-btn"]').exists()).toBe(true)
  })

  it('clicking logout clears auth store and redirects to /login', async () => {
    const pinia = createPinia()
    setActivePinia(pinia)
    const store = useAuthStore()
    store.employeeName = 'Jason Pugh'
    vi.spyOn(store, 'logout')

    const wrapper = mount(AppHeader, {
      props: { employeeName: 'Jason Pugh', role: 'staff' },
      global: { plugins: [router, pinia] },
    })

    await wrapper.find('[data-testid="user-menu-toggle"]').trigger('click')
    await wrapper.find('[data-testid="logout-btn"]').trigger('click')
    await flushPromises()

    expect(store.logout).toHaveBeenCalled()
    expect(router.currentRoute.value.path).toBe('/login')
  })

  it('clicking toggle again closes the dropdown', async () => {
    const wrapper = mountHeader('Jason Pugh', 'staff')
    await wrapper.find('[data-testid="user-menu-toggle"]').trigger('click')
    await wrapper.find('[data-testid="user-menu-toggle"]').trigger('click')
    expect(wrapper.find('[data-testid="user-dropdown"]').exists()).toBe(false)
  })

  it('clicking outside the dropdown closes it', async () => {
    const wrapper = mountHeader('Jason Pugh', 'staff')
    await wrapper.find('[data-testid="user-menu-toggle"]').trigger('click')
    expect(wrapper.find('[data-testid="user-dropdown"]').exists()).toBe(true)
    document.body.dispatchEvent(new MouseEvent('click', { bubbles: true }))
    await wrapper.vm.$nextTick()
    expect(wrapper.find('[data-testid="user-dropdown"]').exists()).toBe(false)
  })

  it('dropdown contains only Logout for admin — no add-location or add-user buttons', async () => {
    const wrapper = mountHeader('Jason Pugh', 'admin')
    await wrapper.find('[data-testid="user-menu-toggle"]').trigger('click')
    expect(wrapper.find('[data-testid="logout-btn"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="add-location-btn"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="add-user-btn"]').exists()).toBe(false)
  })

  it('dropdown contains only Logout for staff', async () => {
    const wrapper = mountHeader('Jane', 'staff')
    await wrapper.find('[data-testid="user-menu-toggle"]').trigger('click')
    expect(wrapper.find('[data-testid="logout-btn"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="add-location-btn"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="add-user-btn"]').exists()).toBe(false)
  })
})
