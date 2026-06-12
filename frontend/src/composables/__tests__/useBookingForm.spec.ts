import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useBookingForm } from '../useBookingForm'
import { useBookingStore } from '@/stores/bookingStore'
import * as bookingApi from '@/api/booking'

vi.mock('@/api/booking')

describe('useBookingForm', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.mocked(bookingApi.fetchSlots).mockResolvedValue([])
    vi.mocked(bookingApi.createBooking).mockResolvedValue({
      bookingId: 'bk-001',
      slotId: 'slot-001',
      startTime: '09:00',
      name: 'Jane',
      date: '2026-05-26',
    })
  })

  it('initialises with empty fields', () => {
    const { name, email, phone, notes } = useBookingForm()
    expect(name.value).toBe('')
    expect(email.value).toBe('')
    expect(phone.value).toBe('')
    expect(notes.value).toBe('')
  })

  it('initialises with no errors', () => {
    const { errors } = useBookingForm()
    expect(errors.value).toEqual({})
  })

  it('initialises with isSubmitting false', () => {
    const { isSubmitting } = useBookingForm()
    expect(isSubmitting.value).toBe(false)
  })

  it('sets name error when name is empty on submit', async () => {
    const { email, phone, errors, submit } = useBookingForm()
    email.value = 'jane@example.com'
    phone.value = '123'
    await submit()
    expect(errors.value.name).toBeTruthy()
  })

  it('sets email error when email is invalid on submit', async () => {
    const { name, email, phone, errors, submit } = useBookingForm()
    name.value = 'Jane'
    email.value = 'not-an-email'
    phone.value = '123'
    await submit()
    expect(errors.value.email).toBeTruthy()
  })

  it('sets phone error when phone is empty on submit', async () => {
    const { name, email, errors, submit } = useBookingForm()
    name.value = 'Jane'
    email.value = 'jane@example.com'
    await submit()
    expect(errors.value.phone).toBeTruthy()
  })

  it('does not call store.submitBooking when validation fails', async () => {
    const { submit } = useBookingForm()
    const store = useBookingStore()
    const spy = vi.spyOn(store, 'submitBooking')
    await submit()
    expect(spy).not.toHaveBeenCalled()
  })

  it('calls store.submitBooking with form data on valid submit', async () => {
    const store = useBookingStore()
    store.selectedSlot = { id: 'slot-001', date: '2026-05-26', startTime: '09:00', endTime: '09:30', status: 'available', locationId: 'a' }
    const spy = vi.spyOn(store, 'submitBooking').mockResolvedValue()
    const { name, email, phone, submit } = useBookingForm()
    name.value = 'Jane Doe'
    email.value = 'jane@example.com'
    phone.value = '0831231234'
    await submit()
    expect(spy).toHaveBeenCalledWith({
      slotId: 'slot-001',
      name: 'Jane Doe',
      email: 'jane@example.com',
      phone: '0831231234',
      notes: undefined,
    })
  })

  it('resets form fields after successful submission', async () => {
    const store = useBookingStore()
    store.selectedSlot = { id: 'slot-001', date: '2026-05-26', startTime: '09:00', endTime: '09:30', status: 'available', locationId: 'a' }
    vi.spyOn(store, 'submitBooking').mockImplementation(async () => { store.bookingStatus = 'success' })
    const { name, email, phone, submit } = useBookingForm()
    name.value = 'Jane Doe'
    email.value = 'jane@example.com'
    phone.value = '0831231234'
    await submit()
    expect(name.value).toBe('')
    expect(email.value).toBe('')
    expect(phone.value).toBe('')
  })

  it('reset() clears all form fields and errors', async () => {
    const { name, email, phone, notes, errors, reset } = useBookingForm()
    name.value = 'Jane'
    email.value = 'jane@example.com'
    phone.value = '0831231234'
    notes.value = 'some notes'
    errors.value = { name: 'required' }
    reset()
    expect(name.value).toBe('')
    expect(email.value).toBe('')
    expect(phone.value).toBe('')
    expect(notes.value).toBe('')
    expect(errors.value).toEqual({})
  })

  it('preserves form fields after failed submission', async () => {
    const store = useBookingStore()
    store.selectedSlot = { id: 'slot-001', date: '2026-05-26', startTime: '09:00', endTime: '09:30', status: 'available', locationId: 'a' }
    vi.spyOn(store, 'submitBooking').mockImplementation(async () => { store.bookingStatus = 'error' })
    const { name, email, phone, submit } = useBookingForm()
    name.value = 'Jane Doe'
    email.value = 'jane@example.com'
    phone.value = '0831231234'
    await submit()
    expect(name.value).toBe('Jane Doe')
    expect(email.value).toBe('jane@example.com')
    expect(phone.value).toBe('0831231234')
  })
})
