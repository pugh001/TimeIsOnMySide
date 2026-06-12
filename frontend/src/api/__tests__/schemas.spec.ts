import { describe, it, expect } from 'vitest'
import {
  SlotSchema,
  BookingRequestSchema,
  BookingResponseSchema,
  StaffBookingSchema,
  LocationSchema,
  DayHoursSchema,
  CreateUserRequestSchema,
  WorkingTimeSchema,
  DAY_KEYS,
} from '../schemas'

describe('LocationSchema', () => {
  it('parses a valid location', () => {
    const result = LocationSchema.parse({ id: 'uuid-a', slug: 'location-a', name: 'Location A' })
    expect(result.id).toBe('uuid-a')
    expect(result.slug).toBe('location-a')
    expect(result.name).toBe('Location A')
  })

  it('parses a location with optional address', () => {
    const result = LocationSchema.parse({ id: 'uuid-a', slug: 'location-a', name: 'Location A', address: '1 Main St' })
    expect(result.address).toBe('1 Main St')
  })

  it('parses a location without address (address optional)', () => {
    const result = LocationSchema.parse({ id: 'uuid-a', slug: 'location-a', name: 'Location A' })
    expect(result.address).toBeUndefined()
  })

  it('parses a location with openingHours', () => {
    const result = LocationSchema.parse({
      id: 'uuid-a', slug: 'location-a', name: 'Location A',
      openingHours: {
        monday: { openTime: '09:00', closeTime: '17:00' },
        tuesday: null,
      },
    })
    expect(result.openingHours?.monday?.openTime).toBe('09:00')
    expect(result.openingHours?.tuesday).toBeNull()
  })

  it('parses a location without openingHours (optional)', () => {
    const result = LocationSchema.parse({ id: 'uuid-a', slug: 'location-a', name: 'Location A' })
    expect(result.openingHours).toBeUndefined()
  })

  it('rejects missing id', () => {
    expect(() => LocationSchema.parse({ slug: 'loc', name: 'Location A' })).toThrow(/invalid|too_small|ZodError/i)
  })

  it('rejects missing slug', () => {
    expect(() => LocationSchema.parse({ id: 'uuid-a', name: 'Location A' })).toThrow(/invalid|too_small|ZodError/i)
  })

  it('rejects missing name', () => {
    expect(() => LocationSchema.parse({ id: 'uuid-a', slug: 'loc' })).toThrow(/invalid|too_small|ZodError/i)
  })

  it('rejects empty id', () => {
    expect(() => LocationSchema.parse({ id: '', slug: 'loc', name: 'Location A' })).toThrow(/invalid|too_small|ZodError/i)
  })
})

describe('SlotSchema', () => {
  const validSlot = {
    id: 'a-2026-05-26-09:00',
    date: '2026-05-26',
    startTime: '09:00',
    endTime: '09:30',
    status: 'available' as const,
    locationId: 'a',
  }

  it('parses a valid available slot', () => {
    const result = SlotSchema.parse(validSlot)
    expect(result.id).toBe('a-2026-05-26-09:00')
    expect(result.status).toBe('available')
  })

  it('parses a valid unavailable slot', () => {
    const result = SlotSchema.parse({ ...validSlot, status: 'unavailable' })
    expect(result.status).toBe('unavailable')
  })

  it('rejects unknown status value', () => {
    expect(() => SlotSchema.parse({ ...validSlot, status: 'booked' })).toThrow(/invalid|too_small|ZodError/i)
  })

  it('rejects missing id', () => {
    const { id: _id, ...withoutId } = validSlot
    expect(() => SlotSchema.parse(withoutId)).toThrow(/invalid|too_small|ZodError/i)
  })

  it('rejects missing date', () => {
    const { date: _date, ...withoutDate } = validSlot
    expect(() => SlotSchema.parse(withoutDate)).toThrow(/invalid|too_small|ZodError/i)
  })

  it('parses a slot with locationId', () => {
    const result = SlotSchema.parse(validSlot)
    expect(result.locationId).toBe('a')
  })

  it('rejects a slot missing locationId', () => {
    const { locationId: _lid, ...withoutLid } = validSlot
    expect(() => SlotSchema.parse(withoutLid)).toThrow(/invalid|too_small|ZodError/i)
  })
})

