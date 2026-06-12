import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import { setActivePinia, createPinia } from 'pinia'
import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'
import StaffDashboardView from '../StaffDashboardView.vue'
import * as bookingsApi from '@/api/bookings'

vi.mock('@/api/bookings')

// "now" fixed at 2026-06-09 12:00 local
const NOW_ISO = '2026-06-09T12:00:00'

const pastBooking   = { bookingRef: 'bk-past', date: '2026-06-08', startTime: '09:00', endTime: '09:30', customerName: 'Past Person' }
const todayPast     = { bookingRef: 'bk-tpast', date: '2026-06-09', startTime: '09:00', endTime: '09:30', customerName: 'Morning Person' }
const todayFuture   = { bookingRef: 'bk-tfut',  date: '2026-06-09', startTime: '14:00', endTime: '14:30', customerName: 'Afternoon Person' }
const futureBooking = { bookingRef: 'bk-fut',   date: '2026-06-10', startTime: '09:00', endTime: '09:30', customerName: 'Future Person' }

const router = createRouter({
  history: createWebHistory(),
  routes: [{ path: '/dashboard', component: { template: '<div />' } }],
})

function mountView(staffUserId = 'uid-staff', staffToken = 'staff-tok') {
  const pinia = createPinia()
  setActivePinia(pinia)
  const auth = useAuthStore()
  auth.employeeName = 'Jane Doe'
  auth.role = 'staff'
  auth.staffUserId = staffUserId
  auth.staffToken = staffToken
  return mount(StaffDashboardView, { global: { plugins: [router, pinia] } })
}

beforeEach(() => {
  setActivePinia(createPinia())
  vi.mocked(bookingsApi.fetchBookings).mockReset()
  vi.useFakeTimers()
  vi.setSystemTime(new Date(NOW_ISO))
})

afterEach(() => {
  vi.useRealTimers()
})

