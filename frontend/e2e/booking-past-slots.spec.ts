import { test, expect } from '@playwright/test'

// Freeze browser clock at 12:00 on 2026-06-09 (today).
// Slots before 12:00 should be unavailable; 12:00 and later should be available.
const FIXED_DATE = '2026-06-09'
const FIXED_TIME_ISO = `${FIXED_DATE}T12:00:00Z`

const MOCK_LOCATION = {
  id: 'loc-e2e-001',
  slug: 'test-branch',
  name: 'Test Branch',
  address: '1 Test Street',
}

const MOCK_SLOTS = {
  slots: [
    { id: 'test-branch-2026-06-09-09:00', date: FIXED_DATE, startTime: '09:00', endTime: '09:30', status: 'unavailable', locationId: 'test-branch' },
    { id: 'test-branch-2026-06-09-11:30', date: FIXED_DATE, startTime: '11:30', endTime: '12:00', status: 'unavailable', locationId: 'test-branch' },
    { id: 'test-branch-2026-06-09-12:00', date: FIXED_DATE, startTime: '12:00', endTime: '12:30', status: 'available',   locationId: 'test-branch' },
    { id: 'test-branch-2026-06-09-14:00', date: FIXED_DATE, startTime: '14:00', endTime: '14:30', status: 'available',   locationId: 'test-branch' },
  ],
}

test.describe('Booking page — past slots are disabled', () => {
  test.beforeEach(async ({ page }) => {
    await page.clock.setFixedTime(FIXED_TIME_ISO)

    await page.route('**/api/locations', (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ locations: [MOCK_LOCATION] }),
      })
    })

    await page.route('**/api/slots**', (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(MOCK_SLOTS),
      })
    })

    await page.goto('/')
    await page.getByRole('option', { name: 'Test Branch' }).click()
  })

  test('past slots are rendered as disabled buttons', async ({ page }) => {
    await expect(page.getByRole('button', { name: /09:00/ })).toBeDisabled()
    await expect(page.getByRole('button', { name: /11:30/ })).toBeDisabled()
  })

  test('past slots have aria-disabled="true"', async ({ page }) => {
    await expect(page.getByRole('button', { name: /09:00/ })).toHaveAttribute('aria-disabled', 'true')
    await expect(page.getByRole('button', { name: /11:30/ })).toHaveAttribute('aria-disabled', 'true')
  })

  test('current and future slots are enabled', async ({ page }) => {
    await expect(page.getByRole('button', { name: /12:00/ })).toBeEnabled()
    await expect(page.getByRole('button', { name: /14:00/ })).toBeEnabled()
  })

  test('clicking a past slot does not open the booking modal', async ({ page }) => {
    await page.getByRole('button', { name: /09:00/ }).click({ force: true })
    await expect(page.getByRole('dialog')).not.toBeVisible()
  })

  test('clicking a future slot opens the booking modal', async ({ page }) => {
    await page.getByRole('button', { name: /14:00/ }).click()
    await expect(page.getByRole('dialog')).toBeVisible()
  })
})
