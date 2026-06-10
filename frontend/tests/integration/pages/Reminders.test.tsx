import { render, screen, waitFor } from "@testing-library/react";
import { MemoryRouter, Outlet, Route, Routes } from "react-router-dom";
import {
  it,
  expect,
  vi,
  beforeEach,
  beforeAll,
  afterEach,
  afterAll,
} from "vitest";
import Reminders from "../../../src/pages/Reminders";
import PathDisplay from "../PathDisplay";
import { http, HttpResponse } from "msw";
import { setupServer } from "msw/node";
import { API_URL } from "../../const";

const TEAM_ID = "team-1";
const HABIT_ID = "habit-1";

const currentUser = {
  id: "member-1",
  name: "Alex Member",
  email: "alex@example.com",
  userType: "Member",
  timezone: "UTC",
};

const habit = {
  habitId: HABIT_ID,
  name: "Morning Walk",
  goal: "Walk before work",
  habitState: "Active",
  habitType: "Binary",
  unit: null,
  expiryDate: null,
};

const reminderAlerts = [
  {
    notificationId: "reminder-1",
    content: 'Reminder: you have not logged "Morning Walk" for "Alpha Team" today.',
    createdAt: "2026-05-20T08:00:00Z",
    status: "Unread",
    type: "Reminder",
  },
];

const pendingToday = {
  status: "Pending",
  entry: null,
};

const loggedToday = {
  status: "Logged",
  entry: null,
};

const makeHandlers = (
  reminders = reminderAlerts,
  todayStatus = pendingToday,
) => [
  http.get(`${API_URL}/notifications`, () => HttpResponse.json(reminders)),
  http.get(`${API_URL}/teams`, () =>
    HttpResponse.json([{ teamId: TEAM_ID, name: "Alpha Team" }]),
  ),
  http.get(`${API_URL}/teams/${TEAM_ID}/habits`, ({ request }) => {
    const url = new URL(request.url);
    const state = url.searchParams.get("state");

    return HttpResponse.json(state === "Archived" ? [] : [habit]);
  }),
  http.get(`${API_URL}/habits/${HABIT_ID}/entries/today`, () =>
    HttpResponse.json(todayStatus),
  ),
];

const server = setupServer(...makeHandlers());
beforeAll(() => server.listen());
afterEach(() => server.resetHandlers());
afterAll(() => server.close());

const App = () => (
  <MemoryRouter initialEntries={["/reminders"]}>
    <Routes>
      <Route element={<Outlet context={currentUser} />}>
        <Route path="reminders" element={<Reminders />} />
      </Route>
      <Route path="/*" element={<PathDisplay />} />
    </Routes>
  </MemoryRouter>
);

beforeEach(() => {
  localStorage.clear();
  localStorage.setItem(
    "habithubAuth",
    JSON.stringify({ sessionId: "member-session" }),
  );
  vi.restoreAllMocks();
  server.resetHandlers(...makeHandlers());
});

it.skip("renders unread reminders with team, habit and context", async () => {
  render(App());

  await waitFor(() => {
    expect(screen.getByText("Alpha Team")).toBeInTheDocument();
    expect(screen.getByText("Morning Walk")).toBeInTheDocument();
    expect(screen.getByText("Walk before work")).toBeInTheDocument();
    expect(screen.getByText("New")).toBeInTheDocument();
  });
});

it.skip("hides reminders for habits already logged today", async () => {
  server.use(...makeHandlers(reminderAlerts, loggedToday));

  render(App());

  await waitFor(() => {
    expect(screen.getByText("No unread reminders found")).toBeInTheDocument();
  });

  expect(screen.queryByText("Morning Walk")).not.toBeInTheDocument();
});

it.skip("shows empty state when there are no unread reminders", async () => {
  server.use(...makeHandlers([]));

  render(App());

  await waitFor(() => {
    expect(screen.getByText("No unread reminders found")).toBeInTheDocument();
  });
});

it.skip("shows error when reminders fail to load", async () => {
  server.use(
    http.get(`${API_URL}/notifications`, () =>
      HttpResponse.json(
        { error: "internal-server-error", message: "Reminder alerts failed." },
        { status: 500 },
      ),
    ),
  );

  render(App());

  await waitFor(() => {
    expect(screen.getByRole("alert")).toHaveTextContent(
      "Reminder alerts failed.",
    );
  });
});