describe('BookingRequestSchema', () => {
  const validRequest = {
    slotId: 'slot-001',
    name: 'Jane Doe',
    email: 'jane@example.com',
    phone: '0831231234',
  }

  it('parses a valid booking request without notes', () => {
    const result = BookingRequestSchema.parse(validRequest)
    expect(result.name).toBe('Jane Doe')
    expect(result.notes).toBeUndefined()
  })

  it('parses a valid booking request with notes', () => {
    const result = BookingRequestSchema.parse({ ...validRequest, notes: 'Discuss project scope' })
    expect(result.notes).toBe('Discuss project scope')
  })

  it('rejects invalid email', () => {
    expect(() => BookingRequestSchema.parse({ ...validRequest, email: 'not-an-email' })).toThrow(/invalid|too_small|ZodError/i)
  })

  it('rejects empty name', () => {
    expect(() => BookingRequestSchema.parse({ ...validRequest, name: '' })).toThrow(/invalid|too_small|ZodError/i)
  })

  it('rejects empty phone', () => {
    expect(() => BookingRequestSchema.parse({ ...validRequest, phone: '' })).toThrow(/invalid|too_small|ZodError/i)
  })

  it('rejects single-character phone', () => {
    expect(() => BookingRequestSchema.parse({ ...validRequest, phone: '5' })).toThrow(/invalid|too_small|ZodError/i)
  })

  it('rejects clearly invalid phone string', () => {
    expect(() => BookingRequestSchema.parse({ ...validRequest, phone: 'abc' })).toThrow(/invalid|too_small|ZodError/i)
  })

  it('accepts valid SA phone number', () => {
    expect(() => BookingRequestSchema.parse({ ...validRequest, phone: '0831231234' })).not.toThrow()
  })

  it('accepts another valid SA phone number', () => {
    expect(() => BookingRequestSchema.parse({ ...validRequest, phone: '0712345678' })).not.toThrow()
  })

  it('rejects international format', () => {
    expect(() => BookingRequestSchema.parse({ ...validRequest, phone: '+1234567890' })).toThrow(/invalid|ZodError/i)
  })

  it('rejects number without leading zero', () => {
    expect(() => BookingRequestSchema.parse({ ...validRequest, phone: '1234567890' })).toThrow(/invalid|ZodError/i)
  })

  it('rejects 9-digit number', () => {
    expect(() => BookingRequestSchema.parse({ ...validRequest, phone: '083123123' })).toThrow(/invalid|ZodError/i)
  })

  it('rejects 11-digit number', () => {
    expect(() => BookingRequestSchema.parse({ ...validRequest, phone: '08312312345' })).toThrow(/invalid|ZodError/i)
  })

  it('rejects missing slotId', () => {
    const { slotId: _slotId, ...withoutSlotId } = validRequest
    expect(() => BookingRequestSchema.parse(withoutSlotId)).toThrow(/invalid|too_small|ZodError/i)
  })
})

describe('BookingResponseSchema', () => {
  const validResponse = {
    bookingId: 'bk-001',
    slotId: 'slot-001',
    startTime: '09:00',
    name: 'Jane Doe',
    date: '2026-05-26',
  }

  it('parses a valid booking response', () => {
    const result = BookingResponseSchema.parse(validResponse)
    expect(result.bookingId).toBe('bk-001')
  })

  it('includes name in parsed response', () => {
    const result = BookingResponseSchema.parse(validResponse)
    expect(result.name).toBe('Jane Doe')
  })

  it('includes date in parsed response', () => {
    const result = BookingResponseSchema.parse(validResponse)
    expect(result.date).toBe('2026-05-26')
  })

  it('rejects missing bookingId', () => {
    const { bookingId: _b, ...without } = validResponse
    expect(() => BookingResponseSchema.parse(without)).toThrow(/invalid|too_small|ZodError/i)
  })

  it('rejects missing name', () => {
    const { name: _n, ...without } = validResponse
    expect(() => BookingResponseSchema.parse(without)).toThrow(/invalid|too_small|ZodError/i)
  })

  it('rejects missing date', () => {
    const { date: _d, ...without } = validResponse
    expect(() => BookingResponseSchema.parse(without)).toThrow(/invalid|too_small|ZodError/i)
  })
})

