import { render, screen, waitFor } from "@testing-library/react";
import { MemoryRouter, Outlet, Route, Routes } from "react-router-dom";
import { it, expect, vi, beforeEach, beforeAll, afterEach, afterAll } from "vitest";
import PathDisplay from "../PathDisplay";
import { http, HttpResponse } from "msw"
import { setupServer } from "msw/node";
import { API_URL } from "../../const";
import Notifications from "../../../src/pages/Notifications";

const USER = {
  id: "tak",
  name: "Dave",
  email: "dave@g.com",
  userType: "Member",
  timezone: "Europe/Warsaw"
}

const handlers = [
  http.get(`${API_URL}/notifications`, async () => {
    return HttpResponse.json([
      {
        notificationId: "john1",
        content: "John Member The 1st",
        createdAt: "2026-06-08T06:00:59.116992Z",
        status: "Unread",
        type: "Reminder",
      },
      {
        notificationId: "john2",
        content: "John Member The 2nd",
        createdAt: "2026-06-08T08:00:59.116992Z",
        status: "Unread",
        type: "Reminder",
      }
    ], {status: 200})
  }),
  http.get(`${API_URL}/auth/me`, () => HttpResponse.json()),
]
const server = setupServer(...handlers);
beforeAll(() => server.listen())
afterEach(() => server.resetHandlers())
afterAll(() => server.close())

const App = () => (
  <MemoryRouter initialEntries={["/notifications"]}>
    <Routes>
      <Route element={<Outlet context={USER} />}>
        <Route path="notifications" element={<Notifications/> }/>
      </Route>
      <Route path="/*" element={<PathDisplay/>}/>
    </Routes>
  </MemoryRouter>
)
beforeEach(() => {
  localStorage.clear();
  localStorage.setItem(
    "habithubAuth",
    JSON.stringify({ sessionId: "1234" }),
  );
  vi.restoreAllMocks();
});

it("renders notifications list correctly", async () => {
  render(App());

  await waitFor(() => {
    expect(screen.getByText("2")).toBeInTheDocument();
    expect(screen.getByText("Jun 8, 2026, 6:00 AM")).toBeInTheDocument();
    expect(screen.getByText("John Member The 1st")).toBeInTheDocument();
    expect(screen.getByText("Jun 8, 2026, 8:00 AM")).toBeInTheDocument();
    expect(screen.getByText("John Member The 2nd")).toBeInTheDocument();
  });
});