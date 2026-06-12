<script setup lang="ts">
import { ref, watch, onUnmounted } from 'vue'
import { RouterLink, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'

const props = defineProps<{
  employeeName?: string | null
  role?: 'admin' | 'staff' | null
}>()

const router = useRouter()
const auth = useAuthStore()
const menuOpen = ref(false)
const menuRef = ref<HTMLElement | null>(null)

function onDocClick(e: MouseEvent): void {
  if (menuRef.value && !menuRef.value.contains(e.target as Node)) {
    menuOpen.value = false
  }
}

watch(menuOpen, (open) => {
  if (open) { document.addEventListener('click', onDocClick); return }
  document.removeEventListener('click', onDocClick)
})

onUnmounted(() => document.removeEventListener('click', onDocClick))

function onLogout(): void {
  menuOpen.value = false
  auth.logout()
  router.push('/login')
}
</script>

<template>
  <header class="app-header">
    <span class="brand">Time Is On My Side - Booking System</span>

    <div class="header-right">
      <RouterLink
        v-if="!props.employeeName"
        to="/login"
        class="login-btn"
        data-testid="login-link"
      >
        Login
      </RouterLink>

      <div v-else ref="menuRef" class="user-menu">
        <button
          class="user-menu-toggle"
          data-testid="user-menu-toggle"
          @click="menuOpen = !menuOpen"
        >
          {{ props.employeeName }}
          <svg
            class="chevron"
            :class="{ open: menuOpen }"
            xmlns="http://www.w3.org/2000/svg"
            width="14"
            height="14"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            stroke-width="2.5"
            stroke-linecap="round"
            stroke-linejoin="round"
          >
            <polyline points="6 9 12 15 18 9" />
          </svg>
        </button>

        <div v-if="menuOpen" class="dropdown" data-testid="user-dropdown">
          <button class="dropdown-item" data-testid="logout-btn" @click="onLogout">
            Logout
          </button>
        </div>
      </div>
    </div>
  </header>
</template>

<style scoped>
.app-header {
  position: sticky;
  top: 0;
  z-index: 50;
  display: flex;
  align-items: center;
  height: 3rem;
  padding: 0 1rem;
  background: #1e3a5f;
  gap: 1rem;
}

.brand {
  font-size: 1rem;
  font-weight: 700;
  color: #ffffff;
  letter-spacing: -0.01em;
  white-space: nowrap;
}

.header-right {
  margin-left: auto;
}

.login-btn {
  display: inline-block;
  padding: 0.35rem 0.875rem;
  background: #1e3a5f;
  color: #ffffff;
  text-decoration: none;
  font-size: 0.875rem;
  font-weight: 500;
  border-radius: 0.25rem;
  transition: background 0.15s ease;
}

.login-btn:hover {
  background: #dc2626;
}

.user-menu {
  position: relative;
}

.user-menu-toggle {
  display: flex;
  align-items: center;
  gap: 0.375rem;
  background: transparent;
  border: none;
  color: #ffffff;
  font-size: 0.875rem;
  font-weight: 500;
  cursor: pointer;
  padding: 0.35rem 0.5rem;
  border-radius: 0.25rem;
  transition: background 0.15s ease;
}

.user-menu-toggle:hover {
  background: rgba(255, 255, 255, 0.1);
}

.chevron {
  transition: transform 0.15s ease;
}

.chevron.open {
  transform: rotate(180deg);
}

.dropdown {
  position: absolute;
  right: 0;
  top: calc(100% + 0.375rem);
  min-width: 9rem;
  background: #ffffff;
  border: 1px solid #e2e8f0;
  border-radius: 0.375rem;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.12);
  overflow: hidden;
  z-index: 100;
}

.dropdown-item {
  display: block;
  width: 100%;
  padding: 0.625rem 1rem;
  background: transparent;
  border: none;
  text-align: left;
  font-size: 0.875rem;
  color: #0f172a;
  cursor: pointer;
  transition: background 0.1s ease;
}

.dropdown-item:hover {
  background: #f1f5f9;
}
</style>
