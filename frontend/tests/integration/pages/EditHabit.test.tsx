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
import EditHabit from "../../../src/pages/EditHabit";
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
  teamId: TEAM_ID,
  creatorId: "creator-1",
};

const handlers = [
  http.get(`${API_URL}/teams/${TEAM_ID}`, () =>
    HttpResponse.json({ teamId: TEAM_ID, name: "Alpha Team" }),
  ),
  http.get(`${API_URL}/habits/${HABIT_ID}`, () => HttpResponse.json(habit)),
  http.patch(`${API_URL}/habits/${HABIT_ID}`, async ({ request }) => {
    const data = (await request.json()) as { name?: string } | undefined;
    if (!data?.name) {
      return HttpResponse.json(
        { error: "validation-error", message: "Habit name is required." },
        { status: 422 },
      );
    }
    return HttpResponse.json({ ...habit, name: data.name });
  }),
];

const server = setupServer(...handlers);
beforeAll(() => server.listen());
afterEach(() => server.resetHandlers());
afterAll(() => server.close());

const App = () => (
  <MemoryRouter
    initialEntries={[`/creator/teams/${TEAM_ID}/habits/${HABIT_ID}/edit`]}
  >
    <Routes>
      <Route
        path="creator/teams/:teamId/habits/:habitId/edit"
        element={<EditHabit />}
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
});

it("loads habit values into form", async () => {
  render(App());

  await waitFor(() => {
    expect(screen.getByLabelText("Name")).toHaveValue("Morning Walk");
    expect(screen.getByLabelText("Goal")).toHaveValue("Walk before work");
  });
});

it("displays team name after load", async () => {
  render(App());

  await waitFor(() => {
    expect(screen.getByText("Alpha Team")).toBeInTheDocument();
  });
});

it("shows readonly type and unit", async () => {
  render(App());

  await waitFor(() => {
    expect(screen.getByText("Binary")).toBeInTheDocument();
    expect(screen.getByText("No unit")).toBeInTheDocument();
  });
});

it("navigates to habit details on successful save", async () => {
  render(App());

  await waitFor(() => {
    expect(screen.getByLabelText("Name")).toHaveValue("Morning Walk");
  });

  fireEvent.change(screen.getByLabelText("Name"), {
    target: { value: "Evening Walk" },
  });
  fireEvent.submit(screen.getByLabelText("Name"));

  await waitFor(() => {
    expect(
      screen.getByText(
        `This is /creator/teams/${TEAM_ID}/habits/${HABIT_ID}/details!`,
      ),
    ).toBeInTheDocument();
  });
});

it("shows server error when save fails", async () => {
  server.use(
    http.patch(`${API_URL}/habits/${HABIT_ID}`, () =>
      HttpResponse.json(
        { error: "validation-error", message: "Please check the habit fields and try again." },
        { status: 422 },
      ),
    ),
  );

  render(App());

  await waitFor(() => {
    expect(screen.getByLabelText("Name")).toHaveValue("Morning Walk");
  });

  fireEvent.change(screen.getByLabelText("Name"), {
    target: { value: "Bad Habit" },
  });
  fireEvent.submit(screen.getByLabelText("Name"));

  await waitFor(() => {
    expect(screen.getByRole("alert")).toHaveTextContent(
      "Please check the habit fields and try again.",
    );
  });
});

it("clears goal field when clear goal is checked", async () => {
  render(App());

  await waitFor(() => {
    expect(screen.getByLabelText("Goal")).toHaveValue("Walk before work");
  });

  fireEvent.click(screen.getByLabelText("Clear goal"));

  await waitFor(() => {
    expect(screen.getByLabelText("Goal")).toHaveValue("");
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
