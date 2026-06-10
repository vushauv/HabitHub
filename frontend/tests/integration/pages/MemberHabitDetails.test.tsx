import { render, screen, fireEvent, waitFor } from "@testing-library/react";
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
import MemberHabitDetails from "../../../src/pages/MemberHabitDetails";
import PathDisplay from "../PathDisplay";
import { http, HttpResponse } from "msw";
import { setupServer } from "msw/node";
import { API_URL } from "../../const";

const TEAM_ID = "team-1";
const HABIT_ID = "habit-1";

const habit = {
  habitId: HABIT_ID,
  name: "Morning Walk",
  goal: "Walk before work",
  habitState: "Active",
  habitType: "Binary",
  unit: null,
  expiryDate: null,
};

const entries = [
  {
    entryId: "entry-1",
    habitId: HABIT_ID,
    memberId: "member-1",
    loggedAt: "2026-05-20T08:00:00Z",
    logDate: "2026-05-20",
    status: "Logged",
    value: null,
    notes: "Felt good",
  },
  {
    entryId: "entry-2",
    habitId: HABIT_ID,
    memberId: "member-1",
    loggedAt: "2026-05-19T08:00:00Z",
    logDate: "2026-05-19",
    status: "Skipped",
    value: null,
    notes: null,
  },
];

type ReminderFixture = {
  habitId: string;
  memberId: string;
  enabled: boolean;
  reminderTime: string | null;
};

const defaultReminder: ReminderFixture = {
  habitId: HABIT_ID,
  memberId: "member-1",
  enabled: false,
  reminderTime: null,
};

const makeHandlers = (h = habit, e = entries, r = defaultReminder) => [
  http.get(`${API_URL}/teams/${TEAM_ID}`, () =>
    HttpResponse.json({ teamId: TEAM_ID, name: "Alpha Team" }),
  ),
  http.get(`${API_URL}/habits/${HABIT_ID}`, () => HttpResponse.json(h)),
  http.get(`${API_URL}/habits/${HABIT_ID}/entries`, () => HttpResponse.json(e)),
  http.get(`${API_URL}/habits/${HABIT_ID}/my-reminder`, () =>
    HttpResponse.json(r),
  ),
];

const server = setupServer(...makeHandlers());
beforeAll(() => server.listen());
afterEach(() => server.resetHandlers());
afterAll(() => server.close());

const App = () => (
  <MemoryRouter
    initialEntries={[`/member/teams/${TEAM_ID}/habits/${HABIT_ID}/details`]}
  >
    <Routes>
      <Route
        path="member/teams/:teamId/habits/:habitId/details"
        element={<MemberHabitDetails />}
      />
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
});

it("renders habit details and team name", async () => {
  render(App());

  await waitFor(() => {
    expect(screen.getByText("Alpha Team")).toBeInTheDocument();
    expect(screen.getByText("Morning Walk")).toBeInTheDocument();
    expect(screen.getByText("Walk before work")).toBeInTheDocument();
  });
});

it("renders entries with status and notes", async () => {
  render(App());

  await waitFor(() => {
    expect(screen.getByText("Logged")).toBeInTheDocument();
    expect(screen.getAllByText("Skipped").length).toBeGreaterThan(0);
    expect(screen.getByText("Felt good")).toBeInTheDocument();
    expect(screen.getByText("No notes")).toBeInTheDocument();
  });
});

it("shows Log Habit link for active habit", async () => {
  render(App());

  await waitFor(() => {
    expect(screen.getByRole("link", { name: "Log Habit" })).toBeInTheDocument();
  });
});

it("hides Log Habit link for archived habit", async () => {
  server.use(...makeHandlers({ ...habit, habitState: "Archived" }, []));

  render(App());

  await waitFor(() => {
    expect(screen.getByText("Archived")).toBeInTheDocument();
  });

  expect(screen.queryByRole("link", { name: "Log Habit" })).not.toBeInTheDocument();
  expect(screen.getByRole("link", { name: "Leaderboard" })).toBeInTheDocument();
});

it("shows empty state when no entries exist", async () => {
  server.use(...makeHandlers(habit, []));

  render(App());

  await waitFor(() => {
    expect(screen.getByText("No progress entries")).toBeInTheDocument();
  });
});

it("shows error when habit load fails", async () => {
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

it("keeps reminder toggle disabled when no reminder time is set", async () => {
  render(App());

  await waitFor(() => {
    expect(screen.getByText("No reminder time set")).toBeInTheDocument();
  });

  expect(
    screen.getByRole("button", { name: "Enable Reminder" }),
  ).toBeDisabled();
});

it("enables a habit reminder", async () => {
  const disabledReminder = {
    ...defaultReminder,
    reminderTime: "07:30:00",
  };

  server.use(
    ...makeHandlers(habit, entries, disabledReminder),
    http.patch(`${API_URL}/habits/${HABIT_ID}/my-reminder`, async ({ request }) => {
      const data = (await request.json()) as { enabled: boolean };

      return HttpResponse.json({
        ...disabledReminder,
        enabled: data.enabled,
      });
    }),
  );

  render(App());

  await waitFor(() => {
    expect(
      screen.getByRole("button", { name: "Enable Reminder" }),
    ).toBeInTheDocument();
  });

  fireEvent.click(screen.getByRole("button", { name: "Enable Reminder" }));

  await waitFor(() => {
    expect(screen.getByText("Reminder enabled.")).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: "Disable Reminder" }),
    ).toBeInTheDocument();
  });
});
