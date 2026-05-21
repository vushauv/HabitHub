import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { it, expect, vi, beforeEach, beforeAll, afterEach, afterAll } from "vitest";
import ChangeEmail from "../../../src/pages/ChangeEmail";
import PathDisplay from "../PathDisplay";
import { http, HttpResponse } from "msw"
import { setupServer } from "msw/node";
import { API_URL } from "../../const";

const handlers = [
  http.post(`${API_URL}/auth/change-email`, async ({ request }) => {
    const data = await request.json() as {password: string} | undefined;
    if (data == null || !("password" in data)) {
      return new HttpResponse(null, {status: 400});
    }
    if (!data.password.includes("wrong")) {
      return new HttpResponse(null, {status: 200})
    }
    return HttpResponse.json({"error":"invalid-credentials","message":"Invalid credentials."}, {status: 401})
  })
]
const server = setupServer(...handlers);
beforeAll(() => server.listen())
afterEach(() => server.resetHandlers())
afterAll(() => server.close())

const App = () => (
  <MemoryRouter initialEntries={["/settings/change-email"]}>
    <Routes>
      <Route path="settings/change-email" element={<ChangeEmail/>}/>
      <Route path="/*" element={<PathDisplay/>}/>
    </Routes>
  </MemoryRouter>
)

beforeEach(() => {
  localStorage.clear();
  localStorage.setItem(
    "habithubAuth",
    JSON.stringify({ isLoggedIn: true, sessionId: "1234", userType: "Creator" }),
  );
  vi.restoreAllMocks();
});

it("renders email and password fields", () => {
  render(App());

  expect(screen.getByLabelText("New email")).toBeInTheDocument();
  expect(screen.getByLabelText("Password")).toBeInTheDocument();
});

it("submit button is disabled when form is empty", () => {
  render(App());

  expect(screen.getByRole("button", { name: "Update email" })).toBeDisabled();
});

it("shows validation errors on blur with empty fields", async () => {
  render(App());

  fireEvent.blur(screen.getByLabelText("New email"));
  fireEvent.blur(screen.getByLabelText("Password"));

  await waitFor(() => {
    expect(screen.getByText("Enter a valid email address.")).toBeInTheDocument();
    expect(screen.getByText("Current password is required.")).toBeInTheDocument();
  });
});

it("shows success message on 200 response", async () => {
  render(App());

  fireEvent.change(screen.getByLabelText("New email"), {
    target: { value: "member@example.com" },
  });
  fireEvent.change(screen.getByLabelText("Password"), {
    target: { value: "password123" },
  });
  fireEvent.submit(screen.getByLabelText("New email"));

  await waitFor(() => {
    expect(screen.getByText("Email changed successfully.", {exact: false})).toBeInTheDocument();
  });
});

it("shows error message on 401 response", async () => {
  render(App());

  fireEvent.change(screen.getByLabelText("New email"), {
    target: { value: "invalid@example.com" },
  });
  fireEvent.change(screen.getByLabelText("Password"), {
    target: { value: "wrongpassword" },
  });
  fireEvent.submit(screen.getByLabelText("New email"));

  await waitFor(() => {
    expect(screen.getByRole("alert")).toHaveTextContent(
      "Your password is incorrect.",
    );
  });
});
