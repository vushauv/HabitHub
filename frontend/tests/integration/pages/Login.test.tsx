import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { it, expect, vi, beforeEach, beforeAll, afterEach, afterAll } from "vitest";
import Login from "../../../src/pages/Login";
import PathDisplay from "../PathDisplay";
import { http, HttpResponse } from "msw"
import { setupServer } from "msw/node";
import { API_URL } from "../../const";

const handlers = [
  http.post(`${API_URL}/auth/login`, async ({ request }) => {
    const data = await request.json() as {email: string} | undefined;
    if (data == null || !("email" in data)) {
      return new HttpResponse(null, {status: 400});
    }
    if (data.email.includes("creator")) {
      return HttpResponse.json({
        userType: "creator",
        creatorId: "abc-123",
        name: "Test Creator",
        sessionId: "session-1",
      })
    }
    if (data.email.includes("member")) {
      return HttpResponse.json({
        userType: "member",
        memberId: "xyz-456",
        name: "Test Member",
        sessionId: "session-2",
      })
    }
    return new HttpResponse(null, {status: 401})
  })
]
const server = setupServer(...handlers);
beforeAll(() => server.listen())
afterEach(() => server.resetHandlers())
afterAll(() => server.close())

const App = () => (
  <MemoryRouter initialEntries={["/login"]}>
    <Routes>
      <Route path="login" element={<Login/>}/>
      <Route path="/*" element={<PathDisplay/>}/>
    </Routes>
  </MemoryRouter>
)

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
  render(App());

  fireEvent.change(screen.getByLabelText("Email"), {
    target: { value: "invalid@example.com" },
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
