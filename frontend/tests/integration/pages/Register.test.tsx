import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { it, expect, vi, beforeEach, beforeAll, afterEach, afterAll } from "vitest";
import Register from "../../../src/pages/Register";
import PathDisplay from "../PathDisplay";
import { http, HttpResponse } from "msw"
import { setupServer } from "msw/node";
import { API_URL } from "../../const";

const handlers = [
  http.post(`${API_URL}/auth/register`, async ({ request }) => {
    const data = await request.json() as {email: string} | undefined;
    if (data == null || !("email" in data) || data.email.includes("400")) {
      return new HttpResponse(null, {status: 400});
    }
    if (data.email.includes("creator")) {
      return HttpResponse.json({
        user: {
          userType: 0,
          id: "abc-123",
          name: "Test Creator",
        },
        sessionId: "session-1",
      })
    }
    if (data.email.includes("member")) {
      return HttpResponse.json({
        user: {
          userType: 1,
          id: "xyz-456",
          name: "Test Member",
        },
        sessionId: "session-2",
      })
    }
    return new HttpResponse(null, {status: 500}) // Case shouldn't be used in tests
  })
]
const server = setupServer(...handlers);
beforeAll(() => server.listen())
afterEach(() => server.resetHandlers())
afterAll(() => server.close())

const App = () => (
  <MemoryRouter initialEntries={["/register"]}>
    <Routes>
      <Route path="register" element={<Register />}/>
      <Route path="/*" element={<PathDisplay/>}/>
    </Routes>
  </MemoryRouter>
)

beforeEach(() => {
  localStorage.clear();
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
    expect(screen.getByText("Enter a valid email address.")).toBeInTheDocument();
    expect(screen.getByText("Password is required.")).toBeInTheDocument();
  });
});

// TODO: Fix
it.skip("navigates to /main-creator and stores auth on successful Creator register", async () => {
  render(App());

  fireEvent.change(screen.getByLabelText("Name"), {
    target: { value: "Test Creator" },
  });
  fireEvent.change(screen.getByLabelText("Email"), {
    target: { value: "creator@example.com" },
  });
  fireEvent.change(screen.getByLabelText("Password"), {
    target: { value: "password123" },
  });
  fireEvent.click(screen.getByRole("button", { name: "Creator" }));
  fireEvent.submit(screen.getByLabelText("Email"));

  await waitFor(() => {
    expect(screen.getByText("This is /main-creator!")).toBeInTheDocument();
  });

  const stored = JSON.parse(localStorage.getItem("habithubAuth")!);
  expect(stored.isLoggedIn).toBe(true);
  expect(stored.userType).toBe("Creator");
  expect(stored.name).toBe("Test Creator");
});

// TODO: Fix
it.skip("navigates to /main-member and stores auth on successful Member register", async () => {
  render(App());

  fireEvent.change(screen.getByLabelText("Name"), {
    target: { value: "Test Member" },
  });
  fireEvent.change(screen.getByLabelText("Email"), {
    target: { value: "member@example.com" },
  });
  fireEvent.change(screen.getByLabelText("Password"), {
    target: { value: "password123" },
  });
  fireEvent.click(screen.getByRole("button", { name: "Member" }));
  fireEvent.submit(screen.getByLabelText("Email"));

  await waitFor(() => {
    expect(screen.getByText("This is /main-member!")).toBeInTheDocument();
  });

  const stored = JSON.parse(localStorage.getItem("habithubAuth")!);
  expect(stored.isLoggedIn).toBe(true);
  expect(stored.userType).toBe("Member");
  expect(stored.name).toBe("Test Member");
});

it("shows error message on 400 response", async () => {
  render(App());

  fireEvent.change(screen.getByLabelText("Name"), {
    target: { value: "Creator" },
  });
  fireEvent.change(screen.getByLabelText("Email"), {
    target: { value: "400@example.com" },
  });
  fireEvent.change(screen.getByLabelText("Password"), {
    target: { value: "password123" },
  });
  fireEvent.click(screen.getByRole("button", { name: "Creator" }));
  fireEvent.submit(screen.getByLabelText("Name"))

  await waitFor(() => {
    expect(screen.getByRole("alert")).toHaveTextContent(
      "Please check the form fields and try again.",
    );
  });

  expect(screen.getByText("Create your account")).toBeInTheDocument();
});