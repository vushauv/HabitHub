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
import MemberHabits from "../../../src/pages/MemberHabits";
import PathDisplay from "../PathDisplay";
import { http, HttpResponse } from "msw";
import { setupServer } from "msw/node";
import { API_URL } from "../../const";

const TEAM_ID = "team-1";

const activeHabits = [
  {
    habitId: "habit-1",
    name: "Morning Walk",
    goal: "Walk before work",
    habitState: "Active",
    habitType: "Binary",
    unit: null,
    expiryDate: null,
  },
  {
    habitId: "habit-2",
    name: "Water Intake",
    goal: null,
    habitState: "Active",
    habitType: "Quantitative",
    unit: "Cups",
    expiryDate: null,
  },
];

const archivedHabits = [
  {
    habitId: "habit-3",
    name: "Old Habit",
    goal: null,
    habitState: "Archived",
    habitType: "Binary",
    unit: null,
    expiryDate: null,
  },
];

const handlers = [
  http.get(`${API_URL}/teams/${TEAM_ID}`, () =>
    HttpResponse.json({ teamId: TEAM_ID, name: "Alpha Team" }),
  ),
  http.get(`${API_URL}/teams/${TEAM_ID}/habits`, ({ request }) => {
    const url = new URL(request.url);
    const state = url.searchParams.get("state");
    return HttpResponse.json(state === "Archived" ? archivedHabits : activeHabits);
  }),
];

const server = setupServer(...handlers);
beforeAll(() => server.listen());
afterEach(() => server.resetHandlers());
afterAll(() => server.close());

const App = () => (
  <MemoryRouter initialEntries={[`/member/teams/${TEAM_ID}/habits`]}>
    <Routes>
      <Route
        path="member/teams/:teamId/habits"
        element={<MemberHabits />}
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

it("renders active habits with name and type", async () => {
  render(App());

  await waitFor(() => {
    expect(screen.getByText("Morning Walk")).toBeInTheDocument();
    expect(screen.getByText("Water Intake")).toBeInTheDocument();
  });

  expect(screen.getByText("Cups")).toBeInTheDocument();
});

it("shows team name after load", async () => {
  render(App());

  await waitFor(() => {
    expect(screen.getByText("Alpha Team")).toBeInTheDocument();
  });
});

it("shows empty state when no habits exist", async () => {
  server.use(
    http.get(`${API_URL}/teams/${TEAM_ID}/habits`, () =>
      HttpResponse.json([]),
    ),
  );

  render(App());

  await waitFor(() => {
    expect(screen.getByText("No active habits found")).toBeInTheDocument();
  });
});

it("switches to Archived tab and loads archived habits", async () => {
  render(App());

  await waitFor(() => {
    expect(screen.getByText("Morning Walk")).toBeInTheDocument();
  });

  fireEvent.click(screen.getByRole("tab", { name: "Archived" }));

  await waitFor(() => {
    expect(screen.getByText("Old Habit")).toBeInTheDocument();
    expect(screen.queryByText("Morning Walk")).not.toBeInTheDocument();
  });
});

it("shows Log link only for active habits", async () => {
  render(App());

  await waitFor(() => {
    expect(screen.getByText("Morning Walk")).toBeInTheDocument();
  });

  expect(screen.getAllByRole("link", { name: "Log" })).toHaveLength(2);

  fireEvent.click(screen.getByRole("tab", { name: "Archived" }));

  await waitFor(() => {
    expect(screen.getByText("Old Habit")).toBeInTheDocument();
  });

  expect(screen.queryByRole("link", { name: "Log" })).not.toBeInTheDocument();
});

it("shows server error when load fails", async () => {
  server.use(
    http.get(`${API_URL}/teams/${TEAM_ID}/habits`, () =>
      HttpResponse.json(
        { error: "forbidden", message: "You are not a member of this team." },
        { status: 403 },
      ),
    ),
  );

  render(App());

  await waitFor(() => {
    expect(screen.getByRole("alert")).toHaveTextContent(
      "You are not a member of this team.",
    );
  });
});
