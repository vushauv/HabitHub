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
import MemberLogHabit from "../../../src/pages/MemberLogHabit";
import PathDisplay from "../PathDisplay";
import { http, HttpResponse } from "msw";
import { setupServer } from "msw/node";
import { API_URL } from "../../const";

const TEAM_ID = "team-1";
const HABIT_ID = "habit-1";
const ENTRY_ID = "entry-1";

const binaryHabit = {
  habitId: HABIT_ID,
  name: "Morning Walk",
  goal: "Walk before work",
  habitState: "Active",
  habitType: "Binary",
  unit: null,
  expiryDate: null,
};

const quantitativeHabit = {
  habitId: HABIT_ID,
  name: "Water Intake",
  goal: null,
  habitState: "Active",
  habitType: "Quantitative",
  unit: "Cups",
  expiryDate: null,
};

const pendingStatus = { status: "Pending", entry: null };

const loggedStatus = {
  status: "Logged",
  entry: {
    entryId: ENTRY_ID,
    habitId: HABIT_ID,
    memberId: "member-1",
    loggedAt: "2026-05-21T08:00:00Z",
    logDate: "2026-05-21",
    status: "Logged",
    value: null,
    notes: null,
  },
};

const makeHandlers = (habit = binaryHabit, todayStatus = pendingStatus) => [
  http.get(`${API_URL}/teams/${TEAM_ID}`, () =>
    HttpResponse.json({ teamId: TEAM_ID, name: "Alpha Team" }),
  ),
  http.get(`${API_URL}/habits/${HABIT_ID}`, () => HttpResponse.json(habit)),
  http.get(`${API_URL}/habits/${HABIT_ID}/entries/today`, () =>
    HttpResponse.json(todayStatus),
  ),
  http.post(`${API_URL}/habits/${HABIT_ID}/entries`, () =>
    HttpResponse.json(
      {
        entryId: ENTRY_ID,
        habitId: HABIT_ID,
        memberId: "member-1",
        loggedAt: "2026-05-21T08:00:00Z",
        logDate: "2026-05-21",
        status: "Logged",
        value: null,
        notes: null,
      },
      { status: 201 },
    ),
  ),
  http.delete(`${API_URL}/habits/${HABIT_ID}/entries/${ENTRY_ID}`, () =>
    new HttpResponse(null, { status: 204 }),
  ),
];

const server = setupServer(...makeHandlers());
beforeAll(() => server.listen());
afterEach(() => server.resetHandlers());
afterAll(() => server.close());

const App = () => (
  <MemoryRouter
    initialEntries={[`/member/teams/${TEAM_ID}/habits/${HABIT_ID}/log`]}
  >
    <Routes>
      <Route
        path="member/teams/:teamId/habits/:habitId/log"
        element={<MemberLogHabit />}
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
  server.resetHandlers(...makeHandlers());
});

it("renders team and habit name after load", async () => {
  render(App());

  await waitFor(() => {
    expect(screen.getByText("Alpha Team · Morning Walk")).toBeInTheDocument();
  });
});

it("shows Mark Completed and Skip Today for pending binary habit", async () => {
  render(App());

  await waitFor(() => {
    expect(
      screen.getByRole("button", { name: "Mark Completed" }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: "Skip Today" }),
    ).toBeInTheDocument();
  });
});

it("shows Value input for pending quantitative habit", async () => {
  server.use(...makeHandlers(quantitativeHabit));

  render(App());

  await waitFor(() => {
    expect(screen.getByLabelText("Value")).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: "Log Progress" }),
    ).toBeInTheDocument();
  });
});

it("logs binary habit progress and shows success message", async () => {
  render(App());

  await waitFor(() => {
    expect(
      screen.getByRole("button", { name: "Mark Completed" }),
    ).not.toBeDisabled();
  });

  server.use(
    http.get(`${API_URL}/habits/${HABIT_ID}/entries/today`, () =>
      HttpResponse.json(loggedStatus),
    ),
  );

  fireEvent.click(screen.getByRole("button", { name: "Mark Completed" }));

  expect(
    await screen.findByText("Progress logged.", {}, { timeout: 4000 }),
  ).toBeInTheDocument();
});

it("skips today and shows success message", async () => {
  render(App());

  await waitFor(() => {
    expect(
      screen.getByRole("button", { name: "Skip Today" }),
    ).toBeInTheDocument();
  });

  server.use(
    http.post(`${API_URL}/habits/${HABIT_ID}/entries`, () =>
      HttpResponse.json(
        {
          entryId: ENTRY_ID,
          habitId: HABIT_ID,
          memberId: "member-1",
          loggedAt: "2026-05-21T08:00:00Z",
          logDate: "2026-05-21",
          status: "Skipped",
          value: null,
          notes: null,
        },
        { status: 201 },
      ),
    ),
    http.get(`${API_URL}/habits/${HABIT_ID}/entries/today`, () =>
      HttpResponse.json({ status: "Skipped", entry: { ...loggedStatus.entry, status: "Skipped" } }),
    ),
  );

  fireEvent.click(screen.getByRole("button", { name: "Skip Today" }));

  expect(
    await screen.findByText("Progress skipped.", {}, { timeout: 4000 }),
  ).toBeInTheDocument();
});

it("shows Undo Log button when already logged today", async () => {
  server.use(...makeHandlers(binaryHabit, loggedStatus));

  render(App());

  await waitFor(() => {
    expect(
      screen.getByRole("button", { name: "Undo Log" }),
    ).toBeInTheDocument();
  });
});

it("undoes today's log and shows success message", async () => {
  vi.spyOn(window, "confirm").mockReturnValue(true);
  server.use(...makeHandlers(binaryHabit, loggedStatus));

  render(App());

  await waitFor(() => {
    expect(
      screen.getByRole("button", { name: "Undo Log" }),
    ).toBeInTheDocument();
  });

  server.use(
    http.get(`${API_URL}/habits/${HABIT_ID}/entries/today`, () =>
      HttpResponse.json(pendingStatus),
    ),
  );

  fireEvent.click(screen.getByRole("button", { name: "Undo Log" }));

  expect(
    await screen.findByText(
      "Today's progress log was undone.",
      {},
      { timeout: 4000 },
    ),
  ).toBeInTheDocument();
});

it("shows error on failed log", async () => {
  server.use(
    http.post(`${API_URL}/habits/${HABIT_ID}/entries`, () =>
      HttpResponse.json(
        { error: "log-already-exists", message: "Progress was already logged for today." },
        { status: 409 },
      ),
    ),
  );

  render(App());

  await waitFor(() => {
    expect(
      screen.getByRole("button", { name: "Mark Completed" }),
    ).toBeInTheDocument();
  });

  fireEvent.click(screen.getByRole("button", { name: "Mark Completed" }));

  const alert = await screen.findByRole("alert", {}, { timeout: 4000 });
  expect(alert).toHaveTextContent("Progress was already logged for today.");
});
