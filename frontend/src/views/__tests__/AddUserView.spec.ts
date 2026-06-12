import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import { setActivePinia, createPinia } from 'pinia'
import { createRouter, createWebHistory } from 'vue-router'
import AddUserView from '../AddUserView.vue'
import { useAuthStore } from '@/stores/authStore'
import * as usersApi from '@/api/users'
import * as locationsApi from '@/api/locations'

vi.mock('@/api/users')
vi.mock('@/api/locations')

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/admin', component: { template: '<div />' } },
    { path: '/users/new', component: { template: '<div />' } },
  ],
})

const mockLocation = {
  id: 'loc-1',
  slug: 'cape-town-branch',
  name: 'Cape Town Branch',
  openingHours: {
    monday:    { openTime: '09:00', closeTime: '17:00' },
    tuesday:   { openTime: '09:00', closeTime: '17:00' },
    wednesday: { openTime: '09:00', closeTime: '17:00' },
    thursday:  { openTime: '09:00', closeTime: '17:00' },
    friday:    { openTime: '09:00', closeTime: '17:00' },
    saturday:  null,
    sunday:    null,
  },
}

async function mountView(adminToken = 'tok123', adminUserId = 'uid-abc') {
  const pinia = createPinia()
  setActivePinia(pinia)

  const auth = useAuthStore()
  auth.employeeName = 'Jason Pugh'
  auth.role = 'admin'
  auth.adminToken = adminToken
  auth.adminUserId = adminUserId

  vi.mocked(locationsApi.getLocation).mockResolvedValue(mockLocation)

  await router.push('/users/new?locationId=loc-1')
  await router.isReady()

  return mount(AddUserView, {
    global: { plugins: [router, pinia] },
  })
}

type Wrapper = Awaited<ReturnType<typeof mountView>>

async function fillAndSubmit(wrapper: Wrapper) {
  await flushPromises()
  await wrapper.find('[data-testid="first-name-input"]').setValue('Jane')
  await wrapper.find('[data-testid="last-name-input"]').setValue('Doe')
  await wrapper.find('[data-testid="password-input"]').setValue('secret99')
  await wrapper.find('[data-testid="confirm-password-input"]').setValue('secret99')
  await wrapper.find('[data-testid="work-day-monday"]').setValue(true)
  await wrapper.find('[data-testid="add-user-form"]').trigger('submit')
  await flushPromises()
}

beforeEach(() => {
  setActivePinia(createPinia())
  vi.mocked(usersApi.createUser).mockReset()
  vi.mocked(locationsApi.getLocation).mockReset()
})