describe('DayHoursSchema', () => {
  it('parses a valid span (08:00–17:00)', () => {
    expect(() => DayHoursSchema.parse({ openTime: '08:00', closeTime: '17:00' })).not.toThrow()
  })


  it('rejects closeTime before openTime', () => {
    expect(() => DayHoursSchema.parse({ openTime: '17:00', closeTime: '08:00' })).toThrow(/after/i)
  })

  it('rejects closeTime equal to openTime', () => {
    expect(() => DayHoursSchema.parse({ openTime: '09:00', closeTime: '09:00' })).toThrow(/after|1 hour/i)
  })

  it('rejects span of exactly 59 minutes (09:00–09:59)', () => {
    expect(() => DayHoursSchema.parse({ openTime: '09:00', closeTime: '09:59' })).toThrow(/1 hour/i)
  })

  it('accepts span of exactly 60 minutes (09:00–10:00)', () => {
    expect(() => DayHoursSchema.parse({ openTime: '09:00', closeTime: '10:00' })).not.toThrow()
  })

  it('rejects midnight-crossing times (23:00–01:00)', () => {
    expect(() => DayHoursSchema.parse({ openTime: '23:00', closeTime: '01:00' })).toThrow(/after/i)
  })

  it('accepts openTime at 00:00 with a 61-minute span (00:00–01:01)', () => {
    expect(() => DayHoursSchema.parse({ openTime: '00:00', closeTime: '01:01' })).not.toThrow()
  })

  it('accepts closeTime at 23:59 with a valid span (08:00–23:59)', () => {
    expect(() => DayHoursSchema.parse({ openTime: '08:00', closeTime: '23:59' })).not.toThrow()
  })
})

describe('WorkingTimeSchema', () => {
  it('parses a valid working time', () => {
    expect(() => WorkingTimeSchema.parse({ day: 'monday', shiftStart: '09:00', shiftEnd: '17:00' })).not.toThrow()
  })

  it('rejects invalid day', () => {
    expect(() => WorkingTimeSchema.parse({ day: 'funday', shiftStart: '09:00', shiftEnd: '17:00' })).toThrow()
  })

  it('rejects bad shiftStart format', () => {
    expect(() => WorkingTimeSchema.parse({ day: 'monday', shiftStart: '9:00', shiftEnd: '17:00' })).toThrow(/HH:MM/i)
  })

  it('rejects bad shiftEnd format', () => {
    expect(() => WorkingTimeSchema.parse({ day: 'monday', shiftStart: '09:00', shiftEnd: '5pm' })).toThrow(/HH:MM/i)
  })

  it('accepts all valid day keys', () => {
    for (const day of DAY_KEYS) {
      expect(() => WorkingTimeSchema.parse({ day, shiftStart: '09:00', shiftEnd: '17:00' })).not.toThrow()
    }
  })
})

