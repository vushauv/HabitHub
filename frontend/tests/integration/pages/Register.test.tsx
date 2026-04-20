import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { describe, it, expect, vi, beforeEach } from "vitest";
import Register from "../../../src/pages/Register";
import PathDisplay from "../PathDisplay";

const App = () => (
  <MemoryRouter initialEntries={["/register"]}>
    <Routes>
      <Route path="register" element={<Register />}/>
      <Route path="/*" element={<PathDisplay/>}/>
    </Routes>
  </MemoryRouter>
)

describe("Register", () => {
  beforeEach(() => {
    vi.restoreAllMocks();
  });

  it("renders name, email, password, timezone and account type fields", () => {
    render(App());

    expect(screen.getByLabelText("Name")).toBeInTheDocument();
    expect(screen.getByLabelText("Email")).toBeInTheDocument();
    expect(screen.getByLabelText("Password")).toBeInTheDocument();
    expect(screen.getByLabelText("Timezone")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Creator" })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Member" })).toBeInTheDocument();
  });

  it("submit button is disabled when form is empty", () => {
    render(App());

    expect(screen.getByRole("button", { name: "Create account" })).toBeDisabled();
  });

  it("shows validation errors on blur with empty fields", async () => {
    render(App());

    fireEvent.blur(screen.getByLabelText("Name"));
    fireEvent.blur(screen.getByLabelText("Email"));
    fireEvent.blur(screen.getByLabelText("Password"));

    await waitFor(() => {
      expect(screen.getByText("Name is required.")).toBeInTheDocument();
      expect(screen.getByText("Email is required.")).toBeInTheDocument();
      expect(screen.getByText("Password is required.")).toBeInTheDocument();
    });
  });

  it("navigates to /login on successful register", async () => {
    global.fetch = vi.fn().mockResolvedValue({
      ok: true
    });

    render(App());

    fireEvent.change(screen.getByLabelText("Name"), {
      target: { value: "Creator" },
    });
    fireEvent.change(screen.getByLabelText("Email"), {
      target: { value: "creator@example.com" },
    });
    fireEvent.change(screen.getByLabelText("Password"), {
      target: { value: "password123" },
    });
    fireEvent.click(screen.getByRole("button", { name: "Creator" }));
    fireEvent.click(screen.getByRole("button", { name: "Create account" }));

    await waitFor(() => {
      expect(screen.getByText("This is /login!")).toBeInTheDocument();
    });
  });

  it("shows error message on 400 response", async () => {
    global.fetch = vi.fn().mockResolvedValue({
      ok: false,
      status: 400,
    });

    render(App());

    fireEvent.change(screen.getByLabelText("Name"), {
      target: { value: "Creator" },
    });
    fireEvent.change(screen.getByLabelText("Email"), {
      target: { value: "creator@example.com" },
    });
    fireEvent.change(screen.getByLabelText("Password"), {
      target: { value: "password123" },
    });
    fireEvent.click(screen.getByRole("button", { name: "Creator" }));
    fireEvent.click(screen.getByRole("button", { name: "Create account" }));

    await waitFor(() => {
      expect(screen.getByRole("alert")).toHaveTextContent(
        "Please check the form fields and try again.",
      );
    });

    expect(screen.getByText("Create your account")).toBeInTheDocument();
  });
});
