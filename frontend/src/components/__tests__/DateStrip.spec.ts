import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import DateStrip from '../DateStrip.vue'

const mockDates = [
  { iso: '2026-01-05', dayName: 'Mon', dayNum: 5 },
  { iso: '2026-01-06', dayName: 'Tue', dayNum: 6 },
  { iso: '2026-01-07', dayName: 'Wed', dayNum: 7 },
  { iso: '2026-01-08', dayName: 'Thu', dayNum: 8 },
  { iso: '2026-01-09', dayName: 'Fri', dayNum: 9 },
  { iso: '2026-01-10', dayName: 'Sat', dayNum: 10 },
  { iso: '2026-01-11', dayName: 'Sun', dayNum: 11 },
]

describe('DateStrip', () => {
  it('renders exactly 7 date buttons', () => {
    const wrapper = mount(DateStrip, { props: { dates: mockDates, selectedDate: '2026-01-05' } })
    expect(wrapper.findAll('button')).toHaveLength(7)
  })

  it('emits select-date with ISO date string when a button is clicked', async () => {
    const wrapper = mount(DateStrip, { props: { dates: mockDates, selectedDate: '2026-01-05' } })
    const buttons = wrapper.findAll('button')
    await buttons[0]!.trigger('click')
    const emitted = wrapper.emitted('select-date')
    expect(emitted).toBeTruthy()
    expect(emitted![0]![0]).toMatch(/^\d{4}-\d{2}-\d{2}$/)
  })

  it('applies active class to the selected date button', () => {
    const wrapper = mount(DateStrip, { props: { dates: mockDates, selectedDate: '2026-01-05' } })
    const activeButton = wrapper.findAll('button').find((b) => b.classes('active'))
    expect(activeButton).toBeTruthy()
  })

  it('shows day name abbreviation on each button', () => {
    const wrapper = mount(DateStrip, { props: { dates: mockDates, selectedDate: '2026-01-05' } })
    const text = wrapper.findAll('button')[0]!.text()
    expect(text).toMatch(/Mon|Tue|Wed|Thu|Fri|Sat|Sun/)
  })

  it('shows day number on each button', () => {
    const wrapper = mount(DateStrip, { props: { dates: mockDates, selectedDate: '2026-01-05' } })
    const text = wrapper.findAll('button')[0]!.text()
    expect(text).toMatch(/\d+/)
  })

  it('first button corresponds to first date in dates prop', async () => {
    const wrapper = mount(DateStrip, { props: { dates: mockDates, selectedDate: '2026-01-05' } })
    await wrapper.findAll('button')[0]!.trigger('click')
    const emitted = wrapper.emitted('select-date')!
    expect(emitted[0]![0]).toBe('2026-01-05')
  })

  it('past date button is disabled when its iso is before today', () => {
    const today = '2026-01-07'
    const wrapper = mount(DateStrip, { props: { dates: mockDates, selectedDate: today, today } })
    // first two buttons (Jan 5, Jan 6) are in the past
    expect(wrapper.findAll('button')[0]!.attributes('disabled')).toBeDefined()
    expect(wrapper.findAll('button')[1]!.attributes('disabled')).toBeDefined()
    expect(wrapper.findAll('button')[2]!.attributes('disabled')).toBeUndefined()
  })

  it('clicking a past date button does not emit select-date', async () => {
    const today = '2026-01-07'
    const wrapper = mount(DateStrip, { props: { dates: mockDates, selectedDate: today, today } })
    await wrapper.findAll('button')[0]!.trigger('click')
    expect(wrapper.emitted('select-date')).toBeFalsy()
  })
})
