import { chromium } from 'playwright';
import { join, dirname } from 'path';
import { fileURLToPath } from 'url';

const __dirname = dirname(fileURLToPath(import.meta.url));
const SCREENSHOTS_DIR = join(__dirname, 'assets', 'screenshots');

const BASE = 'https://hotel-web-demo1.ambitiousmushroom-274454dc.centralindia.azurecontainerapps.io';

const accounts = {
  admin:   { email: 'admin@aetheris.com',        pass: 'Pass@1234' },
  guest:   { email: 'cust2@gmail.com',            pass: 'Pass@1234' },
  kitchen: { email: 'kitchen@aetheris.com',      pass: 'Pass@1234' },
  hk:      { email: 'hk1@aetheris.com',          pass: 'Pass@1234' },
  maint:   { email: 'maintenance@aetheris.com',  pass: 'Pass@1234' },
};

async function login(page, email, password) {
  await page.goto(`${BASE}/auth`, { waitUntil: 'networkidle' });
  await page.waitForTimeout(1500);
  await page.fill('#login-email', email);
  await page.fill('#login-password', password);
  await page.click('.submit-btn');
  await page.waitForURL(url => !url.pathname.includes('/auth'), { timeout: 15000 });
  await page.waitForTimeout(2000);
}

async function screenshot(page, name, opts = {}) {
  const path = join(SCREENSHOTS_DIR, `${name}.png`);
  if (opts.fullPage) {
    await page.screenshot({ path, fullPage: true });
  } else {
    await page.screenshot({ path });
  }
  console.log(`  ✓ ${name}.png`);
}

