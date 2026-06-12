import { z } from 'zod'

const TIME_MINUTES_MIN = 0
const TIME_MINUTES_MAX = 23 * 60 + 59
const OPENING_SPAN_MINUTES_MIN = 60

export const DAY_KEYS = ['monday', 'tuesday', 'wednesday', 'thursday', 'friday', 'saturday', 'sunday'] as const
export type DayKey = typeof DAY_KEYS[number]

function toMinutes(hhmm: string): number {
  const parts = hhmm.split(':').map(Number)
  return (parts[0] ?? 0) * 60 + (parts[1] ?? 0)
}

export const LocationSchema = z.object({
  id: z.string().min(1),
  slug: z.string().min(1),
  name: z.string().min(1),
  address: z.string().optional(),
  openingHours: z.record(
    z.string(),
    z.object({
      openTime: z.string().regex(/^\d{2}:\d{2}$/),
      closeTime: z.string().regex(/^\d{2}:\d{2}$/),
    }).nullable(),
  ).optional(),
})

export type Location = z.infer<typeof LocationSchema>

export const WorkingTimeSchema = z.object({
  day: z.enum(DAY_KEYS),
  shiftStart: z.string().regex(/^\d{2}:\d{2}$/, 'Use HH:MM format'),
  shiftEnd: z.string().regex(/^\d{2}:\d{2}$/, 'Use HH:MM format'),
})

export type WorkingTime = z.infer<typeof WorkingTimeSchema>

export const UserSummarySchema = z.object({
  id: z.string().min(1),
  fullName: z.string().min(1),
  workingTimes: z.array(WorkingTimeSchema),
})

export type UserSummary = z.infer<typeof UserSummarySchema>

export const SlotSchema = z.object({
  id: z.string().min(1),
  date: z.string().min(1),
  startTime: z.string().min(1),
  endTime: z.string().min(1),
  status: z.union([z.literal('available'), z.literal('unavailable')]),
  locationId: z.string().min(1),
})

export const BookingRequestSchema = z.object({
  slotId: z.string().min(1),
  name: z.string().min(1),
  email: z.string().email(),
  phone: z.string().regex(/^0\d{9}$/, 'Enter a valid phone number (e.g. 0831231234)'),
  notes: z.string().optional(),
})

export const BookingResponseSchema = z.object({
  bookingId: z.string().min(1),
  slotId: z.string().min(1),
  startTime: z.string().min(1),
  name: z.string().min(1),
  date: z.string().min(1),
})

export type Slot = z.infer<typeof SlotSchema>
export type BookingRequest = z.infer<typeof BookingRequestSchema>
export type BookingResponse = z.infer<typeof BookingResponseSchema>

export const DayHoursSchema = z
  .object({
    openTime: z.string().regex(/^\d{2}:\d{2}$/, 'Use HH:MM format'),
    closeTime: z.string().regex(/^\d{2}:\d{2}$/, 'Use HH:MM format'),
  })
  .refine(
    ({ openTime, closeTime }) => {
      const open = toMinutes(openTime)
      const close = toMinutes(closeTime)
      return (
        open >= TIME_MINUTES_MIN &&
        open <= TIME_MINUTES_MAX &&
        close >= TIME_MINUTES_MIN &&
        close <= TIME_MINUTES_MAX &&
        close > open
      )
    },
    { message: 'Closing time must be after opening time', path: ['closeTime'] },
  )
  .refine(
    ({ openTime, closeTime }) =>
      toMinutes(closeTime) - toMinutes(openTime) >= OPENING_SPAN_MINUTES_MIN,
    { message: 'Opening hours must span at least 1 hour', path: ['closeTime'] },
  )

const OpeningHoursSchema = z.object({
  monday: DayHoursSchema.nullable().optional(),
  tuesday: DayHoursSchema.nullable().optional(),
  wednesday: DayHoursSchema.nullable().optional(),
  thursday: DayHoursSchema.nullable().optional(),
  friday: DayHoursSchema.nullable().optional(),
  saturday: DayHoursSchema.nullable().optional(),
  sunday: DayHoursSchema.nullable().optional(),
})

export const CreateLocationRequestSchema = z.object({
  name: z.string().min(2, 'Name must be at least 2 characters'),
  address: z.string().min(1, 'Address is required'),
  openingHours: OpeningHoursSchema.optional(),
})

export const CreateLocationResponseSchema = z.object({
  slug: z.string().min(1),
  id: z.string().min(1),
})

export type CreateLocationRequest = z.infer<typeof CreateLocationRequestSchema>
export type CreateLocationResponse = z.infer<typeof CreateLocationResponseSchema>

const SHIFT_SPAN_MINUTES_MAX = 480

function shiftSpanMinutes(start: string, end: string): number {
  const s = toMinutes(start)
  const e = toMinutes(end)
  return e >= s ? e - s : 1440 - s + e
}

export const CreateUserRequestSchema = z.object({
  firstName: z.string().min(1, 'First name is required').regex(/^[a-zA-Z]+$/, 'First name must contain letters only'),
  lastName: z.string().min(1, 'Last name is required').regex(/^[a-zA-Z]+$/, 'Last name must contain letters only'),
  password: z.string().min(8, 'Password must be at least 8 characters'),
  locationId: z.string().min(1, 'A location is required'),
  workingTimes: z.array(
    WorkingTimeSchema.refine(
      ({ shiftStart, shiftEnd }) => shiftSpanMinutes(shiftStart, shiftEnd) <= SHIFT_SPAN_MINUTES_MAX,
      { message: 'Shift cannot exceed 8 hours', path: ['shiftEnd'] },
    ),
  ).min(1, 'Select at least one working day'),
})

export const CreateUserResponseSchema = z.object({
  userId: z.string().min(1),
  username: z.string().min(1),
})

export type CreateUserRequest = z.infer<typeof CreateUserRequestSchema>
export type CreateUserResponse = z.infer<typeof CreateUserResponseSchema>

export const StaffBookingSchema = z.object({
  bookingRef: z.string().min(1),
  date: z.string().min(1),
  startTime: z.string().min(1),
  endTime: z.string().min(1),
  customerName: z.string().min(1),
})

export type StaffBookingResponse = z.infer<typeof StaffBookingSchema>
