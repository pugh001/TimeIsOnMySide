import { describe, it, expect, beforeEach, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import { setActivePinia, createPinia } from 'pinia'
import { useBookingStore } from '@/stores/bookingStore'
import BookingModal from '../BookingModal.vue'
import * as bookingApi from '@/api/booking'

vi.mock('@/api/booking')

const selectedSlot = {
  id: 'a-2026-05-26-09:00',
  date: '2026-05-26',
  startTime: '09:00',
  endTime: '09:30',
  status: 'available' as const,
  locationId: 'a',
}

describe('BookingModal', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.mocked(bookingApi.fetchSlots).mockResolvedValue([])
    vi.mocked(bookingApi.createBooking).mockResolvedValue({
      bookingId: 'bk-001', slotId: '2026-05-26-09:00', startTime: '09:00', name: 'Jane', date: '2026-05-26',
    })
  })

  it('renders the modal overlay when mounted', () => {
    const wrapper = mount(BookingModal, { props: { slotData: selectedSlot } })
    expect(wrapper.find('.modal-overlay').exists()).toBe(true)
  })

  it('renders name input field', () => {
    const wrapper = mount(BookingModal, { props: { slotData: selectedSlot } })
    expect(wrapper.find('input[name="name"]').exists()).toBe(true)
  })

  it('renders email input field', () => {
    const wrapper = mount(BookingModal, { props: { slotData: selectedSlot } })
    expect(wrapper.find('input[name="email"]').exists()).toBe(true)
  })

  it('renders phone input field', () => {
    const wrapper = mount(BookingModal, { props: { slotData: selectedSlot } })
    expect(wrapper.find('input[name="phone"]').exists()).toBe(true)
  })

  it('renders notes textarea', () => {
    const wrapper = mount(BookingModal, { props: { slotData: selectedSlot } })
    expect(wrapper.find('textarea[name="notes"]').exists()).toBe(true)
  })

  it('emits close when cancel button is clicked', async () => {
    const wrapper = mount(BookingModal, { props: { slotData: selectedSlot } })
    await wrapper.find('button.cancel').trigger('click')
    expect(wrapper.emitted('close')).toBeTruthy()
  })

  it('shows the selected slot start time in the header', () => {
    const wrapper = mount(BookingModal, { props: { slotData: selectedSlot } })
    expect(wrapper.find('.modal-header').text()).toContain('09:00')
  })

  it('shows success state after store status becomes success', async () => {
    const store = useBookingStore()
    const wrapper = mount(BookingModal, { props: { slotData: selectedSlot } })
    store.bookingStatus = 'success'
    await wrapper.vm.$nextTick()
    expect(wrapper.find('.success-state').exists()).toBe(true)
  })

  it('shows confirmation date and time in success state', async () => {
    const store = useBookingStore()
    store.bookingConfirmation = { bookingId: 'bk-001', slotId: 'slot-001', startTime: '09:00', name: 'Jane', date: '2026-05-26' }
    store.bookingStatus = 'success'
    const wrapper = mount(BookingModal, { props: { slotData: selectedSlot } })
    await wrapper.vm.$nextTick()
    expect(wrapper.find('.confirmation-detail').text()).toContain('2026-05-26')
    expect(wrapper.find('.confirmation-detail').text()).toContain('09:00')
  })

  it('renders overlay when slot is null', () => {
    const wrapper = mount(BookingModal, { props: { slotData: null } })
    expect(wrapper.find('.modal-overlay').exists()).toBe(true)
  })

  it('does not render slot time when slot is null', () => {
    const wrapper = mount(BookingModal, { props: { slotData: null } })
    expect(wrapper.find('.slot-time').text()).toBe('')
  })

  it('emits close when Escape key is pressed', async () => {
    const wrapper = mount(BookingModal, { props: { slotData: selectedSlot }, attachTo: document.body })
    document.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape' }))
    await wrapper.vm.$nextTick()
    expect(wrapper.emitted('close')).toBeTruthy()
    wrapper.unmount()
  })

  it('has aria-labelledby on the dialog pointing to the h2', () => {
    const wrapper = mount(BookingModal, { props: { slotData: selectedSlot } })
    const dialog = wrapper.find('[role="dialog"]')
    const labelId = dialog.attributes('aria-labelledby')
    expect(labelId).toBeTruthy()
    expect(wrapper.find(`#${labelId}`).exists()).toBe(true)
  })
})
