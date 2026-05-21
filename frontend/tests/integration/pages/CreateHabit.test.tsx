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
import CreateHabit from "../../../src/pages/CreateHabit";
import PathDisplay from "../PathDisplay";
import { http, HttpResponse } from "msw";
import { setupServer } from "msw/node";
import { API_URL } from "../../const";

const TEAM_ID = "team-1";
const HABIT_ID = "habit-99";

const handlers = [
  http.get(`${API_URL}/teams/${TEAM_ID}`, () =>
    HttpResponse.json({ teamId: TEAM_ID, name: "Alpha Team" }),
  ),
  http.post(`${API_URL}/teams/${TEAM_ID}/habits`, async ({ request }) => {
    const data = (await request.json()) as { name?: string } | undefined;
    if (!data?.name) {
      return HttpResponse.json(
        { error: "validation-error", message: "Habit name is required." },
        { status: 422 },
      );
    }
    return HttpResponse.json(
      {
        habitId: HABIT_ID,
        name: data.name,
        goal: null,
        habitState: "Active",
        habitType: "Binary",
        unit: null,
        expiryDate: null,
        teamId: TEAM_ID,
        creatorId: "creator-1",
      },
      { status: 201 },
    );
  }),
];

const server = setupServer(...handlers);
beforeAll(() => server.listen());
afterEach(() => server.resetHandlers());
afterAll(() => server.close());

const App = () => (
  <MemoryRouter
    initialEntries={[`/creator/teams/${TEAM_ID}/habits/new`]}
  >
    <Routes>
      <Route
        path="creator/teams/:teamId/habits/new"
        element={<CreateHabit />}
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

it("renders form fields", async () => {
  render(App());

  await screen.findByText("Alpha Team");

  expect(screen.getByLabelText("Name")).toBeInTheDocument();
  expect(screen.getByLabelText("Goal")).toBeInTheDocument();
  expect(screen.getByLabelText("Type")).toBeInTheDocument();
  expect(screen.getByLabelText("Unit")).toBeInTheDocument();
  expect(screen.getByLabelText("Expires")).toBeInTheDocument();
});

it("submit button is disabled when name is empty", async () => {
  render(App());

  await screen.findByText("Alpha Team");

  expect(
    screen.getByRole("button", { name: "Create habit" }),
  ).toBeDisabled();
});

it("shows validation error when name is blurred empty", async () => {
  render(App());

  fireEvent.blur(screen.getByLabelText("Name"));

  await waitFor(() => {
    expect(screen.getByText("Habit name is required.")).toBeInTheDocument();
  });
});

it("shows validation error when quantitative type has no unit", async () => {
  render(App());

  fireEvent.change(screen.getByLabelText("Type"), {
    target: { value: "Quantitative" },
  });
  fireEvent.change(screen.getByLabelText("Name"), {
    target: { value: "Steps habit" },
  });
  fireEvent.blur(screen.getByLabelText("Unit"));

  await waitFor(() => {
    expect(
      screen.getByText("Choose a unit for quantitative habits."),
    ).toBeInTheDocument();
  });
});

it("navigates to habits page on successful create", async () => {
  render(App());

  fireEvent.change(screen.getByLabelText("Name"), {
    target: { value: "Morning Walk" },
  });
  fireEvent.submit(screen.getByLabelText("Name"));

  await waitFor(() => {
    expect(
      screen.getByText(`This is /creator/teams/${TEAM_ID}/habits!`),
    ).toBeInTheDocument();
  });
});

it("shows server error when create fails", async () => {
  server.use(
    http.post(`${API_URL}/teams/${TEAM_ID}/habits`, () =>
      HttpResponse.json(
        { error: "validation-error", message: "Please check the habit fields and try again." },
        { status: 422 },
      ),
    ),
  );

  render(App());

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

it("displays team name after load", async () => {
  render(App());

  await waitFor(() => {
    expect(screen.getByText("Alpha Team")).toBeInTheDocument();
  });
});
