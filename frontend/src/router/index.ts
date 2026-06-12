import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'
import BookingView from '../views/BookingView.vue'
import LoginView from '../views/LoginView.vue'
import AddLocationView from '../views/AddLocationView.vue'
import AddUserView from '../views/AddUserView.vue'
import StaffDashboardView from '../views/StaffDashboardView.vue'
import AdminDashboardView from '../views/AdminDashboardView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'booking',
      component: BookingView,
    },
    {
      path: '/login',
      name: 'login',
      component: LoginView,
    },
    {
      path: '/admin',
      name: 'admin',
      component: AdminDashboardView,
      meta: { requiresRole: 'admin' },
    },
    {
      path: '/locations/new',
      name: 'add-location',
      component: AddLocationView,
      meta: { requiresRole: 'admin' },
    },
    {
      path: '/users/new',
      name: 'add-user',
      component: AddUserView,
      meta: { requiresRole: 'admin' },
    },
    {
      path: '/dashboard',
      name: 'dashboard',
      component: StaffDashboardView,
      meta: { requiresAuth: true },
    },
  ],
})

router.beforeEach((to) => {
  const auth = useAuthStore()

  if (to.meta.requiresRole === 'admin') {
    if (auth.role !== 'admin') {
      auth.logout()
      return { name: 'login' }
    }
    if (!auth.isTokenFresh) {
      auth.logout()
      return { name: 'login' }
    }
  }

  if (to.meta.requiresAuth) {
    if (!auth.isLoggedIn) {
      auth.logout()
      return { name: 'login' }
    }
    if (!auth.isTokenFresh) {
      auth.logout()
      return { name: 'login' }
    }
  }

  if (to.name === 'booking' && auth.isLoggedIn) {
    if (auth.role === 'admin') return { name: 'admin' }
    if (auth.role === 'staff') return { name: 'dashboard' }
  }
})

export default router
