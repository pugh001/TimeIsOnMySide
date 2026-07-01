import {
  CreateUserRequestSchema,
  CreateUserResponseSchema,
  StaffBookingsEnvelopeSchema,
} from './schemas'
import type { CreateUserRequest, CreateUserResponse, StaffBookingResponse } from './schemas'

const BASE = import.meta.env.VITE_API_BASE_URL ?? ''

export async function createUser(
  request: CreateUserRequest,
  adminToken: string,
  adminUserId: string,
): Promise<CreateUserResponse> {
  const body = CreateUserRequestSchema.parse(request)

  const res = await fetch(`${BASE}/api/users`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'X-Admin-Token': adminToken,
      'X-Admin-UserId': adminUserId,
    },
    body: JSON.stringify(body),
  })
  if (res.status === 401) throw new Error('Unauthorized — admin access required')
  if (res.status === 409) {
    const data = await res.json()
    throw new Error((data as { error?: string }).error ?? 'Username already taken')
  }
  if (res.status === 422) {
    const data = await res.json()
    throw new Error((data as { error?: string }).error ?? 'Validation failed')
  }
  if (!res.ok) throw new Error(`createUser failed: ${res.status}`)
  return CreateUserResponseSchema.parse(await res.json())
}

export async function fetchUserBookings(
  userId: string,
  adminToken: string,
  adminUserId: string,
): Promise<StaffBookingResponse[]> {
  const res = await fetch(`${BASE}/api/users/${userId}/bookings`, {
    headers: {
      'X-Admin-Token': adminToken,
      'X-Admin-UserId': adminUserId,
    },
  })
  if (res.status === 401) throw new Error('Unauthorized — admin access required')
  if (!res.ok) throw new Error(`fetchUserBookings failed: ${res.status}`)
  return StaffBookingsEnvelopeSchema.parse(await res.json()).bookings
}