async function captureAll() {
  const browser = await chromium.launch({ headless: true });

  // ---- 1. Public pages (no auth) ----
  console.log('\n📸 Public pages');
  {
    const pub = await browser.newPage();
    await pub.setViewportSize({ width: 1920, height: 1080 });

    // Home (full-page scroll)
    await pub.goto(`${BASE}/home`, { waitUntil: 'networkidle' });
    await pub.waitForTimeout(3000);
    await screenshot(pub, 'home-fullpage', { fullPage: true });

    // Rooms catalogue — full scroll to show all rooms
    await pub.goto(`${BASE}/rooms`, { waitUntil: 'networkidle' });
    await pub.waitForTimeout(3000);
    await screenshot(pub, 'rooms-catalogue', { fullPage: true });

    // Room detail with carousel scroll
    await pub.goto(`${BASE}/rooms/8`, { waitUntil: 'networkidle' });
    await pub.waitForTimeout(3000);
    await pub.evaluate(() => {
      const gallery = document.querySelector('.gallery-scroll');
      if (gallery) gallery.scrollLeft = 1000;
    });
    await pub.waitForTimeout(500);
    await screenshot(pub, 'room-detail-carousel');

    // Experiences page — Dessert accordion open, first amenity hovered
    await pub.goto(`${BASE}/experiences`, { waitUntil: 'networkidle' });
    await pub.waitForTimeout(3000);
    // Click Dessert accordion header to open it
    const dessertHeader = pub.locator('.menu-row-header', { hasText: /Dessert/i });
    if (await dessertHeader.count() > 0) {
      await dessertHeader.first().click();
      await pub.waitForTimeout(1000);
    }
    // Scroll down to trigger lazy-load of amenities section
    await pub.evaluate(() => window.scrollTo(0, document.body.scrollHeight));
    await pub.waitForTimeout(1500);
    // Scroll back to top
    await pub.evaluate(() => window.scrollTo(0, 0));
    await pub.waitForTimeout(500);
    // Force hover on first amenity card to show overlay
    await pub.evaluate(() => {
      const card = document.querySelector('.amenity-card');
      if (card) {
        const overlay = card.querySelector('.hover-overlay');
        if (overlay) {
          overlay.style.setProperty('transform', 'translateY(0)', 'important');
          overlay.style.setProperty('transition', 'none', 'important');
        }
      }
    });
    await pub.waitForTimeout(500);
    await screenshot(pub, 'experiences', { fullPage: true });

    await pub.close();
  }

  // ---- 2. Guest portal ----
  console.log('\n👤 Guest portal (cust2@gmail.com)');
  {
    const guest = await browser.newPage();
    await guest.setViewportSize({ width: 1920, height: 1080 });
    await login(guest, accounts.guest.email, accounts.guest.pass);

    // Dashboard
    await guest.goto(`${BASE}/user/dashboard`, { waitUntil: 'networkidle' });
    await guest.waitForTimeout(3000);
    await screenshot(guest, 'user-dashboard');

    // Bookings
    await guest.goto(`${BASE}/user/bookings`, { waitUntil: 'networkidle' });
    await guest.waitForTimeout(3000);
    await screenshot(guest, 'user-bookings');

    // Atlas AI concierge — real responses with actual queries
    await guest.goto(`${BASE}/user/dashboard`, { waitUntil: 'networkidle' });
    await guest.waitForTimeout(3000);
    // Open Atlas concierge FAB
    const fab = guest.locator('.concierge-fab');
    if (await fab.isVisible()) {
      await fab.click();
      await guest.waitForTimeout(1500);
    }
    // Click "Check Bill" quick action
    const checkBill = guest.locator('.quick-actions').getByText('Check Bill');
    if (await checkBill.isVisible()) {
      await checkBill.click();
      console.log('  → Sent: "What is my current bill?"');
    }
    // Wait for AI response (typing indicator disappears)
    await guest.waitForFunction(() => !document.querySelector('.typing-indicator'), { timeout: 45000 });
    await guest.waitForTimeout(1500);
    console.log('  → Bill response received');
    // Type second query
    const chatInput = guest.locator('.concierge-chat input');
    if (await chatInput.isVisible()) {
      await chatInput.fill('I just got into my room. Get me some extra towels and send someone to fix the AC.');
      await chatInput.press('Enter');
      console.log('  → Sent: "towels + fix AC"');
    }
    // Wait for AI response
    await guest.waitForFunction(() => !document.querySelector('.typing-indicator'), { timeout: 45000 });
    await guest.waitForTimeout(1500);
    console.log('  → AC/towels response received');
    // Zoom to 70% so full conversation is visible
    await guest.evaluate(() => { document.body.style.zoom = '0.7'; });
    await guest.waitForTimeout(500);
    await screenshot(guest, 'user-atlas-concierge');

    // Room service menu — quick cameo
    await guest.goto(`${BASE}/user/room-service`, { waitUntil: 'networkidle' });
    await guest.waitForTimeout(3000);
    await screenshot(guest, 'room-service-menu');

    await guest.close();
  }

  // ---- 3. Admin portal ----
  console.log('\n🛡️ Admin portal');
  {
    const admin = await browser.newPage();
    await admin.setViewportSize({ width: 1920, height: 1080 });
    await login(admin, accounts.admin.email, accounts.admin.pass);

    await admin.goto(`${BASE}/operations/admin/dashboard`, { waitUntil: 'networkidle' });
    await admin.waitForTimeout(3000);
    await screenshot(admin, 'admin-dashboard');

    await admin.goto(`${BASE}/operations/admin/management/room`, { waitUntil: 'networkidle' });
    await admin.waitForTimeout(3000);
    await screenshot(admin, 'admin-room-management');

    await admin.goto(`${BASE}/operations/admin/oversight/analytics`, { waitUntil: 'networkidle' });
    await admin.waitForTimeout(3000);
    // Zoom to 90% to show full analytics page
    await admin.evaluate(() => { document.body.style.zoom = '0.9'; });
    await admin.waitForTimeout(500);
    await screenshot(admin, 'admin-analytics');

    await admin.goto(`${BASE}/operations/admin/oversight/feedback`, { waitUntil: 'networkidle' });
    await admin.waitForTimeout(3000);
    await screenshot(admin, 'admin-feedback');

    await admin.close();
  }

  // ---- 4. Kitchen ----
  console.log('\n🍳 Kitchen portal');
  {
    const kitchen = await browser.newPage();
    await kitchen.setViewportSize({ width: 1920, height: 1080 });
    await login(kitchen, accounts.kitchen.email, accounts.kitchen.pass);
    await kitchen.goto(`${BASE}/operations/kitchen/dashboard`, { waitUntil: 'networkidle' });
    await kitchen.waitForTimeout(3000);
    await screenshot(kitchen, 'kitchen-dashboard');
    await kitchen.close();
  }

  // ---- 5. Housekeeping ----
  console.log('\n🧹 Housekeeping portal');
  {
    const hk = await browser.newPage();
    await hk.setViewportSize({ width: 1920, height: 1080 });
    await login(hk, accounts.hk.email, accounts.hk.pass);
    await hk.goto(`${BASE}/operations/housekeeping/dashboard`, { waitUntil: 'networkidle' });
    await hk.waitForTimeout(3000);
    await screenshot(hk, 'housekeeping-dashboard');
    await hk.close();
  }

  // ---- 6. Maintenance ----
  console.log('\n🔧 Maintenance portal');
  {
    const maint = await browser.newPage();
    await maint.setViewportSize({ width: 1920, height: 1080 });
    await login(maint, accounts.maint.email, accounts.maint.pass);
    await maint.goto(`${BASE}/operations/maintenance/dashboard`, { waitUntil: 'networkidle' });
    await maint.waitForTimeout(3000);
    await screenshot(maint, 'maintenance-dashboard');
    await maint.close();
  }

  await browser.close();
  console.log('\n✅ All screenshots captured!');
}

captureAll().catch(err => { console.error(err); process.exit(1); });