describe('AddUserView', () => {
  it('shows loading state while location is being fetched', async () => {
    vi.mocked(locationsApi.getLocation).mockReturnValue(new Promise(() => {}))
    const wrapper = await mountView()
    expect(wrapper.find('[data-testid="location-loading"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="add-user-form"]').exists()).toBe(false)
  })

  it('renders form after location loads', async () => {
    const wrapper = await mountView()
    await flushPromises()
    expect(wrapper.find('[data-testid="add-user-form"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="location-loading"]').exists()).toBe(false)
  })

  it('renders all required personal/credential fields', async () => {
    const wrapper = await mountView()
    await flushPromises()
    expect(wrapper.find('[data-testid="first-name-input"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="last-name-input"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="password-input"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="confirm-password-input"]').exists()).toBe(true)
  })

  it('does NOT render a username input', async () => {
    const wrapper = await mountView()
    await flushPromises()
    expect(wrapper.find('[data-testid="username-input"]').exists()).toBe(false)
  })

  it('does NOT render a LocationPicker (location is pre-selected)', async () => {
    const wrapper = await mountView()
    await flushPromises()
    expect(wrapper.find('[data-testid="location-list"]').exists()).toBe(false)
  })

  it('shows location name as subtitle', async () => {
    const wrapper = await mountView()
    await flushPromises()
    expect(wrapper.find('[data-testid="location-name"]').text()).toContain('Cape Town Branch')
  })

  it('renders checkboxes only for open days (5 weekdays, not saturday/sunday)', async () => {
    const wrapper = await mountView()
    await flushPromises()
    expect(wrapper.find('[data-testid="work-day-monday"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="work-day-tuesday"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="work-day-friday"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="work-day-saturday"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="work-day-sunday"]').exists()).toBe(false)
  })

  it('shows shift inputs for a day when its checkbox is ticked', async () => {
    const wrapper = await mountView()
    await flushPromises()
    expect(wrapper.find('[data-testid="shift-start-monday"]').exists()).toBe(false)
    await wrapper.find('[data-testid="work-day-monday"]').setValue(true)
    expect(wrapper.find('[data-testid="shift-start-monday"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="shift-end-monday"]').exists()).toBe(true)
  })

  it('shift inputs pre-fill with branch open/close times', async () => {
    const wrapper = await mountView()
    await flushPromises()
    await wrapper.find('[data-testid="work-day-monday"]').setValue(true)
    const startInput = wrapper.find('[data-testid="shift-start-monday"]').element as HTMLInputElement
    const endInput = wrapper.find('[data-testid="shift-end-monday"]').element as HTMLInputElement
    expect(startInput.value).toBe('09:00')
    expect(endInput.value).toBe('17:00')
  })

  it('shift inputs have min/max constrained to branch hours', async () => {
    const wrapper = await mountView()
    await flushPromises()
    await wrapper.find('[data-testid="work-day-monday"]').setValue(true)
    const startInput = wrapper.find('[data-testid="shift-start-monday"]').element as HTMLInputElement
    expect(startInput.min).toBe('09:00')
    expect(startInput.max).toBe('17:00')
  })

  it('shows working-times-error and blocks submit when no day is ticked', async () => {
    const wrapper = await mountView()
    await flushPromises()
    await wrapper.find('[data-testid="first-name-input"]').setValue('Jane')
    await wrapper.find('[data-testid="last-name-input"]').setValue('Doe')
    await wrapper.find('[data-testid="password-input"]').setValue('secret99')
    await wrapper.find('[data-testid="confirm-password-input"]').setValue('secret99')
    await wrapper.find('[data-testid="add-user-form"]').trigger('submit')
    await flushPromises()
    expect(wrapper.find('[data-testid="working-times-error"]').exists()).toBe(true)
    expect(usersApi.createUser).not.toHaveBeenCalled()
  })

  it('shows confirm password mismatch error and blocks submit', async () => {
    const wrapper = await mountView()
    await flushPromises()
    await wrapper.find('[data-testid="first-name-input"]').setValue('Jane')
    await wrapper.find('[data-testid="last-name-input"]').setValue('Doe')
    await wrapper.find('[data-testid="password-input"]').setValue('secret99')
    await wrapper.find('[data-testid="confirm-password-input"]').setValue('different')
    await wrapper.find('[data-testid="add-user-form"]').trigger('submit')
    await flushPromises()
    expect(wrapper.find('[data-testid="confirm-password-error"]').exists()).toBe(true)
    expect(usersApi.createUser).not.toHaveBeenCalled()
  })

  it('shows first-name-error when firstName contains digits', async () => {
    const wrapper = await mountView()
    await flushPromises()
    await wrapper.find('[data-testid="first-name-input"]').setValue('Jane2')
    await wrapper.find('[data-testid="last-name-input"]').setValue('Doe')
    await wrapper.find('[data-testid="password-input"]').setValue('secret99')
    await wrapper.find('[data-testid="confirm-password-input"]').setValue('secret99')
    await wrapper.find('[data-testid="work-day-monday"]').setValue(true)
    await wrapper.find('[data-testid="add-user-form"]').trigger('submit')
    await flushPromises()
    expect(wrapper.find('[data-testid="first-name-error"]').exists()).toBe(true)
    expect(usersApi.createUser).not.toHaveBeenCalled()
  })

  it('shows last-name-error and blocks submit when lastName contains digits', async () => {
    const wrapper = await mountView()
    await flushPromises()
    await wrapper.find('[data-testid="first-name-input"]').setValue('Jane')
    await wrapper.find('[data-testid="last-name-input"]').setValue('Doe3')
    await wrapper.find('[data-testid="password-input"]').setValue('secret99')
    await wrapper.find('[data-testid="confirm-password-input"]').setValue('secret99')
    await wrapper.find('[data-testid="work-day-monday"]').setValue(true)
    await wrapper.find('[data-testid="add-user-form"]').trigger('submit')
    await flushPromises()
    expect(wrapper.find('[data-testid="last-name-error"]').exists()).toBe(true)
    expect(usersApi.createUser).not.toHaveBeenCalled()
  })

  it('calls createUser with workingTimes containing only ticked days', async () => {
    vi.mocked(usersApi.createUser).mockResolvedValueOnce({ userId: 'uid-1', username: 'jane0001' })
    const wrapper = await mountView()
    await flushPromises()
    await wrapper.find('[data-testid="first-name-input"]').setValue('Jane')
    await wrapper.find('[data-testid="last-name-input"]').setValue('Doe')
    await wrapper.find('[data-testid="password-input"]').setValue('secret99')
    await wrapper.find('[data-testid="confirm-password-input"]').setValue('secret99')
    await wrapper.find('[data-testid="work-day-wednesday"]').setValue(true)
    await wrapper.find('[data-testid="work-day-friday"]').setValue(true)
    await wrapper.find('[data-testid="add-user-form"]').trigger('submit')
    await flushPromises()
    const call = vi.mocked(usersApi.createUser).mock.calls[0]![0]
    expect(call.workingTimes).toHaveLength(2)
    expect(call.workingTimes.map((w: { day: string }) => w.day)).toEqual(['wednesday', 'friday'])
  })

  it('calls createUser with correct full payload on valid submit', async () => {
    vi.mocked(usersApi.createUser).mockResolvedValueOnce({ userId: 'uid-1', username: 'jane0001' })
    const wrapper = await mountView()
    await fillAndSubmit(wrapper)
    expect(usersApi.createUser).toHaveBeenCalledWith(
      expect.objectContaining({
        firstName: 'Jane',
        lastName: 'Doe',
        password: 'secret99',
        locationId: 'loc-1',
        workingTimes: expect.arrayContaining([
          expect.objectContaining({ day: 'monday', shiftStart: '09:00', shiftEnd: '17:00' }),
        ]),
      }),
      'tok123',
      'uid-abc',
    )
    const call = vi.mocked(usersApi.createUser).mock.calls[0]![0]
    expect(Object.keys(call)).not.toContain('username')
  })

  it('shows username modal after successful submit', async () => {
    vi.mocked(usersApi.createUser).mockResolvedValueOnce({ userId: 'uid-1', username: 'jane0001' })
    const wrapper = await mountView()
    await fillAndSubmit(wrapper)
    expect(wrapper.find('[data-testid="username-modal"]').exists()).toBe(true)
  })

  it('modal displays the API-generated username', async () => {
    vi.mocked(usersApi.createUser).mockResolvedValueOnce({ userId: 'uid-1', username: 'jane0001' })
    const wrapper = await mountView()
    await fillAndSubmit(wrapper)
    expect(wrapper.find('[data-testid="modal-username"]').text()).toContain('jane0001')
  })

  it('clicking Done on modal closes it and resets the form', async () => {
    vi.mocked(usersApi.createUser).mockResolvedValueOnce({ userId: 'uid-1', username: 'jane0001' })
    const wrapper = await mountView()
    await fillAndSubmit(wrapper)
    await wrapper.find('[data-testid="modal-close-btn"]').trigger('click')
    expect(wrapper.find('[data-testid="username-modal"]').exists()).toBe(false)
    const firstNameInput = wrapper.find('[data-testid="first-name-input"]').element as HTMLInputElement
    expect(firstNameInput.value).toBe('')
  })

  it('shows API error message on failed submit', async () => {
    vi.mocked(usersApi.createUser).mockRejectedValueOnce(new Error('Server error'))
    const wrapper = await mountView()
    await fillAndSubmit(wrapper)
    expect(wrapper.find('[data-testid="api-error"]').text()).toContain('Server error')
  })

  it('shows error state when location fetch fails', async () => {
    vi.mocked(locationsApi.getLocation).mockRejectedValueOnce(new Error('Location not found'))
    const wrapper = await mountView()
    await flushPromises()
    expect(wrapper.find('[data-testid="location-load-error"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="add-user-form"]').exists()).toBe(false)
  })
})
