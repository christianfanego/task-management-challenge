import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { LoginForm } from "../components/LoginForm";

describe("LoginForm", () => {
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

  it("renders email and password fields", () => {
    render(<LoginForm {...defaultProps} />);

    expect(screen.getByLabelText(/email/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/password/i)).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: /sign in/i }),
    ).toBeInTheDocument();
  });

  it("calls onSubmit with email and password on valid submission", async () => {
    const user = userEvent.setup();
    defaultProps.onSubmit.mockResolvedValue(undefined);

    render(<LoginForm {...defaultProps} />);

    await user.type(screen.getByLabelText(/email/i), "test@example.com");
    await user.type(screen.getByLabelText(/password/i), "Password1!");
    await user.click(screen.getByRole("button", { name: /sign in/i }));

    expect(defaultProps.onSubmit).toHaveBeenCalledWith(
      "test@example.com",
      "Password1!",
    );
  });

  it("does not call onSubmit when email is empty", async () => {
    const user = userEvent.setup();

    render(<LoginForm {...defaultProps} />);

    await user.type(screen.getByLabelText(/password/i), "Password1!");
    await user.click(screen.getByRole("button", { name: /sign in/i }));

    expect(defaultProps.onSubmit).not.toHaveBeenCalled();
  });

  it("does not call onSubmit when password is empty", async () => {
    const user = userEvent.setup();

    render(<LoginForm {...defaultProps} />);

    await user.type(screen.getByLabelText(/email/i), "test@example.com");
    await user.click(screen.getByRole("button", { name: /sign in/i }));

    expect(defaultProps.onSubmit).not.toHaveBeenCalled();
  });

  it("displays error message when provided", () => {
    render(<LoginForm {...defaultProps} error="Invalid credentials" />);

    expect(screen.getByRole("alert")).toHaveTextContent("Invalid credentials");
  });

  it("disables button and shows loading text when isLoading is true", () => {
    render(<LoginForm {...defaultProps} isLoading={true} />);

    const button = screen.getByRole("button", { name: /signing in/i });
    expect(button).toBeDisabled();
  });

  it("does not show error when error is null", () => {
    render(<LoginForm {...defaultProps} error={null} />);

    expect(screen.queryByRole("alert")).not.toBeInTheDocument();
  });
});
