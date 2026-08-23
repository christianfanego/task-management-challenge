import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { render, screen } from "@testing-library/react";
import { TaskList } from "../components/TaskList";
import type { TaskDto } from "../api/types";

const mockTasks: TaskDto[] = [
  {
    id: "task-1",
    title: "First Task",
    description: "Description one",
    status: "Pending",
    dueDate: "2026-09-01",
    createdAt: "2026-08-01T00:00:00Z",
    updatedAt: "2026-08-01T00:00:00Z",
  },
  {
    id: "task-2",
    title: "Second Task",
    description: null,
    status: "Completed",
    dueDate: null,
    createdAt: "2026-08-02T00:00:00Z",
    updatedAt: "2026-08-02T00:00:00Z",
  },
];

describe("TaskList", () => {
  const defaultProps = {
    tasks: [],
    isLoading: false,
    error: null,
    onEdit: vi.fn(),
    onDelete: vi.fn(),
  };

  beforeEach(() => {
    defaultProps.onEdit.mockClear();
    defaultProps.onDelete.mockClear();
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it("shows loading state", () => {
    render(<TaskList {...defaultProps} isLoading={true} />);

    expect(screen.getByText(/loading/i)).toBeInTheDocument();
  });

  it("shows empty state when no tasks", () => {
    render(<TaskList {...defaultProps} tasks={[]} />);

    expect(screen.getByText(/no tasks yet/i)).toBeInTheDocument();
  });

  it("renders task list with tasks", () => {
    render(<TaskList {...defaultProps} tasks={mockTasks} />);

    expect(screen.getByText("First Task")).toBeInTheDocument();
    expect(screen.getByText("Second Task")).toBeInTheDocument();
    expect(screen.getByText("Description one")).toBeInTheDocument();
  });

  it("displays task status badges", () => {
    render(<TaskList {...defaultProps} tasks={mockTasks} />);

    expect(screen.getAllByText("Pending").length).toBeGreaterThanOrEqual(1);
    expect(screen.getAllByText("Completed").length).toBeGreaterThanOrEqual(1);
  });

  it("displays formatted due date when present", () => {
    render(<TaskList {...defaultProps} tasks={mockTasks} />);

    expect(screen.getByText(/Sep 1, 2026/)).toBeInTheDocument();
  });

  it("shows error state", () => {
    render(<TaskList {...defaultProps} error="Failed to load tasks" />);

    expect(screen.getByRole("alert")).toHaveTextContent("Failed to load tasks");
  });

  it("calls onEdit when edit button is clicked", async () => {
    const { default: userEvent } = await import("@testing-library/user-event");
    const user = userEvent.setup();

    render(<TaskList {...defaultProps} tasks={mockTasks} />);

    const editButtons = screen.getAllByRole("button", { name: /edit/i });
    await user.click(editButtons[0]!);

    expect(defaultProps.onEdit).toHaveBeenCalledWith(mockTasks[0]);
  });

  it("calls onDelete when delete is confirmed", async () => {
    const { default: userEvent } = await import("@testing-library/user-event");
    const user = userEvent.setup();
    vi.spyOn(window, "confirm").mockReturnValue(true);

    render(<TaskList {...defaultProps} tasks={mockTasks} />);

    const deleteButtons = screen.getAllByRole("button", { name: /delete/i });
    await user.click(deleteButtons[0]!);

    expect(defaultProps.onDelete).toHaveBeenCalledWith("task-1");
    vi.restoreAllMocks();
  });

  it("does not call onDelete when delete is cancelled", async () => {
    const { default: userEvent } = await import("@testing-library/user-event");
    const user = userEvent.setup();
    vi.spyOn(window, "confirm").mockReturnValue(false);

    render(<TaskList {...defaultProps} tasks={mockTasks} />);

    const deleteButtons = screen.getAllByRole("button", { name: /delete/i });
    await user.click(deleteButtons[0]!);

    expect(defaultProps.onDelete).not.toHaveBeenCalled();
    vi.restoreAllMocks();
  });

  it("renders nothing when not loading, no error, and empty tasks", () => {
    render(<TaskList {...defaultProps} tasks={[]} />);

    expect(screen.getByText(/no tasks yet/i)).toBeInTheDocument();
  });
});
