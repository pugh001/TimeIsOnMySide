import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import { setActivePinia, createPinia } from 'pinia'
import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'
import AdminDashboardView from '../AdminDashboardView.vue'
import * as bookingApi from '@/api/booking'

vi.mock('@/api/booking')
vi.mock('@/components/LocationPanel.vue', () => ({
  default: {
    name: 'LocationPanel',
    props: ['location'],
    template: '<div data-testid="location-panel">{{ location.name }}</div>',
  },
}))

const mockLocations = [
  { id: 'loc-1', slug: 'branch-a', name: 'Branch A', address: '1 A St' },
  { id: 'loc-2', slug: 'branch-b', name: 'Branch B', address: '2 B St' },
]

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/admin', component: { template: '<div />' } },
    { path: '/locations/new', component: { template: '<div />' } },
  ],
})

function mountView() {
  const pinia = createPinia()
  setActivePinia(pinia)
  const auth = useAuthStore()
  auth.employeeName = 'Admin User'
  auth.role = 'admin'
  auth.adminToken = 'tok123'
  auth.adminUserId = 'uid-abc'
  return mount(AdminDashboardView, { global: { plugins: [router, pinia] } })
}

beforeEach(() => {
  setActivePinia(createPinia())
  vi.mocked(bookingApi.fetchLocations).mockReset()
})

describe('AdminDashboardView', () => {
  it('shows loading state on mount', async () => {
    vi.mocked(bookingApi.fetchLocations).mockReturnValueOnce(new Promise(() => {}))
    const wrapper = mountView()
    expect(wrapper.find('[data-testid="loading-state"]').exists()).toBe(true)
  })

  it('renders a LocationPanel per location on success', async () => {
    vi.mocked(bookingApi.fetchLocations).mockResolvedValueOnce(mockLocations)
    const wrapper = mountView()
    await flushPromises()
    const panels = wrapper.findAll('[data-testid="location-panel"]')
    expect(panels).toHaveLength(2)
    expect(wrapper.text()).toContain('Branch A')
    expect(wrapper.text()).toContain('Branch B')
  })

  it('shows empty state when no locations exist', async () => {
    vi.mocked(bookingApi.fetchLocations).mockResolvedValueOnce([])
    const wrapper = mountView()
    await flushPromises()
    expect(wrapper.find('[data-testid="empty-state"]').exists()).toBe(true)
  })

  it('shows error state when fetch fails', async () => {
    vi.mocked(bookingApi.fetchLocations).mockRejectedValueOnce(new Error('Network error'))
    const wrapper = mountView()
    await flushPromises()
    expect(wrapper.find('[data-testid="api-error"]').exists()).toBe(true)
    expect(wrapper.text()).toContain('Network error')
  })

  it('has an Add Location button', async () => {
    vi.mocked(bookingApi.fetchLocations).mockResolvedValueOnce(mockLocations)
    const wrapper = mountView()
    await flushPromises()
    expect(wrapper.find('[data-testid="add-location-btn"]').exists()).toBe(true)
  })

  it('renders page title', async () => {
    vi.mocked(bookingApi.fetchLocations).mockResolvedValueOnce(mockLocations)
    const wrapper = mountView()
    await flushPromises()
    expect(wrapper.text()).toContain('Locations')
  })
})
