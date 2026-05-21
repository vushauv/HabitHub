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
import HabitLeaderboard from "../../../src/pages/HabitLeaderboard";
import PathDisplay from "../PathDisplay";
import { http, HttpResponse } from "msw";
import { setupServer } from "msw/node";
import { API_URL } from "../../const";

const TEAM_ID = "team-1";
const HABIT_ID = "habit-1";

const quantitativeHabit = {
  habitId: HABIT_ID,
  name: "Water Intake",
  goal: null,
  habitState: "Active",
  habitType: "Quantitative",
  unit: "Cups",
  expiryDate: null,
};

const leaderboard = [
  {
    memberId: "member-1",
    memberName: "Alice",
    totalValue: 24,
    loggedCount: 6,
    rank: 1,
  },
  {
    memberId: "member-2",
    memberName: "Bob",
    totalValue: 12,
    loggedCount: 3,
    rank: 2,
  },
];

const currentUser = {
  id: "user-1",
  name: "Alice",
  email: "alice@example.com",
  userType: "Member",
  timezone: null,
};

const makeHandlers = (habit = quantitativeHabit, board = leaderboard) => [
  http.get(`${API_URL}/teams/${TEAM_ID}`, () =>
    HttpResponse.json({ teamId: TEAM_ID, name: "Alpha Team" }),
  ),
  http.get(`${API_URL}/habits/${HABIT_ID}`, () => HttpResponse.json(habit)),
  http.get(`${API_URL}/habits/${HABIT_ID}/leaderboard`, () =>
    HttpResponse.json(board),
  ),
];

const server = setupServer(...makeHandlers());
beforeAll(() => server.listen());
afterEach(() => server.resetHandlers());
afterAll(() => server.close());

const App = () => (
  <MemoryRouter
    initialEntries={[`/teams/${TEAM_ID}/habits/${HABIT_ID}/leaderboard`]}
  >
    <Routes>
      <Route element={<Outlet context={currentUser} />}>
        <Route
          path="teams/:teamId/habits/:habitId/leaderboard"
          element={<HabitLeaderboard />}
        />
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

it("renders leaderboard rows ranked", async () => {
  render(App());

  await waitFor(() => {
    expect(screen.getByText("Alice")).toBeInTheDocument();
    expect(screen.getByText("Bob")).toBeInTheDocument();
    expect(screen.getByText("#1")).toBeInTheDocument();
    expect(screen.getByText("#2")).toBeInTheDocument();
  });
});

it("renders quantitative totals with unit", async () => {
  render(App());

  await waitFor(() => {
    expect(screen.getByText("24 Cups")).toBeInTheDocument();
    expect(screen.getByText("12 Cups")).toBeInTheDocument();
  });
});

it("renders team and habit name in header", async () => {
  render(App());

  await waitFor(() => {
    expect(screen.getByText("Alpha Team · Water Intake")).toBeInTheDocument();
  });
});

it("shows empty state when leaderboard has no entries", async () => {
  server.use(...makeHandlers(quantitativeHabit, []));

  render(App());

  await waitFor(() => {
    expect(screen.getByText("No leaderboard entries")).toBeInTheDocument();
  });
});

it("shows error when load fails", async () => {
  server.use(
    http.get(`${API_URL}/habits/${HABIT_ID}/leaderboard`, () =>
      HttpResponse.json(
        { error: "habit-not-found", message: "Habit could not be found." },
        { status: 404 },
      ),
    ),
  );

  render(App());

  await waitFor(() => {
    expect(screen.getByRole("alert")).toHaveTextContent(
      "Habit could not be found.",
    );
  });
});
