import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import SlotCard from '../SlotCard.vue'

const availableSlot = {
  id: 'a-2026-05-26-09:00',
  date: '2026-05-26',
  startTime: '09:00',
  endTime: '09:30',
  status: 'available' as const,
  locationId: 'a',
}

const unavailableSlot = { ...availableSlot, status: 'unavailable' as const }

describe('SlotCard', () => {
  it('renders the slot startTime', () => {
    const wrapper = mount(SlotCard, { props: { slotData: availableSlot } })
    expect(wrapper.text()).toContain('09:00')
  })

  it('emits select when available slot is clicked', async () => {
    const wrapper = mount(SlotCard, { props: { slotData: availableSlot } })
    await wrapper.trigger('click')
    expect(wrapper.emitted('select')).toBeTruthy()
  })

  it('does not emit select when unavailable slot is clicked', async () => {
    const wrapper = mount(SlotCard, { props: { slotData: unavailableSlot } })
    await wrapper.trigger('click')
    expect(wrapper.emitted('select')).toBeFalsy()
  })

  it('has aria-disabled=true when unavailable', () => {
    const wrapper = mount(SlotCard, { props: { slotData: unavailableSlot } })
    expect(wrapper.attributes('aria-disabled')).toBe('true')
  })

  it('has aria-disabled=false when available', () => {
    const wrapper = mount(SlotCard, { props: { slotData: availableSlot } })
    expect(wrapper.attributes('aria-disabled')).toBe('false')
  })

  it('has an aria-label containing the start time', () => {
    const wrapper = mount(SlotCard, { props: { slotData: availableSlot } })
    expect(wrapper.attributes('aria-label')).toContain('09:00')
  })

  it('has an aria-label indicating available status', () => {
    const wrapper = mount(SlotCard, { props: { slotData: availableSlot } })
    expect(wrapper.attributes('aria-label')).toContain('available')
  })

  it('has an aria-label indicating unavailable status', () => {
    const wrapper = mount(SlotCard, { props: { slotData: unavailableSlot } })
    expect(wrapper.attributes('aria-label')).toContain('unavailable')
  })

  it('has disabled attribute when slot is unavailable', () => {
    const wrapper = mount(SlotCard, { props: { slotData: unavailableSlot } })
    expect(wrapper.attributes('disabled')).toBeDefined()
  })

  it('does not have disabled attribute when slot is available', () => {
    const wrapper = mount(SlotCard, { props: { slotData: availableSlot } })
    expect(wrapper.attributes('disabled')).toBeUndefined()
  })
})
