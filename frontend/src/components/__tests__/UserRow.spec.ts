import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import UserRow from '../UserRow.vue'
import * as usersApi from '@/api/users'

vi.mock('@/api/users')

const mockUser = {
  id: 'u-001',
  fullName: 'Jane Doe',
  workingTimes: [
    { day: 'monday' as const, shiftStart: '09:00', shiftEnd: '17:00' },
    { day: 'wednesday' as const, shiftStart: '09:00', shiftEnd: '17:00' },
  ],
}

const mockBookings = [
  { bookingRef: 'bk-001', date: '2026-12-10', startTime: '10:00', endTime: '10:30', customerName: 'Alice' },
  { bookingRef: 'bk-002', date: '2026-12-11', startTime: '11:00', endTime: '11:30', customerName: 'Bob' },
]

function mountRow() {
  return mount(UserRow, {
    props: { user: mockUser, adminToken: 'tok123', adminUserId: 'uid-admin' },
  })
}

beforeEach(() => {
  vi.mocked(usersApi.fetchUserBookings).mockReset()
})

describe('UserRow', () => {
  it('renders user full name', () => {
    const wrapper = mountRow()
    expect(wrapper.text()).toContain('Jane Doe')
  })

  it('renders working day shifts', () => {
    const wrapper = mountRow()
    // day label is lowercase in markup; CSS text-transform: capitalize is visual only
    expect(wrapper.text()).toContain('mon 09:00–17:00')
    expect(wrapper.text()).toContain('wed 09:00–17:00')
  })

  it('has a Show Bookings button', () => {
    const wrapper = mountRow()
    expect(wrapper.find('[data-testid="show-bookings-btn"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="show-bookings-btn"]').text()).toBe('Show Bookings')
  })

  it('does not show bookings list initially', () => {
    const wrapper = mountRow()
    expect(wrapper.find('[data-testid="bookings-list"]').exists()).toBe(false)
  })

  it('clicking Show Bookings fetches and shows booking cards', async () => {
    vi.mocked(usersApi.fetchUserBookings).mockResolvedValueOnce(mockBookings)
    const wrapper = mountRow()
    await wrapper.find('[data-testid="show-bookings-btn"]').trigger('click')
    await flushPromises()
    expect(wrapper.find('[data-testid="bookings-list"]').exists()).toBe(true)
    expect(wrapper.findAll('[data-testid="booking-card"]')).toHaveLength(2)
    expect(wrapper.text()).toContain('Alice')
    expect(wrapper.text()).toContain('Bob')
  })

  it('button label toggles to Hide Bookings when expanded', async () => {
    vi.mocked(usersApi.fetchUserBookings).mockResolvedValueOnce(mockBookings)
    const wrapper = mountRow()
    await wrapper.find('[data-testid="show-bookings-btn"]').trigger('click')
    await flushPromises()
    expect(wrapper.find('[data-testid="show-bookings-btn"]').text()).toBe('Hide Bookings')
  })

  it('clicking Hide Bookings collapses the list', async () => {
    vi.mocked(usersApi.fetchUserBookings).mockResolvedValueOnce(mockBookings)
    const wrapper = mountRow()
    await wrapper.find('[data-testid="show-bookings-btn"]').trigger('click')
    await flushPromises()
    await wrapper.find('[data-testid="show-bookings-btn"]').trigger('click')
    expect(wrapper.find('[data-testid="bookings-list"]').exists()).toBe(false)
  })

  it('shows loading state while fetching', async () => {
    vi.mocked(usersApi.fetchUserBookings).mockReturnValueOnce(new Promise(() => {}))
    const wrapper = mountRow()
    await wrapper.find('[data-testid="show-bookings-btn"]').trigger('click')
    expect(wrapper.find('[data-testid="bookings-loading"]').exists()).toBe(true)
  })

  it('shows empty state when no bookings returned', async () => {
    vi.mocked(usersApi.fetchUserBookings).mockResolvedValueOnce([])
    const wrapper = mountRow()
    await wrapper.find('[data-testid="show-bookings-btn"]').trigger('click')
    await flushPromises()
    expect(wrapper.find('[data-testid="bookings-empty"]').exists()).toBe(true)
  })

  it('shows error state when fetch fails', async () => {
    vi.mocked(usersApi.fetchUserBookings).mockRejectedValueOnce(new Error('Network error'))
    const wrapper = mountRow()
    await wrapper.find('[data-testid="show-bookings-btn"]').trigger('click')
    await flushPromises()
    expect(wrapper.find('[data-testid="bookings-error"]').exists()).toBe(true)
  })

  it('calls fetchUserBookings with correct userId and admin credentials', async () => {
    vi.mocked(usersApi.fetchUserBookings).mockResolvedValueOnce([])
    const wrapper = mountRow()
    await wrapper.find('[data-testid="show-bookings-btn"]').trigger('click')
    await flushPromises()
    expect(usersApi.fetchUserBookings).toHaveBeenCalledWith('u-001', 'tok123', 'uid-admin')
  })

  it('does not re-fetch on second expand after successful load', async () => {
    vi.mocked(usersApi.fetchUserBookings).mockResolvedValueOnce(mockBookings)
    const wrapper = mountRow()
    await wrapper.find('[data-testid="show-bookings-btn"]').trigger('click')
    await flushPromises()
    await wrapper.find('[data-testid="show-bookings-btn"]').trigger('click') // hide
    await wrapper.find('[data-testid="show-bookings-btn"]').trigger('click') // show again
    await flushPromises()
    expect(usersApi.fetchUserBookings).toHaveBeenCalledTimes(1)
  })
})
