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
import CreatorHabits from "../../../src/pages/CreatorHabits";
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
  http.post(`${API_URL}/habits/habit-1/archive`, () =>
    new HttpResponse(null, { status: 204 }),
  ),
  http.delete(`${API_URL}/habits/habit-1`, () =>
    new HttpResponse(null, { status: 204 }),
  ),
];

const server = setupServer(...handlers);
beforeAll(() => server.listen());
afterEach(() => server.resetHandlers());
afterAll(() => server.close());

const App = () => (
  <MemoryRouter initialEntries={[`/creator/teams/${TEAM_ID}/habits`]}>
    <Routes>
      <Route
        path="creator/teams/:teamId/habits"
        element={<CreatorHabits />}
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

it("renders habits list with name and type", async () => {
  render(App());

  await waitFor(() => {
    expect(screen.getByText("Morning Walk")).toBeInTheDocument();
    expect(screen.getByText("Water Intake")).toBeInTheDocument();
  });

  expect(screen.getAllByText("Binary")[0]).toBeInTheDocument();
  expect(screen.getByText("Cups")).toBeInTheDocument();
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

it("loads archived habits when switching to Archived tab", async () => {
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

it("archives a habit and removes it from the list", async () => {
  vi.spyOn(window, "confirm").mockReturnValue(true);

  render(App());

  await waitFor(() => {
    expect(screen.getByText("Morning Walk")).toBeInTheDocument();
  });

  const archiveButtons = screen.getAllByRole("button", { name: "Archive" });
  fireEvent.click(archiveButtons[0]);

  await waitFor(() => {
    expect(screen.queryByText("Morning Walk")).not.toBeInTheDocument();
    expect(screen.getByText("Habit archived.")).toBeInTheDocument();
  });
});

it("does not archive when confirmation is declined", async () => {
  vi.spyOn(window, "confirm").mockReturnValue(false);

  render(App());

  await waitFor(() => {
    expect(screen.getByText("Morning Walk")).toBeInTheDocument();
  });

  const archiveButtons = screen.getAllByRole("button", { name: "Archive" });
  fireEvent.click(archiveButtons[0]);

  await waitFor(() => {
    expect(screen.getByText("Morning Walk")).toBeInTheDocument();
  });
});

it("deletes a habit and removes it from the list", async () => {
  vi.spyOn(window, "confirm").mockReturnValue(true);

  render(App());

  await waitFor(() => {
    expect(screen.getByText("Morning Walk")).toBeInTheDocument();
  });

  const deleteButtons = screen.getAllByRole("button", { name: "Delete" });
  fireEvent.click(deleteButtons[0]);

  await waitFor(() => {
    expect(screen.queryByText("Morning Walk")).not.toBeInTheDocument();
    expect(screen.getByText("Habit deleted.")).toBeInTheDocument();
  });
});

it("shows error on failed archive", async () => {
  vi.spyOn(window, "confirm").mockReturnValue(true);

  server.use(
    http.post(`${API_URL}/habits/habit-1/archive`, () =>
      HttpResponse.json(
        { error: "forbidden", message: "You do not have permission to manage this habit." },
        { status: 403 },
      ),
    ),
  );

  render(App());

  await waitFor(() => {
    expect(screen.getByText("Morning Walk")).toBeInTheDocument();
  });

  const archiveButtons = screen.getAllByRole("button", { name: "Archive" });
  fireEvent.click(archiveButtons[0]);

  await waitFor(() => {
    expect(screen.getByRole("alert")).toHaveTextContent(
      "You do not have permission to manage this habit.",
    );
  });
});
