import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { describe, it, expect, vi, beforeEach } from "vitest";
import Login from "../../../src/pages/Login";
import PathDisplay from "../PathDisplay";

const App = () => (
  <MemoryRouter initialEntries={["/login"]}>
    <Routes>
      <Route path="login" element={<Login/>}/>
      <Route path="/*" element={<PathDisplay/>}/>
    </Routes>
  </MemoryRouter>
)

describe("Login", () => {
  beforeEach(() => {
    localStorage.clear();
    vi.restoreAllMocks();
  });

  it("renders email, password and account type fields", () => {
    render(App());

    expect(screen.getByLabelText("Email")).toBeInTheDocument();
    expect(screen.getByLabelText("Password")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Creator" })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Member" })).toBeInTheDocument();
  });

  it("submit button is disabled when form is empty", () => {
    render(App());

    expect(screen.getByRole("button", { name: "Log in" })).toBeDisabled();
  });

  it("shows validation errors on blur with empty fields", async () => {
    render(App());

    fireEvent.blur(screen.getByLabelText("Email"));
    fireEvent.blur(screen.getByLabelText("Password"));

    await waitFor(() => {
      expect(screen.getByText("Email is required.")).toBeInTheDocument();
      expect(screen.getByText("Password is required.")).toBeInTheDocument();
    });
  });

  it("navigates to /main-creator and stores auth on successful Creator login", async () => {
    global.fetch = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({
        userType: "creator",
        creatorId: "abc-123",
        name: "Test Creator",
        sessionId: "session-1",
      }),
    });

    render(App());

    fireEvent.change(screen.getByLabelText("Email"), {
      target: { value: "creator@example.com" },
    });
    fireEvent.change(screen.getByLabelText("Password"), {
      target: { value: "password123" },
    });
    fireEvent.click(screen.getByRole("button", { name: "Creator" }));
    fireEvent.click(screen.getByRole("button", { name: "Log in" }));

    await waitFor(() => {
      expect(screen.getByText("This is /main-creator!")).toBeInTheDocument();
    });

    const stored = JSON.parse(localStorage.getItem("habithubAuth")!);
    expect(stored.isLoggedIn).toBe(true);
    expect(stored.userType).toBe("Creator");
    expect(stored.name).toBe("Test Creator");
  });

  it("navigates to /main-member on successful Member login", async () => {
    global.fetch = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({
        userType: "member",
        memberId: "xyz-456",
        name: "Test Member",
        sessionId: "session-2",
      }),
    });

    render(App());

    fireEvent.change(screen.getByLabelText("Email"), {
      target: { value: "member@example.com" },
    });
    fireEvent.change(screen.getByLabelText("Password"), {
      target: { value: "password123" },
    });
    fireEvent.click(screen.getByRole("button", { name: "Log in" }));

    await waitFor(() => {
      expect(screen.getByText("This is /main-member!")).toBeInTheDocument();
    });
  });

  it("shows error message on 401 response", async () => {
    global.fetch = vi.fn().mockResolvedValue({
      ok: false,
      status: 401,
    });

    render(App());

    fireEvent.change(screen.getByLabelText("Email"), {
      target: { value: "test@example.com" },
    });
    fireEvent.change(screen.getByLabelText("Password"), {
      target: { value: "wrongpassword" },
    });
    fireEvent.click(screen.getByRole("button", { name: "Log in" }));

    await waitFor(() => {
      expect(screen.getByRole("alert")).toHaveTextContent(
        "Invalid email, password, or account type.",
      );
    });

    expect(screen.getByText("Welcome back. Log in to continue with HabitHub.")).toBeInTheDocument();
    expect(localStorage.getItem("habithubAuth")).toBeNull();
  });
});
