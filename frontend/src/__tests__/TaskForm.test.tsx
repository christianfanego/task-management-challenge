import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { TaskForm } from "../components/TaskForm";

describe("TaskForm", () => {
  const defaultProps = {
    onSubmit: vi.fn(),
    onCancel: vi.fn(),
    isLoading: false,
    initialData: undefined,
  };

  beforeEach(() => {
    defaultProps.onSubmit.mockClear();
    defaultProps.onCancel.mockClear();
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it("renders create form with empty fields", () => {
    render(<TaskForm {...defaultProps} />);

    expect(screen.getByLabelText(/title/i)).toHaveValue("");
    expect(screen.getByLabelText(/description/i)).toHaveValue("");
    expect(screen.getByLabelText(/status/i)).toHaveValue("Pending");
    expect(screen.getByPlaceholderText(/yyyy-mm-dd/i)).toHaveValue("");
    expect(
      screen.getByRole("button", { name: /create task/i }),
    ).toBeInTheDocument();
  });

  it("renders edit form with initial data", () => {
    render(
      <TaskForm
        {...defaultProps}
        initialData={{
          id: "task-1",
          title: "Existing Task",
          description: "Existing desc",
          status: "InProgress",
          dueDate: "2026-10-01",
          createdAt: "",
          updatedAt: "",
        }}
      />,
    );

    expect(screen.getByLabelText(/title/i)).toHaveValue("Existing Task");
    expect(screen.getByLabelText(/description/i)).toHaveValue("Existing desc");
    expect(screen.getByLabelText(/status/i)).toHaveValue("InProgress");
    expect(screen.getByPlaceholderText(/yyyy-mm-dd/i)).toHaveValue("2026-10-01");
    expect(
      screen.getByRole("button", { name: /update task/i }),
    ).toBeInTheDocument();
  });

  it("calls onSubmit with form data on valid submission", async () => {
    const user = userEvent.setup();
    defaultProps.onSubmit.mockResolvedValue(undefined);

    render(<TaskForm {...defaultProps} />);

    await user.type(screen.getByLabelText(/title/i), "New Task");
    await user.type(screen.getByLabelText(/description/i), "A description");
    await user.selectOptions(screen.getByLabelText(/status/i), "InProgress");
    await user.type(screen.getByPlaceholderText(/yyyy-mm-dd/i), "2026-10-15");
    await user.click(screen.getByRole("button", { name: /create task/i }));

    expect(defaultProps.onSubmit).toHaveBeenCalledWith({
      title: "New Task",
      description: "A description",
      status: "InProgress",
      dueDate: "2026-10-15",
    });
  });

  it("does not submit when title is empty", async () => {
    const user = userEvent.setup();

    render(<TaskForm {...defaultProps} />);

    await user.click(screen.getByRole("button", { name: /create task/i }));

    expect(defaultProps.onSubmit).not.toHaveBeenCalled();
  });

  it("shows error when title exceeds 120 characters", async () => {
    const user = userEvent.setup();
    render(<TaskForm {...defaultProps} />);

    const titleInput = screen.getByLabelText(/title/i);
    await user.type(titleInput, "a".repeat(121));
    await user.click(screen.getByRole("button", { name: /create task/i }));

    expect(screen.getByText(/at most 120/i)).toBeInTheDocument();
    expect(defaultProps.onSubmit).not.toHaveBeenCalled();
  });

  it("validates description length on submit", async () => {
    const user = userEvent.setup();
    render(<TaskForm {...defaultProps} />);

    const titleInput = screen.getByLabelText(/title/i);
    await user.type(titleInput, "Task Title");
    const descInput = screen.getByLabelText(/description/i);
    await user.type(descInput, "Valid description");
    await user.click(screen.getByRole("button", { name: /create task/i }));

    expect(defaultProps.onSubmit).toHaveBeenCalled();
  });

  it("calls onCancel when cancel button is clicked", async () => {
    const user = userEvent.setup();

    render(<TaskForm {...defaultProps} />);

    await user.click(screen.getByRole("button", { name: /cancel/i }));

    expect(defaultProps.onCancel).toHaveBeenCalled();
  });

  it("disables submit button when isLoading", () => {
    render(<TaskForm {...defaultProps} isLoading={true} />);

    const button = screen.getByRole("button", { name: /saving/i });
    expect(button).toBeDisabled();
  });

  it("allows due date to be cleared", async () => {
    const user = userEvent.setup();
    defaultProps.onSubmit.mockResolvedValue(undefined);

    render(
      <TaskForm
        {...defaultProps}
        initialData={{
          id: "task-1",
          title: "Task",
          description: null,
          status: "Pending",
          dueDate: "2026-10-01",
          createdAt: "",
          updatedAt: "",
        }}
      />,
    );

    await user.clear(screen.getByPlaceholderText(/yyyy-mm-dd/i));
    await user.click(screen.getByRole("button", { name: /update task/i }));

    expect(defaultProps.onSubmit).toHaveBeenCalledWith(
      expect.objectContaining({ dueDate: "" }),
    );
  });
});
