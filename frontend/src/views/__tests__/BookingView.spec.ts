import { describe, it, expect, beforeEach, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import { setActivePinia, createPinia } from 'pinia'
import { useBookingStore } from '@/stores/bookingStore'
import BookingView from '../BookingView.vue'
import * as bookingApi from '@/api/booking'

vi.mock('@/api/booking')

const mockLocations = [
  { id: 'a', slug: 'location-a', name: 'Location A' },
  { id: 'b', slug: 'location-b', name: 'Location B' },
  { id: 'c', slug: 'location-c', name: 'Location C' },
]

const mockSlots = [
  { id: 'a-2026-05-26-09:00', date: '2026-05-26', startTime: '09:00', endTime: '09:30', status: 'available' as const, locationId: 'a' },
]

const mountView = () => mount(BookingView, {
  global: { stubs: { RouterLink: { template: '<a><slot /></a>' } } },
})

describe('BookingView', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.mocked(bookingApi.fetchLocations).mockResolvedValue(mockLocations)
    vi.mocked(bookingApi.fetchSlots).mockResolvedValue(mockSlots)
    vi.mocked(bookingApi.createBooking).mockResolvedValue({
      bookingId: 'bk-001', slotId: 'a-2026-05-26-09:00', startTime: '09:00', name: 'Jane', date: '2026-05-26',
    })
  })

  it('renders AppHeader', () => {
    const wrapper = mountView()
    expect(wrapper.find('.app-header').exists()).toBe(true)
  })

  it('calls store.loadLocations on mount', async () => {
    const store = useBookingStore()
    const spy = vi.spyOn(store, 'loadLocations').mockResolvedValue()
    mountView()
    await vi.waitFor(() => expect(spy).toHaveBeenCalled())
  })

  it('renders LocationPicker on mount', () => {
    const wrapper = mountView()
    expect(wrapper.find('.location-picker').exists()).toBe(true)
  })

  it('hides DateStrip when no location is selected', () => {
    const wrapper = mountView()
    expect(wrapper.find('.date-strip').exists()).toBe(false)
  })

  it('hides SlotGrid when no location is selected', () => {
    const wrapper = mountView()
    expect(wrapper.find('.slot-grid').exists()).toBe(false)
  })

  it('shows DateStrip after a location is selected', async () => {
    const store = useBookingStore()
    const wrapper = mountView()
    store.selectLocation(mockLocations[0]!)
    await wrapper.vm.$nextTick()
    expect(wrapper.find('.date-strip').exists()).toBe(true)
  })

  it('shows SlotGrid after a location is selected', async () => {
    const store = useBookingStore()
    const wrapper = mountView()
    store.selectLocation(mockLocations[0]!)
    await wrapper.vm.$nextTick()
    expect(wrapper.find('.slot-grid').exists()).toBe(true)
  })

  it('calls store.loadSlots with locationId when location is selected', async () => {
    const store = useBookingStore()
    store.locations = mockLocations
    const spy = vi.spyOn(store, 'loadSlots').mockResolvedValue()
    const wrapper = mountView()
    await wrapper.find('.location-picker button').trigger('click')
    await vi.waitFor(() => expect(spy).toHaveBeenCalledWith(expect.any(String), 'a'))
  })

  it('BookingModal is not visible initially', () => {
    const wrapper = mountView()
    expect(wrapper.find('.modal-overlay').exists()).toBe(false)
  })

  it('opens BookingModal when a slot is selected', async () => {
    const store = useBookingStore()
    store.slots = mockSlots
    store.selectLocation(mockLocations[0]!)
    const wrapper = mountView()
    store.selectSlot(mockSlots[0]!)
    await wrapper.vm.$nextTick()
    expect(wrapper.find('.modal-overlay').exists()).toBe(true)
  })

  it('closes BookingModal when clearSelectedSlot is called', async () => {
    const store = useBookingStore()
    store.slots = mockSlots
    store.selectLocation(mockLocations[0]!)
    store.selectSlot(mockSlots[0]!)
    const wrapper = mountView()
    await wrapper.vm.$nextTick()
    store.clearSelectedSlot()
    await wrapper.vm.$nextTick()
    expect(wrapper.find('.modal-overlay').exists()).toBe(false)
  })

  it('reloads slots after a successful booking when modal closes', async () => {
    const store = useBookingStore()
    store.slots = mockSlots
    store.selectLocation(mockLocations[0]!)
    store.selectSlot(mockSlots[0]!)
    store.bookingStatus = 'success'
    vi.spyOn(store, 'loadLocations').mockResolvedValue()
    const spy = vi.spyOn(store, 'loadSlots').mockResolvedValue()
    const wrapper = mountView()
    await wrapper.vm.$nextTick()
    await wrapper.findComponent({ name: 'BookingModal' }).vm.$emit('close')
    await wrapper.vm.$nextTick()
    expect(spy).toHaveBeenCalled()
  })
})
