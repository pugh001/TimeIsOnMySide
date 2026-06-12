<script setup lang="ts">
import { ref, computed } from 'vue'
import type { Location } from '@/api/schemas'

type LocationStatus = 'idle' | 'loading' | 'error'

const props = defineProps<{
  locations: Location[]
  selectedLocation: Location | null
  status: LocationStatus
}>()

const emit = defineEmits<{ 'select-location': [loc: Location] }>()

const search = ref('')

const filteredLocations = computed(() => {
  const q = search.value.trim().toLowerCase()
  if (!q) return props.locations
  return props.locations.filter(loc => loc.name.toLowerCase().includes(q))
})
</script>

<template>
  <div class="location-picker">
    <div v-if="status === 'loading'" data-testid="loading" class="loading">
      Loading locations…
    </div>
    <div v-else-if="status === 'error'" data-testid="locations-error" class="empty-state">
      <svg class="empty-icon" xmlns="http://www.w3.org/2000/svg" width="40" height="40" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
        <circle cx="12" cy="12" r="10" />
        <line x1="12" y1="8" x2="12" y2="12" />
        <line x1="12" y1="16" x2="12.01" y2="16" />
      </svg>
      <h3 class="empty-title">Could not load locations</h3>
      <p class="empty-body">There was a problem loading available locations. Please try again later.</p>
    </div>
    <div v-else-if="locations.length === 0" data-testid="unavailable" class="empty-state">
      <svg class="empty-icon" xmlns="http://www.w3.org/2000/svg" width="40" height="40" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
        <rect x="3" y="4" width="18" height="18" rx="2" ry="2" />
        <line x1="16" y1="2" x2="16" y2="6" />
        <line x1="8" y1="2" x2="8" y2="6" />
        <line x1="3" y1="10" x2="21" y2="10" />
      </svg>
      <h3 class="empty-title">No availability right now</h3>
      <p class="empty-body">There are no locations available for booking at this time. Check back soon.</p>
    </div>
    <div v-else class="list-container">
      <div class="search-wrap">
        <svg class="search-icon" xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
          <circle cx="11" cy="11" r="8" />
          <line x1="21" y1="21" x2="16.65" y2="16.65" />
        </svg>
        <input
          v-model="search"
          data-testid="location-search"
          type="search"
          class="search-input"
          placeholder="Search locations…"
          aria-label="Search locations"
        />
      </div>
      <div data-testid="location-list" class="list" role="listbox" aria-label="Locations">
        <p v-if="filteredLocations.length === 0" data-testid="no-results" class="no-results">
          No locations match "{{ search }}"
        </p>
        <button
          v-for="loc in filteredLocations"
          :key="loc.id"
          type="button"
          class="list-row"
          :class="{ selected: selectedLocation?.id === loc.id }"
          :aria-pressed="selectedLocation?.id === loc.id"
          role="option"
          :aria-selected="selectedLocation?.id === loc.id"
          @click="emit('select-location', loc)"
        >
          <span
            v-if="selectedLocation?.id === loc.id"
            data-testid="selected-indicator"
            class="check"
            aria-hidden="true"
          >✓</span>
          <span v-else class="check-placeholder" aria-hidden="true" />
          <span class="row-name">{{ loc.name }}</span>
        </button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.location-picker {
  padding: 0.25rem 0;
}

.list-container {
  border: 1.5px solid var(--color-border, #e2e8f0);
  border-radius: 0.5rem;
  overflow: hidden;
  background: var(--color-card, #fff);
}

.search-wrap {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.625rem 0.875rem;
  border-bottom: 1px solid var(--color-border, #e2e8f0);
  background: var(--color-surface, #f8fafc);
}

.search-icon {
  font-size: 0.875rem;
  flex-shrink: 0;
}

.search-input {
  flex: 1;
  border: none;
  outline: none;
  background: transparent;
  font-size: 0.9rem;
  color: var(--color-text-primary, #0f172a);
}

.search-input::placeholder {
  color: var(--color-text-muted, #94a3b8);
}

.list {
  max-height: 16rem;
  overflow-y: auto;
  overscroll-behavior: contain;
}

.list-row {
  display: flex;
  align-items: center;
  gap: 0.625rem;
  width: 100%;
  padding: 0.625rem 0.875rem;
  border: none;
  border-bottom: 1px solid var(--color-border, #f1f5f9);
  background: transparent;
  color: var(--color-text-primary, #0f172a);
  font-size: 0.9rem;
  font-weight: 500;
  cursor: pointer;
  text-align: left;
  transition: background 0.1s;
}

.list-row:last-child {
  border-bottom: none;
}

.list-row:hover {
  background: var(--color-surface, #f8fafc);
}

.list-row.selected {
  background: var(--color-brand-subtle, #f0fdfa);
  color: var(--color-brand, #0d9488);
}

.check {
  font-size: 0.875rem;
  color: var(--color-brand, #0d9488);
  width: 1rem;
  flex-shrink: 0;
}

.check-placeholder {
  width: 1rem;
  flex-shrink: 0;
}

.row-name {
  flex: 1;
}

.no-results {
  padding: 1rem 0.875rem;
  font-size: 0.875rem;
  color: var(--color-text-muted, #64748b);
  margin: 0;
}

.loading {
  color: var(--color-text-muted, #64748b);
  font-size: 0.95rem;
  padding: 0.5rem 0;
}

.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  padding: 3rem 1rem;
  gap: 0.75rem;
}

.empty-icon {
  font-size: 2.5rem;
  line-height: 1;
}

.empty-title {
  font-size: 1.1rem;
  font-weight: 700;
  color: var(--color-text-primary, #0f172a);
  margin: 0;
}

.empty-body {
  font-size: 0.9rem;
  color: var(--color-text-muted, #64748b);
  margin: 0;
  max-width: 26rem;
  line-height: 1.5;
}
</style>
