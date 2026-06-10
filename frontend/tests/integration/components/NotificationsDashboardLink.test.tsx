import { render, fireEvent, screen, waitFor } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { it, expect, beforeEach, beforeAll, afterEach, afterAll } from "vitest";
import PathDisplay from "../PathDisplay";
import { http, HttpResponse } from "msw"
import { setupServer } from "msw/node";
import { API_URL } from "../../const";
import NotificationsDashboardLink from "../../../src/components/NotificationsDashboardLink";

const handlers = [
  http.get(`${API_URL}/notifications/unread-count`, async () => {
    return HttpResponse.json({
      count: 67
    }, {status: 200})
  })
]
const server = setupServer(...handlers);
beforeAll(() => server.listen())
afterEach(() => server.resetHandlers())
afterAll(() => server.close())

const App = () => (
  <MemoryRouter initialEntries={["/test"]}>
    <Routes>
      <Route path="/test" element={<NotificationsDashboardLink/>}/>
      <Route path="/*" element={<PathDisplay/>}/>
    </Routes>
  </MemoryRouter>
)

beforeEach(() => {
  localStorage.clear();
  localStorage.setItem(
    "habithubAuth",
    JSON.stringify({ sessionId: "some-session" }),
  );
});

it("displays notification count on screen", async () => {
  render(App());

  await waitFor(() => {
    expect(screen.getByText("67")).toBeInTheDocument();
  });
});

it("navigates to notifications page on click", async () => {
  render(App());

  await waitFor(() => {
    expect(screen.getByText("Notifications")).toBeInTheDocument();
    fireEvent.click(screen.getByText("Notifications"));
    expect(screen.getByText("This is /notifications!")).toBeInTheDocument();
  });
});