import { describe, it, expect } from "vitest";
import { render, screen } from "@testing-library/react";

/**
 * Harness Admission Test (ARCH-TEST-001 / U1.1)
 *
 * This test verifies the frontend test harness is correctly configured:
 * - Vitest runs and discovers test files
 * - React Testing Library can render components
 * - jsdom environment is active
 * - TypeScript compilation works
 * - Basic assertions work
 *
 * This is NOT a feature test — it proves the harness exists and works.
 */

function HarnessProbe() {
  return <div data-testid="harness-active">harness-ok</div>;
}

describe("Frontend Test Harness Admission", () => {
  it("renders a component via React Testing Library", () => {
    render(<HarnessProbe />);
    expect(screen.getByTestId("harness-active")).toHaveTextContent(
      "harness-ok",
    );
  });

  it("confirms jsdom environment is active", () => {
    expect(typeof window).toBe("object");
    expect(typeof document).toBe("object");
  });

  it("confirms Vitest globals are available", () => {
    expect(describe).toBeDefined();
    expect(it).toBeDefined();
    expect(expect).toBeDefined();
  });
});
