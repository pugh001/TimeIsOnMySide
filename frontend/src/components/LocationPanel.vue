<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'
import { getLocationUsers } from '@/api/locations'
import type { Location, UserSummary } from '@/api/schemas'

const props = defineProps<{ location: Location }>()

type UsersStatus = 'idle' | 'loading' | 'success' | 'error'

const router = useRouter()
const auth = useAuthStore()

const showUsers = ref(false)
const usersStatus = ref<UsersStatus>('idle')
const users = ref<UserSummary[]>([])
const usersError = ref('')

async function toggleUsers(): Promise<void> {
  if (showUsers.value) {
    showUsers.value = false
    return
  }
  showUsers.value = true
  if (usersStatus.value === 'success') return
  usersStatus.value = 'loading'
  try {
    users.value = await getLocationUsers(props.location.id, auth.adminToken ?? '', auth.adminUserId ?? '')
    usersStatus.value = 'success'
  } catch (err) {
    usersError.value = err instanceof Error ? err.message : 'Failed to load users'
    usersStatus.value = 'error'
  }
}

function onAddUser(): void {
  router.push({ name: 'add-user', query: { locationId: props.location.id } })
}
</script>

<template>
  <div class="location-panel">
    <div class="panel-header">
      <div class="panel-info">
        <h3 class="panel-name">{{ location.name }}</h3>
        <p v-if="location.address" class="panel-address">{{ location.address }}</p>
      </div>
      <div class="panel-actions">
        <button
          class="btn btn-secondary"
          data-testid="show-users-btn"
          :aria-expanded="showUsers"
          @click="toggleUsers"
        >
          {{ showUsers ? 'Hide Users' : 'Show Users' }}
        </button>
        <button
          class="btn btn-primary"
          data-testid="add-user-btn"
          @click="onAddUser"
        >
          + Add User
        </button>
      </div>
    </div>

    <div v-if="showUsers" class="users-section">
      <div v-if="usersStatus === 'loading'" data-testid="users-loading" class="users-state">
        Loading staff…
      </div>

      <div v-else-if="usersStatus === 'error'" data-testid="users-error" class="users-state users-state--error">
        {{ usersError }}
      </div>

      <div v-else-if="usersStatus === 'success' && users.length === 0" data-testid="users-empty" class="users-state">
        No staff members assigned yet.
      </div>

      <ul v-else-if="usersStatus === 'success'" data-testid="user-list" class="user-list" role="list">
        <li v-for="user in users" :key="user.id" class="user-item">
          <span class="user-name">{{ user.fullName }}</span>
          <span class="user-schedule">
            <span
              v-for="wt in user.workingTimes"
              :key="wt.day"
              class="user-day-shift"
            >{{ wt.day.slice(0, 3) }} {{ wt.shiftStart }}–{{ wt.shiftEnd }}</span>
          </span>
        </li>
      </ul>
    </div>
  </div>
</template>

<style scoped>
.location-panel {
  background: #ffffff;
  border: 1px solid #e2e8f0;
  border-radius: 0.5rem;
  overflow: hidden;
}

.panel-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
  padding: 1.25rem;
}

.panel-info {
  flex: 1;
  min-width: 0;
}

.panel-name {
  font-size: 1rem;
  font-weight: 600;
  color: #0f172a;
  margin: 0 0 0.25rem;
}

.panel-address {
  font-size: 0.8125rem;
  color: #64748b;
  margin: 0;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.panel-actions {
  display: flex;
  gap: 0.5rem;
  flex-shrink: 0;
}

.btn {
  padding: 0.4rem 0.875rem;
  font-size: 0.8125rem;
  font-weight: 600;
  border: none;
  border-radius: 0.375rem;
  cursor: pointer;
  white-space: nowrap;
  transition: background 0.15s ease;
}

.btn-primary {
  background: #0d9488;
  color: #fff;
}

.btn-primary:hover {
  background: #0f766e;
}

.btn-secondary {
  background: #f1f5f9;
  color: #0f172a;
}

.btn-secondary:hover {
  background: #e2e8f0;
}

.users-section {
  border-top: 1px solid #f1f5f9;
  padding: 1rem 1.25rem;
  background: #f8fafc;
}

.users-state {
  font-size: 0.875rem;
  color: #64748b;
  padding: 0.5rem 0;
}

.users-state--error {
  color: #dc2626;
}

.user-list {
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.user-item {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  font-size: 0.875rem;
  color: #0f172a;
  padding: 0.5rem 0;
  border-bottom: 1px solid #f1f5f9;
}

.user-item:last-child {
  border-bottom: none;
}

.user-name {
  font-weight: 500;
}

.user-schedule {
  display: flex;
  flex-wrap: wrap;
  gap: 0.25rem 0.75rem;
}

.user-day-shift {
  font-size: 0.8125rem;
  color: #64748b;
  text-transform: capitalize;
}
</style>
