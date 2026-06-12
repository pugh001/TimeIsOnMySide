import { describe, it, expect, beforeEach, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import { setActivePinia, createPinia } from 'pinia'
import { useBookingStore } from '@/stores/bookingStore'
import SlotGrid from '../SlotGrid.vue'
import * as bookingApi from '@/api/booking'

vi.mock('@/api/booking')

const mockSlots = [
  { id: 'a-2026-05-26-09:00', date: '2026-05-26', startTime: '09:00', endTime: '09:30', status: 'available' as const, locationId: 'a' },
  { id: 'a-2026-05-26-09:30', date: '2026-05-26', startTime: '09:30', endTime: '10:00', status: 'unavailable' as const, locationId: 'a' },
  { id: 'a-2026-05-26-10:00', date: '2026-05-26', startTime: '10:00', endTime: '10:30', status: 'available' as const, locationId: 'a' },
]

describe('SlotGrid', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.mocked(bookingApi.fetchSlots).mockResolvedValue(mockSlots)
    vi.mocked(bookingApi.createBooking).mockResolvedValue({ bookingId: 'bk-1', slotId: 'slot-1', startTime: '09:00', name: 'Jane', date: '2026-05-26' })
  })

  it('renders a SlotCard for each slot', () => {
    const store = useBookingStore()
    store.slots = mockSlots
    const wrapper = mount(SlotGrid, { props: { date: '2026-05-26' } })
    const cards = wrapper.findAll('[aria-label]')
    expect(cards).toHaveLength(3)
  })

  it('shows empty state when no slots', () => {
    const store = useBookingStore()
    store.slots = []
    const wrapper = mount(SlotGrid, { props: { date: '2026-05-26' } })
    expect(wrapper.find('[data-testid="no-slots-empty-state"]').exists()).toBe(true)
    expect(wrapper.text()).toContain('No slots available')
  })

  it('shows loading state when store status is loading', () => {
    const store = useBookingStore()
    store.slots = []
    store.slotsStatus = 'loading'
    const wrapper = mount(SlotGrid, { props: { date: '2026-05-26' } })
    expect(wrapper.text()).toContain('Loading')
  })

  it('emits select-slot when a SlotCard emits select', async () => {
    const store = useBookingStore()
    store.slots = mockSlots
    const wrapper = mount(SlotGrid, { props: { date: '2026-05-26' } })
    await wrapper.findAll('[aria-label]')[0]!.trigger('click')
    expect(wrapper.emitted('select-slot')).toBeTruthy()
  })

  it('updates displayed slots when date prop changes', async () => {
    const store = useBookingStore()
    const slotOtherDay = { id: 'a-2026-05-27-09:00', date: '2026-05-27', startTime: '09:00', endTime: '09:30', status: 'available' as const, locationId: 'a' }
    store.slots = [...mockSlots, slotOtherDay]
    const wrapper = mount(SlotGrid, { props: { date: '2026-05-26' } })
    expect(wrapper.findAll('[aria-label]')).toHaveLength(3)
    await wrapper.setProps({ date: '2026-05-27' })
    expect(wrapper.findAll('[aria-label]')).toHaveLength(1)
  })
})
