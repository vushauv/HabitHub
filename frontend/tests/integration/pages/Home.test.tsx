import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { it, expect, vi, beforeEach, beforeAll, afterEach, afterAll } from "vitest";
import Home from "../../../src/pages/Home";
import PathDisplay from "../PathDisplay";
import { http, HttpResponse } from "msw"
import { setupServer } from "msw/node";
import { API_URL } from "../../const";

const logoutMockHandler = vi.fn();
const handlers = [
  http.delete(`${API_URL}/auth/logout`, async ({ request }) => {
    const sessionId = request.headers.get("X-Session-Id");
    if(sessionId == null)
      return new HttpResponse(null, { status: 400 });
    logoutMockHandler(sessionId);
    return new HttpResponse(null, { status: 204 });
  })
]
const server = setupServer(...handlers);
beforeAll(() => server.listen())
afterEach(() => server.resetHandlers())
afterAll(() => server.close())

const App = () => (
  <MemoryRouter initialEntries={["/"]}>
    <Routes>
      <Route index element={<Home/>}/>
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
  logoutMockHandler.mockReset();
  vi.restoreAllMocks();
});

it("logs out user correctly", () => {
  render(App());

  expect(screen.getByRole("button", { name: "Log out" })).toBeInTheDocument();
  fireEvent.click(screen.getByRole("button", { name: "Log out" }));
  waitFor(() => {
    expect(logoutMockHandler).toHaveBeenCalledExactlyOnceWith("1234");
    expect(screen.getByText("Get started")).toBeInTheDocument();
    expect(screen.getByText("Log in")).toBeInTheDocument();
    expect(localStorage.getItem("habithubAuth")).toBeNull();
  });
});