import { z } from 'zod'
import {
  CreateLocationRequestSchema,
  CreateLocationResponseSchema,
  LocationSchema,
  UserSummarySchema,
} from './schemas'
import type { CreateLocationRequest, CreateLocationResponse, Location, UserSummary } from './schemas'

const BASE = import.meta.env.VITE_API_BASE_URL ?? ''

export async function getLocation(
  id: string,
  adminToken: string,
  adminUserId: string,
): Promise<Location> {
  const res = await fetch(`${BASE}/api/locations/${id}`, {
    headers: {
      'X-Admin-Token': adminToken,
      'X-Admin-UserId': adminUserId,
    },
  })
  if (res.status === 401) throw new Error('Unauthorized — admin access required')
  if (res.status === 404) throw new Error('Location not found')
  if (!res.ok) throw new Error(`getLocation failed: ${res.status}`)
  return LocationSchema.parse(await res.json())
}

export async function createLocation(
  request: CreateLocationRequest,
  adminToken: string,
  adminUserId: string,
): Promise<CreateLocationResponse> {
  const res = await fetch(`${BASE}/api/locations`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'X-Admin-Token': adminToken,
      'X-Admin-UserId': adminUserId,
    },
    body: JSON.stringify(CreateLocationRequestSchema.parse(request)),
  })
  if (res.status === 401) throw new Error('Unauthorized — admin access required')
  if (res.status === 422) {
    const data = await res.json()
    throw new Error((data as { error?: string }).error ?? 'Validation failed')
  }
  if (!res.ok) throw new Error(`createLocation failed: ${res.status}`)
  return CreateLocationResponseSchema.parse(await res.json())
}

export async function getLocationUsers(
  locationId: string,
  adminToken: string,
  adminUserId: string,
): Promise<UserSummary[]> {
  const res = await fetch(`${BASE}/api/locations/${locationId}/users`, {
    headers: {
      'X-Admin-Token': adminToken,
      'X-Admin-UserId': adminUserId,
    },
  })
  if (res.status === 401) throw new Error('Unauthorized — admin access required')
  if (res.status === 404) throw new Error('Location not found')
  if (!res.ok) throw new Error(`getLocationUsers failed: ${res.status}`)
  return z.object({ users: z.array(UserSummarySchema) }).parse(await res.json()).users
}
