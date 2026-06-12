import { describe, it, expect, beforeEach } from 'vitest'
import { fetchSlots, createBooking, resetMockStore, fetchLocations } from '../booking.mock'
import { LocationSchema } from '../schemas'
import { ZodError } from 'zod'

describe('fetchLocations', () => {
  it('returns exactly 3 locations', async () => {
    const locations = await fetchLocations()
    expect(locations).toHaveLength(3)
  })

  it('each location passes LocationSchema', async () => {
    const locations = await fetchLocations()
    for (const loc of locations) {
      expect(() => LocationSchema.parse(loc)).not.toThrow()
    }
  })

  it('has ids a, b, c and correct names', async () => {
    const locations = await fetchLocations()
    expect(locations[0]).toMatchObject({ id: 'a', name: 'Location A' })
    expect(locations[1]).toMatchObject({ id: 'b', name: 'Location B' })
    expect(locations[2]).toMatchObject({ id: 'c', name: 'Location C' })
  })
})

describe('fetchSlots', () => {
  beforeEach(() => resetMockStore())

  it('returns 16 slots for a weekday', async () => {
    const slots = await fetchSlots('2026-05-26', 'a')
    expect(slots).toHaveLength(16)
  })

  it('returns empty array for a Saturday', async () => {
    const slots = await fetchSlots('2026-05-23', 'a')
    expect(slots).toHaveLength(0)
  })

  it('returns empty array for a Sunday', async () => {
    const slots = await fetchSlots('2026-05-24', 'a')
    expect(slots).toHaveLength(0)
  })

  it('first slot starts at 09:00', async () => {
    const slots = await fetchSlots('2026-05-26', 'a')
    expect(slots[0]!.startTime).toBe('09:00')
  })

  it('last slot starts at 16:30', async () => {
    const slots = await fetchSlots('2026-05-26', 'a')
    expect(slots[15]!.startTime).toBe('16:30')
  })

  it('slot ids include locationId prefix', async () => {
    const slots = await fetchSlots('2026-05-26', 'a')
    expect(slots[0]!.id).toBe('a-2026-05-26-09:00')
  })

  it('pre-seeded slot for location a (09:00) is unavailable', async () => {
    const slots = await fetchSlots('2026-05-26', 'a')
    const preseed = slots.find((s) => s.startTime === '09:00')
    expect(preseed?.status).toBe('unavailable')
  })

  it('pre-seeded slot for location b (10:00) is unavailable', async () => {
    const slots = await fetchSlots('2026-05-26', 'b')
    const preseed = slots.find((s) => s.startTime === '10:00')
    expect(preseed?.status).toBe('unavailable')
  })

  it('pre-seeded slot for location c (10:30) is unavailable', async () => {
    const slots = await fetchSlots('2026-05-26', 'c')
    const preseed = slots.find((s) => s.startTime === '10:30')
    expect(preseed?.status).toBe('unavailable')
  })

  it('all other slots for location a are available', async () => {
    const slots = await fetchSlots('2026-05-26', 'a')
    const others = slots.filter((s) => s.startTime !== '09:00')
    expect(others.every((s) => s.status === 'available')).toBe(true)
  })

  it('returns slots with valid Zod-parsed shape', async () => {
    const slots = await fetchSlots('2026-05-26', 'a')
    expect(slots[0]).toMatchObject({
      id: expect.any(String),
      date: '2026-05-26',
      startTime: expect.any(String),
      endTime: expect.any(String),
      locationId: 'a',
    })
  })
})

describe('createBooking', () => {
  beforeEach(() => resetMockStore())

  const validRequest = {
    slotId: 'a-2026-05-26-10:00',
    name: 'Jane Doe',
    email: 'jane@example.com',
    phone: '0831231234',
  }

  it('returns a booking response for a valid request', async () => {
    await fetchSlots('2026-05-26', 'a')
    const response = await createBooking(validRequest)
    expect(response.bookingId).toBeTruthy()
    expect(response.slotId).toBe('a-2026-05-26-10:00')
    expect(response.startTime).toBe('10:00')
  })

  it('marks the slot as unavailable after booking', async () => {
    await fetchSlots('2026-05-26', 'a')
    await createBooking(validRequest)
    const slots = await fetchSlots('2026-05-26', 'a')
    const bookedSlot = slots.find((s) => s.id === 'a-2026-05-26-10:00')
    expect(bookedSlot?.status).toBe('unavailable')
  })

  it('throws an error when slot is already booked', async () => {
    await fetchSlots('2026-05-26', 'a')
    await createBooking(validRequest)
    await expect(createBooking(validRequest)).rejects.toThrow('Slot already booked')
  })

  it('throws ZodError for invalid email', async () => {
    await fetchSlots('2026-05-26', 'a')
    await expect(
      createBooking({ ...validRequest, email: 'not-an-email' }),
    ).rejects.toBeInstanceOf(ZodError)
  })

  it('throws ZodError for empty name', async () => {
    await fetchSlots('2026-05-26', 'a')
    await expect(createBooking({ ...validRequest, name: '' })).rejects.toBeInstanceOf(ZodError)
  })
})
