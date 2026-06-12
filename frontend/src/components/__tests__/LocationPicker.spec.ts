import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import LocationPicker from '../LocationPicker.vue'
import type { Location } from '@/api/schemas'

const mockLocations: Location[] = [
  { id: 'a', slug: 'location-a', name: 'Location A' },
  { id: 'b', slug: 'location-b', name: 'Location B' },
  { id: 'c', slug: 'location-c', name: 'Location C' },
]

describe('LocationPicker', () => {
  it('renders one button per location', () => {
    const wrapper = mount(LocationPicker, {
      props: { locations: mockLocations, selectedLocation: null, status: 'idle' },
    })
    const buttons = wrapper.findAll('button')
    expect(buttons).toHaveLength(3)
    expect(buttons[0]!.text()).toContain('Location A')
    expect(buttons[1]!.text()).toContain('Location B')
    expect(buttons[2]!.text()).toContain('Location C')
  })

  it('emits select-location with the location object when a button is clicked', async () => {
    const wrapper = mount(LocationPicker, {
      props: { locations: mockLocations, selectedLocation: null, status: 'idle' },
    })
    await wrapper.findAll('button')[1]!.trigger('click')
    expect(wrapper.emitted('select-location')).toBeTruthy()
    expect(wrapper.emitted('select-location')![0]).toEqual([mockLocations[1]])
  })

  it('applies selected class to the active location button', () => {
    const wrapper = mount(LocationPicker, {
      props: { locations: mockLocations, selectedLocation: mockLocations[0]!, status: 'idle' },
    })
    const buttons = wrapper.findAll('button')
    expect(buttons[0]!.classes()).toContain('selected')
    expect(buttons[1]!.classes()).not.toContain('selected')
    expect(buttons[2]!.classes()).not.toContain('selected')
  })

  it('shows loading indicator when status is loading', () => {
    const wrapper = mount(LocationPicker, {
      props: { locations: [], selectedLocation: null, status: 'loading' },
    })
    expect(wrapper.find('[data-testid="loading"]').exists()).toBe(true)
    expect(wrapper.findAll('button')).toHaveLength(0)
  })

  it('does NOT show loading indicator when status is idle with empty locations', () => {
    const wrapper = mount(LocationPicker, {
      props: { locations: [], selectedLocation: null, status: 'idle' },
    })
    expect(wrapper.find('[data-testid="loading"]').exists()).toBe(false)
  })

  it('shows error state when status is error', () => {
    const wrapper = mount(LocationPicker, {
      props: { locations: [], selectedLocation: null, status: 'error' },
    })
    expect(wrapper.find('[data-testid="locations-error"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="locations-error"]').text()).toContain('Could not load')
  })

  it('shows unavailable state when status is idle and locations is empty', () => {
    const wrapper = mount(LocationPicker, {
      props: { locations: [], selectedLocation: null, status: 'idle' },
    })
    expect(wrapper.find('[data-testid="unavailable"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="unavailable"]').text()).toContain('No availability')
  })

  it('renders a search input when there are locations', () => {
    const wrapper = mount(LocationPicker, {
      props: { locations: mockLocations, selectedLocation: null, status: 'idle' },
    })
    expect(wrapper.find('[data-testid="location-search"]').exists()).toBe(true)
  })

  it('renders locations in a scrollable list container', () => {
    const wrapper = mount(LocationPicker, {
      props: { locations: mockLocations, selectedLocation: null, status: 'idle' },
    })
    expect(wrapper.find('[data-testid="location-list"]').exists()).toBe(true)
  })

  it('filters visible locations as the user types in the search box', async () => {
    const wrapper = mount(LocationPicker, {
      props: { locations: mockLocations, selectedLocation: null, status: 'idle' },
    })
    await wrapper.find('[data-testid="location-search"]').setValue('B')
    const buttons = wrapper.findAll('[data-testid="location-list"] button')
    expect(buttons).toHaveLength(1)
    expect(buttons[0]!.text()).toContain('Location B')
  })

  it('shows a no-results message when search matches nothing', async () => {
    const wrapper = mount(LocationPicker, {
      props: { locations: mockLocations, selectedLocation: null, status: 'idle' },
    })
    await wrapper.find('[data-testid="location-search"]').setValue('zzz')
    expect(wrapper.find('[data-testid="no-results"]').exists()).toBe(true)
  })

  it('shows a checkmark indicator on the selected row', () => {
    const wrapper = mount(LocationPicker, {
      props: { locations: mockLocations, selectedLocation: mockLocations[0]!, status: 'idle' },
    })
    expect(wrapper.find('[data-testid="location-list"] button.selected [data-testid="selected-indicator"]').exists()).toBe(true)
  })

  it('all location list-row buttons have type="button" to prevent accidental form submission', () => {
    const wrapper = mount(LocationPicker, {
      props: { locations: mockLocations, selectedLocation: null, status: 'idle' },
    })
    const buttons = wrapper.findAll('[data-testid="location-list"] button')
    expect(buttons.length).toBeGreaterThan(0)
    for (const btn of buttons) {
      expect(btn.attributes('type')).toBe('button')
    }
  })
})
