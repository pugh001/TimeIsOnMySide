import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import { setActivePinia, createPinia } from 'pinia'
import { useAuthStore } from '@/stores/authStore'
import LocationPanel from '../LocationPanel.vue'
import * as locationsApi from '@/api/locations'

vi.mock('@/api/locations')

const mockLocation = { id: 'loc-uuid-1', slug: 'test-branch', name: 'Test Branch', address: '1 Main St' }
const mockUsers = [
  {
    id: 'u1', fullName: 'Jane Doe',
    workingTimes: [
      { day: 'monday' as const, shiftStart: '09:00', shiftEnd: '17:00' },
      { day: 'tuesday' as const, shiftStart: '09:00', shiftEnd: '17:00' },
    ],
  },
  {
    id: 'u2', fullName: 'John Smith',
    workingTimes: [
      { day: 'wednesday' as const, shiftStart: '10:00', shiftEnd: '18:00' },
    ],
  },
]

function mountPanel() {
  const pinia = createPinia()
  setActivePinia(pinia)
  const auth = useAuthStore()
  auth.adminToken = 'tok123'
  auth.adminUserId = 'uid-abc'
  return mount(LocationPanel, {
    props: { location: mockLocation },
    global: { plugins: [pinia] },
  })
}

beforeEach(() => {
  setActivePinia(createPinia())
  vi.mocked(locationsApi.getLocationUsers).mockReset()
})

describe('LocationPanel', () => {
  it('renders location name', () => {
    const wrapper = mountPanel()
    expect(wrapper.text()).toContain('Test Branch')
  })

  it('renders location address', () => {
    const wrapper = mountPanel()
    expect(wrapper.text()).toContain('1 Main St')
  })

  it('has a Show Users button', () => {
    const wrapper = mountPanel()
    expect(wrapper.find('[data-testid="show-users-btn"]').exists()).toBe(true)
  })

  it('has an Add User button', () => {
    const wrapper = mountPanel()
    expect(wrapper.find('[data-testid="add-user-btn"]').exists()).toBe(true)
  })

  it('does not show user list initially', () => {
    const wrapper = mountPanel()
    expect(wrapper.find('[data-testid="user-list"]').exists()).toBe(false)
  })

  it('clicking Show Users fetches and shows user list', async () => {
    vi.mocked(locationsApi.getLocationUsers).mockResolvedValueOnce(mockUsers)
    const wrapper = mountPanel()
    await wrapper.find('[data-testid="show-users-btn"]').trigger('click')
    await flushPromises()
    expect(wrapper.find('[data-testid="user-list"]').exists()).toBe(true)
    expect(wrapper.text()).toContain('Jane Doe')
    expect(wrapper.text()).toContain('John Smith')
  })

  it('shows shift times for each user', async () => {
    vi.mocked(locationsApi.getLocationUsers).mockResolvedValueOnce(mockUsers)
    const wrapper = mountPanel()
    await wrapper.find('[data-testid="show-users-btn"]').trigger('click')
    await flushPromises()
    expect(wrapper.text()).toContain('09:00')
    expect(wrapper.text()).toContain('17:00')
  })

  it('clicking Show Users again hides the list (toggle)', async () => {
    vi.mocked(locationsApi.getLocationUsers).mockResolvedValueOnce(mockUsers)
    const wrapper = mountPanel()
    await wrapper.find('[data-testid="show-users-btn"]').trigger('click')
    await flushPromises()
    await wrapper.find('[data-testid="show-users-btn"]').trigger('click')
    expect(wrapper.find('[data-testid="user-list"]').exists()).toBe(false)
  })

  it('shows loading state while fetching users', async () => {
    vi.mocked(locationsApi.getLocationUsers).mockReturnValueOnce(new Promise(() => {}))
    const wrapper = mountPanel()
    await wrapper.find('[data-testid="show-users-btn"]').trigger('click')
    expect(wrapper.find('[data-testid="users-loading"]').exists()).toBe(true)
  })

  it('shows empty state when no users returned', async () => {
    vi.mocked(locationsApi.getLocationUsers).mockResolvedValueOnce([])
    const wrapper = mountPanel()
    await wrapper.find('[data-testid="show-users-btn"]').trigger('click')
    await flushPromises()
    expect(wrapper.find('[data-testid="users-empty"]').exists()).toBe(true)
  })

  it('shows error state when fetch fails', async () => {
    vi.mocked(locationsApi.getLocationUsers).mockRejectedValueOnce(new Error('Server error'))
    const wrapper = mountPanel()
    await wrapper.find('[data-testid="show-users-btn"]').trigger('click')
    await flushPromises()
    expect(wrapper.find('[data-testid="users-error"]').exists()).toBe(true)
  })

  it('calls getLocationUsers with correct locationId and admin credentials', async () => {
    vi.mocked(locationsApi.getLocationUsers).mockResolvedValueOnce(mockUsers)
    const wrapper = mountPanel()
    await wrapper.find('[data-testid="show-users-btn"]').trigger('click')
    await flushPromises()
    expect(locationsApi.getLocationUsers).toHaveBeenCalledWith('loc-uuid-1', 'tok123', 'uid-abc')
  })
})
