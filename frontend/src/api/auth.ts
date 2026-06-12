import { z } from 'zod'

const BASE = import.meta.env.VITE_API_BASE_URL ?? ''

const LoginResponseSchema = z.object({
  employeeName: z.string().min(1),
  role: z.union([z.literal('admin'), z.literal('staff')]),
  adminToken: z.string().nullable().optional(),
  adminUserId: z.string().nullable().optional(),
  staffToken: z.string().nullable().optional(),
  staffUserId: z.string().nullable().optional(),
})

export type LoginResponse = z.infer<typeof LoginResponseSchema>

export async function loginUser(username: string, password: string): Promise<LoginResponse> {
  const res = await fetch(`${BASE}/api/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ username, password }),
  })
  if (!res.ok) {
    const data = await res.json()
    throw new Error((data as { error?: string }).error ?? 'Login failed')
  }
  return LoginResponseSchema.parse(await res.json())
}