describe('CreateUserRequestSchema', () => {
  const validUser = {
    firstName: 'Jane',
    lastName: 'Doe',
    password: 'secret99',
    locationId: 'loc-abc',
    workingTimes: [{ day: 'monday', shiftStart: '09:00', shiftEnd: '17:00' }],
  }

  it('parses a valid user payload', () => {
    expect(() => CreateUserRequestSchema.parse(validUser)).not.toThrow()
  })

  it('accepts a shift of exactly 8 hours (480 min)', () => {
    expect(() => CreateUserRequestSchema.parse({
      ...validUser,
      workingTimes: [{ day: 'monday', shiftStart: '09:00', shiftEnd: '17:00' }],
    })).not.toThrow()
  })

  it('accepts a midnight-crossing shift of exactly 8 hours (23:00–07:00)', () => {
    expect(() => CreateUserRequestSchema.parse({
      ...validUser,
      workingTimes: [{ day: 'monday', shiftStart: '23:00', shiftEnd: '07:00' }],
    })).not.toThrow()
  })

  it('rejects a shift longer than 8 hours', () => {
    expect(() => CreateUserRequestSchema.parse({
      ...validUser,
      workingTimes: [{ day: 'monday', shiftStart: '09:00', shiftEnd: '17:01' }],
    })).toThrow(/8 hours/i)
  })

  it('rejects a midnight-crossing shift longer than 8 hours (22:00–07:00 = 9h)', () => {
    expect(() => CreateUserRequestSchema.parse({
      ...validUser,
      workingTimes: [{ day: 'monday', shiftStart: '22:00', shiftEnd: '07:00' }],
    })).toThrow(/8 hours/i)
  })

  it('rejects a password shorter than 8 characters', () => {
    expect(() => CreateUserRequestSchema.parse({ ...validUser, password: 'short' })).toThrow(/8 characters/i)
  })

  it('rejects a missing locationId', () => {
    const { locationId: _lid, ...without } = validUser
    expect(() => CreateUserRequestSchema.parse(without)).toThrow(/required|invalid/i)
  })

  it('rejects an empty firstName', () => {
    expect(() => CreateUserRequestSchema.parse({ ...validUser, firstName: '' })).toThrow(/first name/i)
  })

  it('accepts workingTimes with a single day', () => {
    expect(() => CreateUserRequestSchema.parse({
      ...validUser,
      workingTimes: [{ day: 'monday', shiftStart: '09:00', shiftEnd: '17:00' }],
    })).not.toThrow()
  })

  it('accepts workingTimes with multiple days', () => {
    expect(() => CreateUserRequestSchema.parse({
      ...validUser,
      workingTimes: [
        { day: 'monday', shiftStart: '09:00', shiftEnd: '17:00' },
        { day: 'wednesday', shiftStart: '09:00', shiftEnd: '17:00' },
        { day: 'friday', shiftStart: '09:00', shiftEnd: '17:00' },
      ],
    })).not.toThrow()
  })

  it('accepts workingTimes with all 7 days', () => {
    expect(() => CreateUserRequestSchema.parse({
      ...validUser,
      workingTimes: DAY_KEYS.map(day => ({ day, shiftStart: '09:00', shiftEnd: '17:00' })),
    })).not.toThrow()
  })

  it('rejects workingTimes as empty array', () => {
    expect(() => CreateUserRequestSchema.parse({ ...validUser, workingTimes: [] })).toThrow(/at least one/i)
  })

  it('rejects missing workingTimes field', () => {
    const { workingTimes: _wt, ...without } = validUser
    expect(() => CreateUserRequestSchema.parse(without)).toThrow(/invalid|required/i)
  })
})

describe('StaffBookingSchema', () => {
  const validBooking = {
    bookingRef:   'bk-abc12345',
    date:         '2026-06-10',
    startTime:    '09:00',
    endTime:      '09:30',
    customerName: 'Alice Smith',
  }

  it('parses a valid staff booking', () => {
    const result = StaffBookingSchema.parse(validBooking)
    expect(result.bookingRef).toBe('bk-abc12345')
    expect(result.customerName).toBe('Alice Smith')
  })

  it('parses date, startTime and endTime', () => {
    const result = StaffBookingSchema.parse(validBooking)
    expect(result.date).toBe('2026-06-10')
    expect(result.startTime).toBe('09:00')
    expect(result.endTime).toBe('09:30')
  })

  it('rejects missing bookingRef', () => {
    const { bookingRef: _b, ...without } = validBooking
    expect(() => StaffBookingSchema.parse(without)).toThrow(/invalid|too_small|ZodError/i)
  })

  it('rejects empty bookingRef', () => {
    expect(() => StaffBookingSchema.parse({ ...validBooking, bookingRef: '' })).toThrow(/invalid|too_small|ZodError/i)
  })

  it('rejects missing customerName', () => {
    const { customerName: _c, ...without } = validBooking
    expect(() => StaffBookingSchema.parse(without)).toThrow(/invalid|too_small|ZodError/i)
  })

  it('rejects a payload with bookingId instead of bookingRef', () => {
    const wrongShape = { bookingId: 'bk-abc12345', date: '2026-06-10', startTime: '09:00', endTime: '09:30', customerName: 'Alice' }
    expect(() => StaffBookingSchema.parse(wrongShape)).toThrow(/invalid|too_small|ZodError/i)
  })
})
