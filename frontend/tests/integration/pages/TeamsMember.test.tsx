import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { it, expect, vi, beforeEach, beforeAll, afterEach, afterAll } from "vitest";
import PathDisplay from "../PathDisplay";
import { http, HttpResponse } from "msw"
import { setupServer } from "msw/node";
import { API_URL } from "../../const";
import TeamsMember from "../../../src/pages/TeamsMember";

const handlers = [
  http.get(`${API_URL}/teams`, async () => {
    return HttpResponse.json([
      {
        teamId: "A113",
        name: "Team 1"
      },
      {
        teamId: "2056",
        name: "Mel's Team"
      }
    ], {status: 200})
  }),
  http.post(`${API_URL}/teams/:teamId/leave`, async ({ params }) => {
    if(params.teamId == "A113") {
      return new HttpResponse(null, { status: 500 })
    }
    return new HttpResponse(null, { status: 200 });
  })
]
const server = setupServer(...handlers);
beforeAll(() => server.listen())
afterEach(() => server.resetHandlers())
afterAll(() => server.close())

const App = () => (
  <MemoryRouter initialEntries={["/tested-page"]}>
    <Routes>
      <Route path="tested-page" element={<TeamsMember/>}/>
      <Route path="/*" element={<PathDisplay/>}/>
    </Routes>
  </MemoryRouter>
)
beforeEach(() => {
  localStorage.clear();
  localStorage.setItem(
    "habithubAuth",
    JSON.stringify({ sessionId: "1234" }),
  );
  vi.restoreAllMocks();
});

it("renders teams correctly", () => {
  render(App());

  waitFor(() => {
    expect(screen.getByText("Team 1")).toBeInTheDocument();
    expect(screen.getByText("Mel's Team")).toBeInTheDocument();
  });
});

it("on leave with confirmation removes team from list", async () => {
  render(App());
  const confirm = vi.fn(() => true);
  vi.stubGlobal('confirm', confirm);

  await waitFor(() => {
    fireEvent.click(screen.getAllByRole("button", { name: "Leave the team" })[1]);
  });

  await waitFor(() => {
    expect(confirm).toHaveBeenCalledOnce();
    expect(screen.getByText("Team 1")).toBeInTheDocument();
    expect(screen.queryByText("Mel's Team")).not.toBeInTheDocument();
  });
});

it("on leave with reject doesn't remove team from list", async () => {
  render(App());
  const confirm = vi.fn(() => false);
  vi.stubGlobal('confirm', confirm);

  await waitFor(() => {
    fireEvent.click(screen.getAllByRole("button", { name: "Leave the team" })[1]);
  });

  await waitFor(() => {
    expect(confirm).toHaveBeenCalledOnce();
    expect(screen.getByText("Team 1")).toBeInTheDocument();
    expect(screen.getByText("Mel's Team")).toBeInTheDocument();
  });
});

it("on leave with error doesn't remove team from list", async () => {
  render(App());
  const confirm = vi.fn(() => true);
  vi.stubGlobal('confirm', confirm);

  await waitFor(() => {
    fireEvent.click(screen.getAllByRole("button", { name: "Leave the team" })[0]);
  });

  await waitFor(() => {
    expect(confirm).toHaveBeenCalledOnce();
    expect(screen.getByText("Team 1")).toBeInTheDocument();
    expect(screen.getByText("Mel's Team")).toBeInTheDocument();
  });
});