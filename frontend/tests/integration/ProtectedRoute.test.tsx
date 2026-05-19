import { render, screen, waitFor } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { it, expect, beforeEach, afterAll, beforeAll, afterEach } from "vitest";
import ProtectedRoute from "../../src/ProtectedRoute";
import { http, HttpResponse } from "msw"
import { setupServer } from "msw/node";
import { API_URL } from "../const";

const handlers = [
  http.get(`${API_URL}/auth/me`, async ({ request }) => {
    const sessionId = request.headers.get("X-Session-Id");
    if(sessionId == "creator") {
      return HttpResponse.json({
        id: "1234",
        name: "John Creator",
        email: "john@example.com",
        userType: 0,
        timezone: "UTC"
      })
    } else if(sessionId == "member") {
      return HttpResponse.json({
        id: "1235",
        name: "John Member",
        email: "john@example.com",
        userType: 1,
        timezone: "UTC"
      })
    } else {
      return HttpResponse.json({ error: "auth-required", message: "Authentication is required." }, {status: 401})
    }
  })
]
const server = setupServer(...handlers);
beforeAll(() => server.listen())
afterEach(() => server.resetHandlers())
afterAll(() => server.close())

function renderApp(initialPath: string) {
  render(
    <MemoryRouter initialEntries={[initialPath]}>
      <Routes>
        <Route path="/login" element={<div>Login Page</div>} />
        <Route element={<ProtectedRoute allowedUserType="Creator" />}>
          <Route path="/creator" element={<div>Creator Dashboard</div>} />
        </Route>
        <Route element={<ProtectedRoute allowedUserType="Member" />}>
          <Route path="/member" element={<div>Member Dashboard</div>} />
        </Route>
      </Routes>
    </MemoryRouter>,
  );
}

beforeEach(() => {
  localStorage.clear();
});
afterAll(() => {
  localStorage.clear();
});

it("redirects to /login when not authenticated", () => {
  renderApp("/creator");

  expect(screen.getByText("Login Page")).toBeInTheDocument();
});

it("redirects to /login when sessionId is invalid", async () => {
  localStorage.setItem(
    "habithubAuth",
    JSON.stringify({ sessionId: "this-is-invalid-trust-me-im-a-dolphin" }),
  );

  renderApp("/creator");

  await waitFor(() => {
    expect(screen.getByText("Login Page")).toBeInTheDocument();
  });
});

it("renders Creator Dashboard when authenticated as Creator", async () => {
  localStorage.setItem(
    "habithubAuth",
    JSON.stringify({ sessionId: "creator" }),
  );

  renderApp("/creator");

  await waitFor(() => {
    expect(screen.getByText("Creator Dashboard")).toBeInTheDocument();
  });
});

it("renders Member Dashboard when authenticated as Member", async () => {
  localStorage.setItem(
    "habithubAuth",
    JSON.stringify({ sessionId: "member" }),
  );

  renderApp("/member");

  await waitFor(() => {
    expect(screen.getByText("Member Dashboard")).toBeInTheDocument();
  });
});

// TODO: Fix
it.skip("redirects Creator to /creator when accessing Member route", () => {
  localStorage.setItem(
    "habithubAuth",
    JSON.stringify({ sessionId: "creator" }),
  );

  renderApp("/member");

  await waitFor(() => {
    expect(screen.getByText("Creator Dashboard")).toBeInTheDocument();
  });
});

it("redirects to /login and clears storage when localStorage contains corrupted data", () => {
  localStorage.setItem("habithubAuth", "not-valid-json{{{");

  renderApp("/creator");

  expect(screen.getByText("Login Page")).toBeInTheDocument();
  expect(localStorage.getItem("habithubAuth")).toBeNull();
});
