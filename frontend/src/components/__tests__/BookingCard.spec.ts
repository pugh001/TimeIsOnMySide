import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import BookingCard from '../BookingCard.vue'

const mockBooking = {
  bookingRef: 'bk-test001',
  date: '2026-12-10',
  startTime: '10:00',
  endTime: '10:30',
  customerName: 'Alice Smith',
}

describe('BookingCard', () => {
  it('renders booking ref', () => {
    const wrapper = mount(BookingCard, { props: { booking: mockBooking } })
    expect(wrapper.text()).toContain('bk-test001')
  })

  it('renders customer name', () => {
    const wrapper = mount(BookingCard, { props: { booking: mockBooking } })
    expect(wrapper.text()).toContain('Alice Smith')
  })

  it('renders date', () => {
    const wrapper = mount(BookingCard, { props: { booking: mockBooking } })
    expect(wrapper.text()).toContain('2026-12-10')
  })

  it('renders time range', () => {
    const wrapper = mount(BookingCard, { props: { booking: mockBooking } })
    expect(wrapper.text()).toContain('10:00')
    expect(wrapper.text()).toContain('10:30')
  })

  it('has data-testid booking-card', () => {
    const wrapper = mount(BookingCard, { props: { booking: mockBooking } })
    expect(wrapper.find('[data-testid="booking-card"]').exists()).toBe(true)
  })
})
