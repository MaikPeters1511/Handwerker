import { expect, test } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';

const routes = ['/invoices', '/invoices/new'];

for (const route of routes) {
  test(`AXE: ${route} hat keine kritischen/ernsten Verstöße`, async ({ page, baseURL }) => {
    await page.goto(route, { waitUntil: 'domcontentloaded' });

    await expect(page.locator('body')).toBeVisible();

    const results = await new AxeBuilder({ page })
      .withTags(['wcag2a', 'wcag2aa'])
      .analyze();

    const blockingViolations = results.violations.filter(
      violation => violation.impact === 'critical' || violation.impact === 'serious'
    );

    expect(
      blockingViolations,
      [
        `AXE-Verstöße auf ${baseURL ?? ''}${route}:`,
        ...blockingViolations.map(v => `${v.id} (${v.impact}) – ${v.help}`)
      ].join('\n')
    ).toHaveLength(0);
  });
}
