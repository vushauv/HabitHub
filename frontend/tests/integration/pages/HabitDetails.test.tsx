import { render, screen, waitFor } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import {
  it,
  expect,
  vi,
  beforeEach,
  beforeAll,
  afterEach,
  afterAll,
} from "vitest";
import HabitDetails from "../../../src/pages/HabitDetails";
import PathDisplay from "../PathDisplay";
import { http, HttpResponse } from "msw";
import { setupServer } from "msw/node";
import { API_URL } from "../../const";

const TEAM_ID = "team-1";
const HABIT_ID = "habit-1";

type HabitFixture = {
  habitId: string;
  name: string;
  goal: string | null;
  habitState: string;
  habitType: string;
  unit: string | null;
  expiryDate: string | null;
};

const activeHabit: HabitFixture = {
  habitId: HABIT_ID,
  name: "Morning Walk",
  goal: "Walk before work",
  habitState: "Active",
  habitType: "Binary",
  unit: null,
  expiryDate: null,
};

const archivedHabit = { ...activeHabit, habitState: "Archived" };

const makeHandlers = (habit = activeHabit) => [
  http.get(`${API_URL}/teams/${TEAM_ID}`, () =>
    HttpResponse.json({ teamId: TEAM_ID, name: "Alpha Team" }),
  ),
  http.get(`${API_URL}/habits/${HABIT_ID}`, () => HttpResponse.json(habit)),
];

const server = setupServer(...makeHandlers());
beforeAll(() => server.listen());
afterEach(() => server.resetHandlers());
afterAll(() => server.close());

const App = () => (
  <MemoryRouter
    initialEntries={[`/creator/teams/${TEAM_ID}/habits/${HABIT_ID}/details`]}
  >
    <Routes>
      <Route
        path="creator/teams/:teamId/habits/:habitId/details"
        element={<HabitDetails />}
      />
      <Route path="/*" element={<PathDisplay />} />
    </Routes>
  </MemoryRouter>
);

beforeEach(() => {
  localStorage.clear();
  localStorage.setItem(
    "habithubAuth",
    JSON.stringify({ sessionId: "creator-session" }),
  );
  vi.restoreAllMocks();
  server.resetHandlers(...makeHandlers());
});

it("renders habit details after load", async () => {
  render(App());

  await waitFor(() => {
    expect(screen.getByText("Morning Walk")).toBeInTheDocument();
    expect(screen.getByText("Walk before work")).toBeInTheDocument();
    expect(screen.getByText("Binary")).toBeInTheDocument();
    expect(screen.getByText("Active")).toBeInTheDocument();
  });
});

it("displays team name after load", async () => {
  render(App());

  await waitFor(() => {
    expect(screen.getByText("Alpha Team")).toBeInTheDocument();
  });
});

it("shows Edit Habit link for active habit", async () => {
  render(App());

  await waitFor(() => {
    expect(screen.getByRole("link", { name: "Edit Habit" })).toBeInTheDocument();
  });
});

it("hides Edit Habit link for archived habit", async () => {
  server.use(...makeHandlers(archivedHabit));

  render(App());

  await waitFor(() => {
    expect(screen.getByText("Archived")).toBeInTheDocument();
  });

  expect(screen.queryByRole("link", { name: "Edit Habit" })).not.toBeInTheDocument();
  expect(screen.getByRole("link", { name: "Leaderboard" })).toBeInTheDocument();
});

it("shows error when load fails", async () => {
  server.use(
    http.get(`${API_URL}/habits/${HABIT_ID}`, () =>
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

it("shows 'No goal' when goal is null", async () => {
  server.use(...makeHandlers({ ...activeHabit, goal: null }));

  render(App());

  await waitFor(() => {
    expect(screen.getByText("No goal")).toBeInTheDocument();
  });
});
