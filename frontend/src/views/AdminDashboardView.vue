<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { fetchLocations } from '@/api/booking'
import AppHeader from '@/components/AppHeader.vue'
import LocationPanel from '@/components/LocationPanel.vue'
import { useAuthStore } from '@/stores/authStore'
import type { Location } from '@/api/schemas'

type Status = 'loading' | 'success' | 'error'

const router = useRouter()
const auth = useAuthStore()

const status = ref<Status>('loading')
const locations = ref<Location[]>([])
const apiError = ref('')

onMounted(async () => {
  try {
    locations.value = await fetchLocations()
    status.value = 'success'
  } catch (err) {
    apiError.value = err instanceof Error ? err.message : 'Failed to load locations'
    status.value = 'error'
  }
})

function onAddLocation(): void {
  router.push({ name: 'add-location' })
}
</script>

<template>
  <div class="admin-view">
    <AppHeader :employee-name="auth.employeeName" :role="auth.role" />

    <main class="main-content">
      <div class="page-header">
        <h1 class="page-title">Locations</h1>
        <button
          class="btn btn-primary"
          data-testid="add-location-btn"
          @click="onAddLocation"
        >
          + Add Location
        </button>
      </div>

      <div v-if="status === 'loading'" data-testid="loading-state" class="state-msg">
        Loading locations…
      </div>

      <span v-else-if="status === 'error'" data-testid="api-error" class="api-error">
        {{ apiError }}
      </span>

      <div
        v-else-if="status === 'success' && locations.length === 0"
        data-testid="empty-state"
        class="empty-state"
      >
        <svg class="empty-icon" xmlns="http://www.w3.org/2000/svg" width="40" height="40" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
          <path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z"/>
          <circle cx="12" cy="10" r="3"/>
        </svg>
        <p class="empty-heading">No locations yet</p>
        <p class="empty-subtext">Add your first location to get started.</p>
        <button class="btn btn-primary" @click="onAddLocation">+ Add Location</button>
      </div>

      <div v-else-if="status === 'success'" class="panels-grid">
        <LocationPanel
          v-for="location in locations"
          :key="location.id"
          :location="location"
        />
      </div>
    </main>
  </div>
</template>

<style scoped>
.admin-view {
  min-height: 100vh;
  background: var(--color-surface, #f8fafc);
}

.main-content {
  max-width: 56rem;
  margin: 0 auto;
  padding: 2rem;
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
}

.page-title {
  font-size: 1.5rem;
  font-weight: 700;
  color: var(--color-text, #0f172a);
  margin: 0;
}

.btn {
  padding: 0.5rem 1rem;
  font-size: 0.875rem;
  font-weight: 600;
  border: none;
  border-radius: 0.375rem;
  cursor: pointer;
  transition: background 0.15s ease;
  white-space: nowrap;
}

.btn-primary {
  background: #0d9488;
  color: #fff;
}

.btn-primary:hover {
  background: #0f766e;
}

.state-msg {
  color: var(--color-text-muted, #64748b);
  font-size: 0.95rem;
  padding: 1rem 0;
}

.api-error {
  font-size: 0.875rem;
  color: #dc2626;
}

.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.5rem;
  padding: 4rem 1rem;
  text-align: center;
  color: var(--color-text-muted, #64748b);
}

.empty-icon {
  color: #94a3b8;
  margin-bottom: 0.25rem;
}

.empty-heading {
  margin: 0;
  font-size: 1rem;
  font-weight: 600;
  color: var(--color-text, #1e293b);
}

.empty-subtext {
  margin: 0 0 1rem;
  font-size: 0.875rem;
  max-width: 28rem;
}

.panels-grid {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}
</style>
