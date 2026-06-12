import { ref, computed } from 'vue'
import { useBookingStore } from '@/stores/bookingStore'

function dateToIso(d: Date): string {
  return d.toISOString().slice(0, 10)
}

function todayIso(): string {
  return dateToIso(new Date())
}

export type DateEntry = { iso: string; dayName: string; dayNum: number }

const DAY_NAMES = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat']

export function useSlots() {
  const store = useBookingStore()
  const today = ref(todayIso())
  const selectedDate = ref(todayIso())

  const weekDates = computed<DateEntry[]>(() => {
    return Array.from({ length: 7 }, (_, i) => {
      const d = new Date(today.value + 'T12:00:00')
      d.setDate(d.getDate() + i)
      return {
        iso: dateToIso(d),
        dayName: DAY_NAMES[d.getDay()] ?? '',
        dayNum: d.getDate(),
      }
    })
  })

  const slotsForDate = computed(() => store.slots.filter((s) => s.date === selectedDate.value))

  function setDate(date: string): void {
    selectedDate.value = date
  }

  return { today, selectedDate, weekDates, slotsForDate, setDate }
}
