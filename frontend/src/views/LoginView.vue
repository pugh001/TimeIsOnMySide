<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'

const router = useRouter()
const auth = useAuthStore()

const username = ref('')
const password = ref('')
const showPassword = ref(false)
const usernameError = ref('')
const passwordError = ref('')
const apiError = ref('')

function validateUsername(): boolean {
  if (!username.value) {
    usernameError.value = 'Username is required'
    return false
  }
  usernameError.value = ''
  return true
}

function validatePassword(): boolean {
  const p = password.value
  if (!p) {
    passwordError.value = 'Password is required'
    return false
  }
  if (!/[a-z]/.test(p)) {
    passwordError.value = 'Password must contain a lowercase letter'
    return false
  }
  if (!/[A-Z]/.test(p)) {
    passwordError.value = 'Password must contain an uppercase letter'
    return false
  }
  if (!/[0-9]/.test(p)) {
    passwordError.value = 'Password must contain a number'
    return false
  }
  if (!/[^a-zA-Z0-9]/.test(p)) {
    passwordError.value = 'Password must contain a special character'
    return false
  }
  passwordError.value = ''
  return true
}

async function onSubmit(): Promise<void> {
  apiError.value = ''
  const validUser = validateUsername()
  const validPass = validatePassword()
  if (!validUser || !validPass) return

  try {
    await auth.login(username.value, password.value)
    if (!auth.role) { router.push('/'); return }
    if (auth.role === 'admin') { router.push('/admin'); return }
    if (auth.role === 'staff') { router.push('/dashboard'); return }
    router.push('/')
  } catch (err) {
    apiError.value = err instanceof Error ? err.message : 'Login failed'
  }
}
</script>

<template>
  <div class="login-view">
    <div class="login-card">
      <h1 class="login-title">Sign in to your account</h1>

      <form data-testid="login-form" class="login-form" @submit.prevent="onSubmit">
        <div class="field">
          <label for="username" class="field-label">Username</label>
          <input
            id="username"
            v-model="username"
            data-testid="username-input"
            type="text"
            class="field-input"
            autocomplete="username"
          />
          <span v-if="usernameError" data-testid="username-error" class="field-error">
            {{ usernameError }}
          </span>
        </div>

        <div class="field">
          <label for="password" class="field-label">Password</label>
          <div class="password-wrap">
            <input
              id="password"
              v-model="password"
              data-testid="password-input"
              :type="showPassword ? 'text' : 'password'"
              class="field-input password-input"
              autocomplete="current-password"
            />
            <button
              type="button"
              data-testid="password-toggle"
              class="password-toggle"
              :aria-label="showPassword ? 'Hide password' : 'Show password'"
              @click="showPassword = !showPassword"
            >
              <svg v-if="showPassword" xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24"/>
                <line x1="1" y1="1" x2="23" y2="23"/>
              </svg>
              <svg v-else xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/>
                <circle cx="12" cy="12" r="3"/>
              </svg>
            </button>
          </div>
          <span v-if="passwordError" data-testid="password-error" class="field-error">
            {{ passwordError }}
          </span>
        </div>

        <span v-if="apiError" data-testid="api-error" class="api-error">{{ apiError }}</span>

        <button type="submit" data-testid="submit-btn" class="submit-btn">Sign In</button>
      </form>
    </div>
  </div>
</template>

<style scoped>
.login-view {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #f8fafc;
  padding: 2rem;
}

.login-card {
  width: 100%;
  max-width: 28rem;
  background: #ffffff;
  border-radius: 0.5rem;
  padding: 2.5rem 2rem;
  box-shadow: 0 1px 3px rgba(0,0,0,0.1), 0 1px 2px rgba(0,0,0,0.06);
}

.login-title {
  font-size: 1.75rem;
  font-weight: 700;
  color: #0f172a;
  margin: 0 0 2rem;
  line-height: 1.2;
}

.login-form {
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
}

.field {
  display: flex;
  flex-direction: column;
  gap: 0.375rem;
}

.field-label {
  font-size: 0.875rem;
  font-weight: 500;
  color: #0f172a;
}

.field-input {
  width: 100%;
  padding: 0.625rem 0.75rem;
  border: 1px solid #cbd5e1;
  border-radius: 0.375rem;
  font-size: 1rem;
  color: #0f172a;
  background: #f8fafc;
  outline: none;
  box-sizing: border-box;
  transition: border-color 0.15s ease, box-shadow 0.15s ease;
}

.field-input:focus {
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59,130,246,0.15);
  background: #fff;
}

.password-wrap {
  position: relative;
  display: flex;
  align-items: center;
}

.password-input {
  padding-right: 2.75rem;
}

.password-toggle {
  position: absolute;
  right: 0.625rem;
  background: none;
  border: none;
  cursor: pointer;
  padding: 0.25rem;
  color: #64748b;
  display: flex;
  align-items: center;
}

.password-toggle:hover {
  color: #0f172a;
}

.field-error {
  font-size: 0.8125rem;
  color: #dc2626;
}

.api-error {
  font-size: 0.875rem;
  color: #dc2626;
  text-align: center;
}

.submit-btn {
  width: 100%;
  padding: 0.75rem;
  background: #3b82f6;
  color: #ffffff;
  font-size: 1rem;
  font-weight: 600;
  border: none;
  border-radius: 0.375rem;
  cursor: pointer;
  transition: background 0.15s ease;
  margin-top: 0.25rem;
}

.submit-btn:hover {
  background: #2563eb;
}

.submit-btn:active {
  background: #1d4ed8;
}
</style>
