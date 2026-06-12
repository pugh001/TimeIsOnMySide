import { test, expect } from '@playwright/test'

test('visits the app root url and shows the booking page', async ({ page }) => {
  await page.route('**/api/locations', (route) => {
    route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ locations: [] }),
    })
  })
  await page.goto('/')
  await expect(page.getByText('Select a location')).toBeVisible()
})
