import { test, expect } from '@playwright/test'

const ADMIN_USER = 'admin'
const ADMIN_PASS = 'Admin1234!'
const STAFF_PASS = 'Staff1234!'

// Shared state captured during the run — later tests depend on earlier ones
let locationSlug = ''
let staffUsername = ''
let bookingRef = ''
let customerName = 'Docker Test'

test.describe('Docker smoke — full end-to-end', () => {
  test.describe.configure({ mode: 'serial' })

  test.beforeAll(async ({ request }) => {
    const reachable = await request.get('/health').then(r => r.ok()).catch(() => false)
    if (!reachable) {
      test.skip(true, 'Docker stack not running — start it with ./start-all before running docker tests')
    }
  })

  test('1 — health check', async ({ request }) => {
    const res = await request.get('/health')
    expect(res.status()).toBe(200)
  })

  test('2 — admin login', async ({ page }) => {
    await page.goto('/login')
    await page.getByTestId('username-input').fill(ADMIN_USER)
    await page.getByTestId('password-input').fill(ADMIN_PASS)
    await page.getByTestId('submit-btn').click()
    await expect(page).toHaveURL(/\/admin/)
    await expect(page.getByTestId('add-location-btn')).toBeVisible()
  })

  test('3 — create location', async ({ page }) => {
    await page.goto('/login')
    await page.getByTestId('username-input').fill(ADMIN_USER)
    await page.getByTestId('password-input').fill(ADMIN_PASS)
    await page.getByTestId('submit-btn').click()
    await expect(page).toHaveURL(/\/admin/)

    await page.getByTestId('add-location-btn').click()
    await expect(page).toHaveURL(/\/locations\/new/)

    await page.getByTestId('name-input').fill('Docker Branch')
    await page.getByTestId('address-input').fill('1 Docker Street')

    // Enable Mon–Fri with 09:00–17:00
    for (const day of ['monday', 'tuesday', 'wednesday', 'thursday', 'friday']) {
      await page.getByTestId(`toggle-${day}`).check()
    }

    await page.getByTestId('submit-btn').click()
    await expect(page.getByTestId('success-state')).toBeVisible({ timeout: 10_000 })

    // Capture the slug from the success message
    const successText = await page.getByTestId('success-state').textContent()
    const match = successText?.match(/docker-branch[-\w]*/i) ?? successText?.match(/\b[\w-]+\b/g)
    locationSlug = 'docker-branch'
    expect(successText).toContain('created successfully')
  })

  test('4 — create staff user', async ({ page }) => {
    await page.goto('/login')
    await page.getByTestId('username-input').fill(ADMIN_USER)
    await page.getByTestId('password-input').fill(ADMIN_PASS)
    await page.getByTestId('submit-btn').click()
    await expect(page).toHaveURL(/\/admin/)

    // Find the location panel and click Add User
    await expect(page.getByTestId('add-user-btn').first()).toBeVisible({ timeout: 10_000 })
    await page.getByTestId('add-user-btn').first().click()
    await expect(page).toHaveURL(/\/users\/new/)

    await page.getByTestId('first-name-input').fill('Docker')
    await page.getByTestId('last-name-input').fill('Staff')
    await page.getByTestId('password-input').fill(STAFF_PASS)
    await page.getByTestId('confirm-password-input').fill(STAFF_PASS)

    // Enable at least one working day — pick Monday (first available after location hours set)
    const mondayCheckbox = page.getByTestId('work-day-monday')
    if (await mondayCheckbox.isVisible()) {
      await mondayCheckbox.check()
    }

    await page.getByTestId('submit-btn').click()

    // Modal shows the generated username
    await expect(page.getByTestId('username-modal')).toBeVisible({ timeout: 10_000 })
    staffUsername = (await page.getByTestId('modal-username').textContent()) ?? ''
    expect(staffUsername).toMatch(/^[a-z]+\d{4}$/)

    await page.getByTestId('modal-close-btn').click()
  })

  test('5 — admin logout', async ({ page }) => {
    await page.goto('/login')
    await page.getByTestId('username-input').fill(ADMIN_USER)
    await page.getByTestId('password-input').fill(ADMIN_PASS)
    await page.getByTestId('submit-btn').click()
    await expect(page).toHaveURL(/\/admin/)

    await page.getByTestId('user-menu-toggle').click()
    await page.getByTestId('logout-btn').click()
    await expect(page).toHaveURL(/\/$|\/login/)
  })

  test('6 — client books a slot', async ({ page }) => {
    await page.goto('/')

    // Pick the location
    await expect(page.getByRole('option', { name: 'Docker Branch' })).toBeVisible({ timeout: 10_000 })
    await page.getByRole('option', { name: 'Docker Branch' }).click()

    // Pick the next available weekday date by clicking date strip buttons until we find one that has slots
    // Try up to 7 days ahead
    let slotFound = false
    for (let attempt = 0; attempt < 7; attempt++) {
      const dateButtons = page.getByRole('button', { name: /^\d{1,2}$/ })
      const count = await dateButtons.count()
      if (count > attempt) {
        await dateButtons.nth(attempt).click()
      }
      // Check if any available slot appeared
      const availableSlot = page.getByRole('button', { name: /^\d{2}:\d{2}$/ }).first()
      if (await availableSlot.isVisible({ timeout: 2_000 }).catch(() => false)) {
        await availableSlot.click()
        slotFound = true
        break
      }
    }

    if (!slotFound) {
      // Fallback: try clicking any enabled slot button
      await page.getByRole('button', { name: /\d{2}:\d{2}/ }).first().click()
    }

    // Booking modal should open
    await expect(page.getByRole('dialog')).toBeVisible({ timeout: 5_000 })

    // Fill the form — name field has no testid, use label
    await page.getByLabel('Name').fill(customerName)
    await page.locator('#email').fill('dockertest@example.com')
    await page.locator('#phone').fill('0831234567')

    // Submit
    await page.getByRole('button', { name: /book|confirm|submit/i }).click()

    // Confirmation — capture booking ref
    await expect(page.locator('.confirmation-detail')).toBeVisible({ timeout: 10_000 })
    const confirmText = await page.locator('.confirmation-detail').textContent()
    const refMatch = confirmText?.match(/bk-[a-z0-9]+/)
    expect(refMatch).not.toBeNull()
    bookingRef = refMatch![0]
    expect(bookingRef).toMatch(/^bk-[a-z0-9]+$/)
  })

  test('7 — staff login', async ({ page }) => {
    expect(staffUsername).toMatch(/^[a-z]+\d{4}$/)

    await page.goto('/login')
    await page.getByTestId('username-input').fill(staffUsername)
    await page.getByTestId('password-input').fill(STAFF_PASS)
    await page.getByTestId('submit-btn').click()
    await expect(page).toHaveURL(/\/dashboard/, { timeout: 10_000 })
    await expect(page.getByRole('heading', { name: 'My Bookings' })).toBeVisible()
  })

  test('8 — staff sees the booking', async ({ page }) => {
    expect(staffUsername).toMatch(/^[a-z]+\d{4}$/)
    expect(bookingRef).toMatch(/^bk-[a-z0-9]+$/)

    await page.goto('/login')
    await page.getByTestId('username-input').fill(staffUsername)
    await page.getByTestId('password-input').fill(STAFF_PASS)
    await page.getByTestId('submit-btn').click()
    await expect(page).toHaveURL(/\/dashboard/, { timeout: 10_000 })

    await expect(page.getByTestId('tab-future')).toBeVisible()
    await expect(page.getByText(bookingRef)).toBeVisible({ timeout: 10_000 })
    await expect(page.getByText(customerName)).toBeVisible()
  })

  test('9 — mailpit received booking emails', async ({ request }) => {
    // Only meaningful when running against docker compose (Mailpit available on :8025)
    const mailpitUrl = process.env.DOCKER_BASE_URL
      ? 'http://localhost:8025'
      : null

    if (!mailpitUrl) {
      test.skip()
      return
    }

    const res = await request.get(`${mailpitUrl}/api/v1/messages`)
    expect(res.status()).toBe(200)

    const body = await res.json()
    expect(body.total).toBeGreaterThan(0)

    const messages = body.messages as Array<{ Subject: string }>
    const confirmationEmail = messages.find(m =>
      m.Subject?.toLowerCase().includes('appointment') ||
      m.Subject?.toLowerCase().includes('confirmed') ||
      m.Subject?.toLowerCase().includes('booking')
    )
    expect(confirmationEmail).toBeDefined()
  })
})