describe('StaffDashboardView', () => {
  it('calls fetchBookings on mount with staffUserId and staffToken from auth store', async () => {
    vi.mocked(bookingsApi.fetchBookings).mockResolvedValueOnce([futureBooking])
    mountView('uid-staff', 'staff-tok')
    await flushPromises()
    expect(bookingsApi.fetchBookings).toHaveBeenCalledWith('uid-staff', 'staff-tok')
  })

  it('shows loading state while fetching', async () => {
    vi.mocked(bookingsApi.fetchBookings).mockReturnValueOnce(new Promise(() => {}))
    const wrapper = mountView()
    expect(wrapper.find('[data-testid="loading-state"]').exists()).toBe(true)
  })

  it('renders tabs on success', async () => {
    vi.mocked(bookingsApi.fetchBookings).mockResolvedValueOnce([futureBooking])
    const wrapper = mountView()
    await flushPromises()
    expect(wrapper.find('[data-testid="tab-future"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="tab-past"]').exists()).toBe(true)
  })

  it('future tab is active by default', async () => {
    vi.mocked(bookingsApi.fetchBookings).mockResolvedValueOnce([futureBooking])
    const wrapper = mountView()
    await flushPromises()
    expect(wrapper.find('[data-testid="tab-future"]').attributes('aria-selected')).toBe('true')
    expect(wrapper.find('[data-testid="tab-past"]').attributes('aria-selected')).toBe('false')
  })

  it('future tab shows bookings with date > today', async () => {
    vi.mocked(bookingsApi.fetchBookings).mockResolvedValueOnce([pastBooking, futureBooking])
    const wrapper = mountView()
    await flushPromises()
    expect(wrapper.text()).toContain('Future Person')
    expect(wrapper.text()).not.toContain('Past Person')
  })

  it('future tab shows today bookings with startTime >= now', async () => {
    vi.mocked(bookingsApi.fetchBookings).mockResolvedValueOnce([todayPast, todayFuture])
    const wrapper = mountView()
    await flushPromises()
    expect(wrapper.text()).toContain('Afternoon Person')
    expect(wrapper.text()).not.toContain('Morning Person')
  })

  it('past tab shows bookings with date < today', async () => {
    vi.mocked(bookingsApi.fetchBookings).mockResolvedValueOnce([pastBooking, futureBooking])
    const wrapper = mountView()
    await flushPromises()
    await wrapper.find('[data-testid="tab-past"]').trigger('click')
    expect(wrapper.text()).toContain('Past Person')
    expect(wrapper.text()).not.toContain('Future Person')
  })

  it('past tab shows today bookings with startTime < now', async () => {
    vi.mocked(bookingsApi.fetchBookings).mockResolvedValueOnce([todayPast, todayFuture])
    const wrapper = mountView()
    await flushPromises()
    await wrapper.find('[data-testid="tab-past"]').trigger('click')
    expect(wrapper.text()).toContain('Morning Person')
    expect(wrapper.text()).not.toContain('Afternoon Person')
  })

  it('clicking past tab switches the active tab', async () => {
    vi.mocked(bookingsApi.fetchBookings).mockResolvedValueOnce([pastBooking])
    const wrapper = mountView()
    await flushPromises()
    await wrapper.find('[data-testid="tab-past"]').trigger('click')
    expect(wrapper.find('[data-testid="tab-past"]').attributes('aria-selected')).toBe('true')
    expect(wrapper.find('[data-testid="tab-future"]').attributes('aria-selected')).toBe('false')
  })

  it('past tab shows per-tab empty message when no past bookings', async () => {
    vi.mocked(bookingsApi.fetchBookings).mockResolvedValueOnce([futureBooking])
    const wrapper = mountView()
    await flushPromises()
    await wrapper.find('[data-testid="tab-past"]').trigger('click')
    expect(wrapper.find('[data-testid="tab-empty-state"]').exists()).toBe(true)
  })

  it('future tab shows per-tab empty message when no future bookings', async () => {
    vi.mocked(bookingsApi.fetchBookings).mockResolvedValueOnce([pastBooking])
    const wrapper = mountView()
    await flushPromises()
    expect(wrapper.find('[data-testid="tab-empty-state"]').exists()).toBe(true)
  })

  it('shows global empty state when API returns empty array', async () => {
    vi.mocked(bookingsApi.fetchBookings).mockResolvedValueOnce([])
    const wrapper = mountView()
    await flushPromises()
    expect(wrapper.find('[data-testid="empty-state"]').exists()).toBe(true)
    expect(wrapper.text()).toContain('No bookings')
  })

  it('shows booking reference for each booking', async () => {
    vi.mocked(bookingsApi.fetchBookings).mockResolvedValueOnce([futureBooking])
    const wrapper = mountView()
    await flushPromises()
    expect(wrapper.text()).toContain('bk-fut')
  })

  it('shows date and time for each booking', async () => {
    vi.mocked(bookingsApi.fetchBookings).mockResolvedValueOnce([futureBooking])
    const wrapper = mountView()
    await flushPromises()
    expect(wrapper.text()).toContain('2026-06-10')
    expect(wrapper.text()).toContain('09:00')
  })

  it('shows error message on fetch failure', async () => {
    vi.mocked(bookingsApi.fetchBookings).mockRejectedValueOnce(new Error('Server error'))
    const wrapper = mountView()
    await flushPromises()
    expect(wrapper.find('[data-testid="api-error"]').exists()).toBe(true)
    expect(wrapper.text()).toContain('Server error')
  })

  it('shows error state without calling fetchBookings when staffUserId is null', async () => {
    const pinia = createPinia()
    setActivePinia(pinia)
    const auth = useAuthStore()
    auth.employeeName = 'Jane Doe'
    auth.role = 'staff'
    // staffUserId and staffToken intentionally left null
    const wrapper = mount(StaffDashboardView, { global: { plugins: [router, pinia] } })
    await flushPromises()
    expect(bookingsApi.fetchBookings).not.toHaveBeenCalled()
    expect(wrapper.find('[data-testid="api-error"]').exists()).toBe(true)
  })
})
