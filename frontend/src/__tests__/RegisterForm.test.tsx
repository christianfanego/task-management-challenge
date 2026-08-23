import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { RegisterForm } from "../components/RegisterForm";

describe("RegisterForm", () => {
  const defaultProps = {
    onSubmit: vi.fn(),
    error: null,
    isLoading: false,
  };

  beforeEach(() => {
    defaultProps.onSubmit.mockClear();
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it("renders email, password, and confirm password fields", () => {
    render(<RegisterForm {...defaultProps} />);

    expect(screen.getByLabelText(/email/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/^password$/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/confirm password/i)).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: /create account/i }),
    ).toBeInTheDocument();
  });

  it("calls onSubmit with email and password on valid submission", async () => {
    const user = userEvent.setup();
    defaultProps.onSubmit.mockResolvedValue(undefined);

    render(<RegisterForm {...defaultProps} />);

    await user.type(screen.getByLabelText(/email/i), "new@example.com");
    await user.type(screen.getByLabelText(/^password$/i), "Password1!");
    await user.type(screen.getByLabelText(/confirm password/i), "Password1!");
    await user.click(screen.getByRole("button", { name: /create account/i }));

    expect(defaultProps.onSubmit).toHaveBeenCalledWith(
      "new@example.com",
      "Password1!",
    );
  });

  it("does not call onSubmit when passwords do not match", async () => {
    const user = userEvent.setup();

    render(<RegisterForm {...defaultProps} />);

    await user.type(screen.getByLabelText(/email/i), "new@example.com");
    await user.type(screen.getByLabelText(/^password$/i), "Password1!");
    await user.type(screen.getByLabelText(/confirm password/i), "Different1!");
    await user.click(screen.getByRole("button", { name: /create account/i }));

    expect(defaultProps.onSubmit).not.toHaveBeenCalled();
    expect(screen.getByText(/passwords do not match/i)).toBeInTheDocument();
  });

  it("does not call onSubmit when fields are empty", async () => {
    const user = userEvent.setup();

    render(<RegisterForm {...defaultProps} />);

    await user.click(screen.getByRole("button", { name: /create account/i }));

    expect(defaultProps.onSubmit).not.toHaveBeenCalled();
  });

  it("displays error message when provided", () => {
    render(<RegisterForm {...defaultProps} error="Email already registered" />);

    expect(screen.getByRole("alert")).toHaveTextContent(
      "Email already registered",
    );
  });

  it("disables button and shows loading text when isLoading is true", () => {
    render(<RegisterForm {...defaultProps} isLoading={true} />);

    const button = screen.getByRole("button", { name: /creating/i });
    expect(button).toBeDisabled();
  });
});
