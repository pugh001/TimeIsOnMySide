import { test, expect } from '@playwright/test'

// Fix "now" before the mock bookings so they all land in "Future"
const FIXED_NOW = '2026-06-09T08:00:00'

const AUTH_SNAPSHOT = JSON.stringify({
  employeeName: 'Jane Doe',
  role: 'staff',
  adminToken: null,
  adminUserId: null,
  staffToken: 'test-staff-token',
  staffUserId: '00000000-0000-0000-0000-000000000099',
  tokenDate: '2026-06-09',
})

const MOCK_BOOKINGS = {
  bookings: [
    {
      bookingRef:   'bk-e2etest1',
      date:         '2026-06-10',
      startTime:    '09:00',
      endTime:      '09:30',
      customerName: 'Alice E2E',
    },
    {
      bookingRef:   'bk-e2etest2',
      date:         '2026-06-11',
      startTime:    '10:00',
      endTime:      '10:30',
      customerName: 'Bob E2E',
    },
  ],
}

const MIXED_BOOKINGS = {
  bookings: [
    {
      bookingRef:   'bk-past01',
      date:         '2026-06-08',
      startTime:    '09:00',
      endTime:      '09:30',
      customerName: 'Past Customer',
    },
    {
      bookingRef:   'bk-fut01',
      date:         '2026-06-10',
      startTime:    '10:00',
      endTime:      '10:30',
      customerName: 'Future Customer',
    },
  ],
}

test.describe('Staff Dashboard — My Bookings', () => {
  test.beforeEach(async ({ page }) => {
    await page.clock.setFixedTime(FIXED_NOW)

    await page.addInitScript((snapshot) => {
      sessionStorage.setItem('auth', snapshot)
    }, AUTH_SNAPSHOT)

    await page.route('**/api/bookings', (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(MOCK_BOOKINGS),
      })
    })
  })

  test('renders the page title', async ({ page }) => {
    await page.goto('/dashboard')
    await expect(page.getByRole('heading', { name: 'My Bookings' })).toBeVisible()
  })

  test('renders Future and Past tabs', async ({ page }) => {
    await page.goto('/dashboard')
    await expect(page.getByTestId('tab-future')).toBeVisible()
    await expect(page.getByTestId('tab-past')).toBeVisible()
  })

  test('future tab is active by default', async ({ page }) => {
    await page.goto('/dashboard')
    await expect(page.getByTestId('tab-future')).toHaveAttribute('aria-selected', 'true')
    await expect(page.getByTestId('tab-past')).toHaveAttribute('aria-selected', 'false')
  })

  test('displays future booking references on future tab', async ({ page }) => {
    await page.goto('/dashboard')
    await expect(page.getByText('bk-e2etest1')).toBeVisible()
    await expect(page.getByText('bk-e2etest2')).toBeVisible()
  })

  test('displays customer names on future tab', async ({ page }) => {
    await page.goto('/dashboard')
    await expect(page.getByText('Alice E2E')).toBeVisible()
    await expect(page.getByText('Bob E2E')).toBeVisible()
  })

  test('displays booking dates and times on future tab', async ({ page }) => {
    await page.goto('/dashboard')
    await expect(page.getByText('2026-06-10')).toBeVisible()
    await expect(page.getByText('09:00')).toBeVisible()
  })

  test('past tab shows empty state when no past bookings', async ({ page }) => {
    await page.goto('/dashboard')
    await page.getByTestId('tab-past').click()
    await expect(page.getByTestId('tab-empty-state')).toBeVisible()
  })

  test('past tab becomes active after click', async ({ page }) => {
    await page.goto('/dashboard')
    await page.getByTestId('tab-past').click()
    await expect(page.getByTestId('tab-past')).toHaveAttribute('aria-selected', 'true')
    await expect(page.getByTestId('tab-future')).toHaveAttribute('aria-selected', 'false')
  })

  test('mixed bookings — future tab shows only future booking', async ({ page }) => {
    await page.route('**/api/bookings', (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(MIXED_BOOKINGS),
      })
    })
    await page.goto('/dashboard')
    await expect(page.getByText('Future Customer')).toBeVisible()
    await expect(page.getByText('Past Customer')).not.toBeVisible()
  })

  test('mixed bookings — past tab shows only past booking', async ({ page }) => {
    await page.route('**/api/bookings', (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(MIXED_BOOKINGS),
      })
    })
    await page.goto('/dashboard')
    await page.getByTestId('tab-past').click()
    await expect(page.getByText('Past Customer')).toBeVisible()
    await expect(page.getByText('Future Customer')).not.toBeVisible()
  })

  test('shows empty state when API returns no bookings', async ({ page }) => {
    await page.route('**/api/bookings', (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ bookings: [] }),
      })
    })
    await page.goto('/dashboard')
    await expect(page.getByTestId('empty-state')).toBeVisible()
    await expect(page.getByText('No bookings assigned yet')).toBeVisible()
  })

  test('shows error message when API returns 401', async ({ page }) => {
    await page.route('**/api/bookings', (route) => {
      route.fulfill({ status: 401, body: '' })
    })
    await page.goto('/dashboard')
    await expect(page.getByTestId('api-error')).toBeVisible()
    await expect(page.getByText(/unauthorized/i)).toBeVisible()
  })
})
