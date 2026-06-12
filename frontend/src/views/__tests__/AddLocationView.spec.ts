import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import { setActivePinia, createPinia } from 'pinia'
import { createRouter, createWebHistory } from 'vue-router'
import AddLocationView from '../AddLocationView.vue'
import { useAuthStore } from '@/stores/authStore'
import * as locationsApi from '@/api/locations'

vi.mock('@/api/locations')

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/admin', component: { template: '<div />' } },
    { path: '/locations/new', component: { template: '<div />' } },
  ],
})

function mountView(adminToken = 'tok123', adminUserId = 'uid-abc') {
  const pinia = createPinia()
  setActivePinia(pinia)
  const store = useAuthStore()
  store.employeeName = 'Jason Pugh'
  store.role = 'admin'
  store.adminToken = adminToken
  store.adminUserId = adminUserId

  return mount(AddLocationView, {
    global: { plugins: [router, pinia] },
  })
}

beforeEach(() => {
  setActivePinia(createPinia())
  vi.mocked(locationsApi.createLocation).mockReset()
})

describe('AddLocationView', () => {
  it('renders the form with name and address inputs', () => {
    const wrapper = mountView()
    expect(wrapper.find('[data-testid="name-input"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="address-input"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="submit-btn"]').exists()).toBe(true)
  })

  it('renders opening hours rows for all 7 days', () => {
    const wrapper = mountView()
    const days = ['monday', 'tuesday', 'wednesday', 'thursday', 'friday', 'saturday', 'sunday']
    for (const day of days) {
      expect(wrapper.find(`[data-testid="day-row-${day}"]`).exists()).toBe(true)
    }
  })

  it('shows name validation error when name is empty on submit', async () => {
    const wrapper = mountView()
    await wrapper.find('[data-testid="address-input"]').setValue('123 Main St')
    await wrapper.find('[data-testid="add-location-form"]').trigger('submit')
    expect(wrapper.find('[data-testid="name-error"]').exists()).toBe(true)
    expect(locationsApi.createLocation).not.toHaveBeenCalled()
  })

  it('shows name validation error when name is 1 character', async () => {
    const wrapper = mountView()
    await wrapper.find('[data-testid="name-input"]').setValue('A')
    await wrapper.find('[data-testid="address-input"]').setValue('123 Main St')
    await wrapper.find('[data-testid="add-location-form"]').trigger('submit')
    expect(wrapper.find('[data-testid="name-error"]').exists()).toBe(true)
    expect(locationsApi.createLocation).not.toHaveBeenCalled()
  })

  it('shows address validation error when address is empty on submit', async () => {
    const wrapper = mountView()
    await wrapper.find('[data-testid="name-input"]').setValue('Main Branch')
    await wrapper.find('[data-testid="add-location-form"]').trigger('submit')
    expect(wrapper.find('[data-testid="address-error"]').exists()).toBe(true)
    expect(locationsApi.createLocation).not.toHaveBeenCalled()
  })

  it('calls createLocation with name, address, and admin credentials on valid submit', async () => {
    vi.mocked(locationsApi.createLocation).mockResolvedValueOnce({ slug: 'main-branch', id: 'guid-1' })
    const wrapper = mountView()
    await wrapper.find('[data-testid="name-input"]').setValue('Main Branch')
    await wrapper.find('[data-testid="address-input"]').setValue('123 Main St')
    await wrapper.find('[data-testid="add-location-form"]').trigger('submit')
    await flushPromises()
    expect(locationsApi.createLocation).toHaveBeenCalledWith(
      expect.objectContaining({ name: 'Main Branch', address: '123 Main St' }),
      'tok123',
      'uid-abc',
    )
  })

  it('shows success state with slug after successful submit', async () => {
    vi.mocked(locationsApi.createLocation).mockResolvedValueOnce({ slug: 'main-branch', id: 'guid-1' })
    const wrapper = mountView()
    await wrapper.find('[data-testid="name-input"]').setValue('Main Branch')
    await wrapper.find('[data-testid="address-input"]').setValue('123 Main St')
    await wrapper.find('[data-testid="add-location-form"]').trigger('submit')
    await flushPromises()
    expect(wrapper.find('[data-testid="success-state"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="success-state"]').text()).toContain('main-branch')
  })

  it('shows api error message on failed submit', async () => {
    vi.mocked(locationsApi.createLocation).mockRejectedValueOnce(new Error('Unauthorized — admin access required'))
    const wrapper = mountView()
    await wrapper.find('[data-testid="name-input"]').setValue('Main Branch')
    await wrapper.find('[data-testid="address-input"]').setValue('123 Main St')
    await wrapper.find('[data-testid="add-location-form"]').trigger('submit')
    await flushPromises()
    expect(wrapper.find('[data-testid="api-error"]').text()).toContain('Unauthorized')
  })

  it('clicking Add another resets the form', async () => {
    vi.mocked(locationsApi.createLocation).mockResolvedValueOnce({ slug: 'main-branch', id: 'guid-1' })
    const wrapper = mountView()
    await wrapper.find('[data-testid="name-input"]').setValue('Main Branch')
    await wrapper.find('[data-testid="address-input"]').setValue('123 Main St')
    await wrapper.find('[data-testid="add-location-form"]').trigger('submit')
    await flushPromises()
    await wrapper.find('[data-testid="add-another-btn"]').trigger('click')
    expect(wrapper.find('[data-testid="add-location-form"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="success-state"]').exists()).toBe(false)
  })

  it('enabling a day shows open/close time inputs', async () => {
    const wrapper = mountView()
    await wrapper.find('[data-testid="toggle-monday"]').setValue(true)
    expect(wrapper.find('[data-testid="open-monday"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="close-monday"]').exists()).toBe(true)
  })

  it('disabling a day hides the time inputs', async () => {
    const wrapper = mountView()
    const toggle = wrapper.find('[data-testid="toggle-monday"]')
    await toggle.setValue(true)
    await toggle.setValue(false)
    expect(wrapper.find('[data-testid="open-monday"]').exists()).toBe(false)
  })

  it('includes enabled day hours in the API call', async () => {
    vi.mocked(locationsApi.createLocation).mockResolvedValueOnce({ slug: 'main-branch', id: 'guid-1' })
    const wrapper = mountView()
    await wrapper.find('[data-testid="name-input"]').setValue('Main Branch')
    await wrapper.find('[data-testid="address-input"]').setValue('123 Main St')
    await wrapper.find('[data-testid="toggle-monday"]').setValue(true)
    await wrapper.find('[data-testid="add-location-form"]').trigger('submit')
    await flushPromises()
    const call = vi.mocked(locationsApi.createLocation).mock.calls[0]![0]
    expect(call.openingHours?.monday).toMatchObject({ openTime: '08:00', closeTime: '17:00' })
  })

  it('back button navigates to /admin', async () => {
    const wrapper = mountView()
    await wrapper.find('[data-testid="back-btn"]').trigger('click')
    await flushPromises()
    expect(router.currentRoute.value.path).toBe('/admin')
  })

  it('shows time error when closeTime is before openTime and blocks API call', async () => {
    const wrapper = mountView()
    await wrapper.find('[data-testid="name-input"]').setValue('Main Branch')
    await wrapper.find('[data-testid="address-input"]').setValue('123 Main St')
    await wrapper.find('[data-testid="toggle-monday"]').setValue(true)
    await wrapper.find('[data-testid="open-monday"]').setValue('17:00')
    await wrapper.find('[data-testid="close-monday"]').setValue('08:00')
    await wrapper.find('[data-testid="add-location-form"]').trigger('submit')
    await flushPromises()
    expect(wrapper.find('[data-testid="time-error-monday"]').exists()).toBe(true)
    expect(locationsApi.createLocation).not.toHaveBeenCalled()
  })

  it('shows time error when span is 59 minutes and blocks API call', async () => {
    const wrapper = mountView()
    await wrapper.find('[data-testid="name-input"]').setValue('Main Branch')
    await wrapper.find('[data-testid="address-input"]').setValue('123 Main St')
    await wrapper.find('[data-testid="toggle-monday"]').setValue(true)
    await wrapper.find('[data-testid="open-monday"]').setValue('09:00')
    await wrapper.find('[data-testid="close-monday"]').setValue('09:59')
    await wrapper.find('[data-testid="add-location-form"]').trigger('submit')
    await flushPromises()
    expect(wrapper.find('[data-testid="time-error-monday"]').exists()).toBe(true)
    expect(locationsApi.createLocation).not.toHaveBeenCalled()
  })

  it('shows no time error and calls API when span is exactly 1 hour', async () => {
    vi.mocked(locationsApi.createLocation).mockResolvedValueOnce({ slug: 'main-branch', id: 'guid-1' })
    const wrapper = mountView()
    await wrapper.find('[data-testid="name-input"]').setValue('Main Branch')
    await wrapper.find('[data-testid="address-input"]').setValue('123 Main St')
    await wrapper.find('[data-testid="toggle-monday"]').setValue(true)
    await wrapper.find('[data-testid="open-monday"]').setValue('09:00')
    await wrapper.find('[data-testid="close-monday"]').setValue('10:00')
    await wrapper.find('[data-testid="add-location-form"]').trigger('submit')
    await flushPromises()
    expect(wrapper.find('[data-testid="time-error-monday"]').exists()).toBe(false)
    expect(locationsApi.createLocation).toHaveBeenCalled()
  })
})
