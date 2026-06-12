import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { loginUser } from '@/api/auth'

const SESSION_KEY = 'auth'

type AuthSnapshot = {
  employeeName: string
  role: 'admin' | 'staff'
  adminToken: string | null
  adminUserId: string | null
  staffToken: string | null
  staffUserId: string | null
  tokenDate: string | null
}

function todayUtc(): string {
  return new Date().toISOString().slice(0, 10)
}

function loadSnapshot(): AuthSnapshot | null {
  try {
    const raw = sessionStorage.getItem(SESSION_KEY)
    if (!raw) return null
    const parsed = JSON.parse(raw) as AuthSnapshot
    if (!parsed.employeeName || !parsed.role) return null
    return parsed
  } catch {
    return null
  }
}

export const useAuthStore = defineStore('auth', () => {
  const snapshot = loadSnapshot()
  const snapshotFresh = snapshot?.tokenDate === todayUtc()

  const employeeName = ref<string | null>(snapshotFresh ? (snapshot?.employeeName ?? null) : null)
  const role = ref<'admin' | 'staff' | null>(snapshotFresh ? (snapshot?.role ?? null) : null)
  const adminToken = ref<string | null>(snapshotFresh ? (snapshot?.adminToken ?? null) : null)
  const adminUserId = ref<string | null>(snapshotFresh ? (snapshot?.adminUserId ?? null) : null)
  const staffToken = ref<string | null>(snapshotFresh ? (snapshot?.staffToken ?? null) : null)
  const staffUserId = ref<string | null>(snapshotFresh ? (snapshot?.staffUserId ?? null) : null)
  const tokenDate = ref<string | null>(snapshotFresh ? (snapshot?.tokenDate ?? null) : null)
  const isLoggedIn = computed(() => !!employeeName.value)
  const isTokenFresh = computed(() => tokenDate.value === todayUtc())

  if (snapshot && !snapshotFresh) {
    sessionStorage.removeItem(SESSION_KEY)
  }

  async function login(username: string, password: string): Promise<void> {
    const data = await loginUser(username, password)
    const today = todayUtc()
    employeeName.value = data.employeeName
    role.value = data.role
    adminToken.value = data.adminToken ?? null
    adminUserId.value = data.adminUserId ?? null
    staffToken.value = data.staffToken ?? null
    staffUserId.value = data.staffUserId ?? null
    tokenDate.value = today
    sessionStorage.setItem(
      SESSION_KEY,
      JSON.stringify({
        employeeName: data.employeeName,
        role: data.role,
        adminToken: data.adminToken ?? null,
        adminUserId: data.adminUserId ?? null,
        staffToken: data.staffToken ?? null,
        staffUserId: data.staffUserId ?? null,
        tokenDate: today,
      }),
    )
  }

  function logout(): void {
    employeeName.value = null
    role.value = null
    adminToken.value = null
    adminUserId.value = null
    staffToken.value = null
    staffUserId.value = null
    tokenDate.value = null
    sessionStorage.removeItem(SESSION_KEY)
  }

  return { employeeName, role, adminToken, adminUserId, staffToken, staffUserId, tokenDate, isLoggedIn, isTokenFresh, login, logout }
})
