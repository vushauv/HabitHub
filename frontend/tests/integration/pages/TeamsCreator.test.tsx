import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { it, expect, vi, beforeEach, beforeAll, afterEach, afterAll } from "vitest";
import PathDisplay from "../PathDisplay";
import { http, HttpResponse } from "msw"
import { setupServer } from "msw/node";
import { API_URL } from "../../const";
import TeamsCreator from "../../../src/pages/TeamsCreator";

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
  http.get(`${API_URL}/teams/:teamId/invite-codes`, async ({ params }) => {
    if (params.teamId == "A113") {
      return HttpResponse.json([
        {
          codeId: "yes",
          code: "ThisIsA113Code",
          teamId: params.teamId,
          expiryDate: "2004-05-22T18:06:52.714506Z"
        }
      ], {status: 200})
    }
    return HttpResponse.json([], {status: 200})
  }),
  http.delete(`${API_URL}/teams/:teamId`, async ({ params }) => {
    if(params.teamId == "A113") {
      return new HttpResponse(null, { status: 500 })
    }
    return new HttpResponse(null, { status: 200 });
  }),
  http.delete(`${API_URL}/teams/:teamId/invite-codes/:inviteCodeId`, async () => {
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
      <Route path="tested-page" element={<TeamsCreator/>}/>
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

it("renders team invite codes correctly", () => {
  render(App());

  waitFor(() => {
    expect(screen.getByText("Team 1")).toBeInTheDocument();
    expect(screen.getByText("ThisIsA113Code")).toBeInTheDocument();
    expect(screen.getByText("Mel's Team")).toBeInTheDocument();
    expect(screen.getByText("No Active Code")).toBeInTheDocument();
  });
});

it("on invalidate code invalidates code", async () => {
  render(App());

  await waitFor(() => {
    fireEvent.click(screen.getByRole("button", { name: "Invalidate" }));
  });

  await waitFor(() => {
    expect(screen.getByText("Team 1")).toBeInTheDocument();
    expect(screen.queryByText("ThisIsA113Code")).not.toBeInTheDocument();
    expect(screen.getByText("Mel's Team")).toBeInTheDocument();
    expect(screen.getAllByText("No Active Code")).toHaveLength(2);
  });
});

it("on delete with confirmation removes team from list", async () => {
  render(App());
  const confirm = vi.fn(() => true);
  vi.stubGlobal('confirm', confirm);

  await waitFor(() => {
    fireEvent.click(screen.getAllByRole("button", { name: "Delete team" })[1]);
  });

  await waitFor(() => {
    expect(confirm).toHaveBeenCalledOnce();
    expect(screen.getByText("Team 1")).toBeInTheDocument();
    expect(screen.queryByText("Mel's Team")).not.toBeInTheDocument();
  });
});

it("on delete with reject doesn't remove team from list", async () => {
  render(App());
  const confirm = vi.fn(() => false);
  vi.stubGlobal('confirm', confirm);

  await waitFor(() => {
    fireEvent.click(screen.getAllByRole("button", { name: "Delete team" })[1]);
  });

  await waitFor(() => {
    expect(confirm).toHaveBeenCalledOnce();
    expect(screen.getByText("Team 1")).toBeInTheDocument();
    expect(screen.getByText("Mel's Team")).toBeInTheDocument();
  });
});

it("on delete with error doesn't remove team from list", async () => {
  render(App());
  const confirm = vi.fn(() => true);
  vi.stubGlobal('confirm', confirm);

  await waitFor(() => {
    fireEvent.click(screen.getAllByRole("button", { name: "Delete team" })[0]);
  });

  await waitFor(() => {
    expect(confirm).toHaveBeenCalledOnce();
    expect(screen.getByText("Team 1")).toBeInTheDocument();
    expect(screen.getByText("Mel's Team")).toBeInTheDocument();
  });
});