import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { it, expect, vi, beforeEach, beforeAll, afterEach, afterAll } from "vitest";
import PathDisplay from "../PathDisplay";
import { http, HttpResponse } from "msw"
import { setupServer } from "msw/node";
import { API_URL } from "../../const";
import MemberList from "../../../src/pages/MemberList";

const handlers = [
  http.get(`${API_URL}/teams/:teamId`, async () => {
    return HttpResponse.json({
      teamId: "A113",
      name: "Team 1"
    }, {status: 200})
  }),
  http.get(`${API_URL}/teams/:teamId/members`, async () => {
    return HttpResponse.json([
      {
        memberId: "john1",
        name: "John Member The 1st",
        email: "john.member.i@example.com",
        status: "Active"
      },
      {
        memberId: "john2",
        name: "John Member The 2nd",
        email: "john.member.ii@example.com",
        status: "Active"
      }
    ], {status: 200})
  }),
  http.post(`${API_URL}/teams/:teamId/members/:memberId/kick`, async ({ params }) => {
    if (params.memberId == "john1") {
      return new HttpResponse(null, { status: 500 });
    }
    return new HttpResponse(null, { status: 200 });
  })
]
const server = setupServer(...handlers);
beforeAll(() => server.listen())
afterEach(() => server.resetHandlers())
afterAll(() => server.close())

const App = () => (
  <MemoryRouter initialEntries={["/creator/teams/A113/members"]}>
    <Routes>
      <Route path="/creator/teams/:teamId/members" element={<MemberList/>}/>
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

it("renders team name and members correctly", async () => {
  render(App());

  await waitFor(() => {
    expect(screen.getByText("Team 1")).toBeInTheDocument();
    expect(screen.getByText("John Member The 1st")).toBeInTheDocument();
    expect(screen.getByText("john.member.i@example.com")).toBeInTheDocument();
    expect(screen.getByText("John Member The 2nd")).toBeInTheDocument();
    expect(screen.getByText("john.member.ii@example.com")).toBeInTheDocument();
  });
});

it("on kick with confirmation removes member from list", async () => {
  render(App());
  const confirm = vi.fn(() => true);
  vi.stubGlobal('confirm', confirm);

  const kickButtons = await screen.findAllByRole("button", { name: "Kick" });
  fireEvent.click(kickButtons[1]);

  await waitFor(() => {
    expect(confirm).toHaveBeenCalledOnce();
    expect(screen.getByText("John Member The 1st")).toBeInTheDocument();
    expect(screen.getByText("john.member.i@example.com")).toBeInTheDocument();
    expect(screen.queryByText("John Member The 2nd")).not.toBeInTheDocument();
    expect(screen.queryByText("john.member.ii@example.com")).not.toBeInTheDocument();
  });
});

it("on kick with reject doesn't remove member from list", async () => {
  render(App());
  const confirm = vi.fn(() => false);
  vi.stubGlobal('confirm', confirm);

  const kickButtons = await screen.findAllByRole("button", { name: "Kick" });
  fireEvent.click(kickButtons[1]);

  await waitFor(() => {
    expect(confirm).toHaveBeenCalledOnce();
    expect(screen.getByText("John Member The 1st")).toBeInTheDocument();
    expect(screen.getByText("john.member.i@example.com")).toBeInTheDocument();
    expect(screen.getByText("John Member The 2nd")).toBeInTheDocument();
    expect(screen.getByText("john.member.ii@example.com")).toBeInTheDocument();
  });
});

it("on kick with error doesn't remove member from list", async () => {
  render(App());
  const confirm = vi.fn(() => true);
  vi.stubGlobal('confirm', confirm);

  const kickButtons = await screen.findAllByRole("button", { name: "Kick" });
  fireEvent.click(kickButtons[0]);

  await waitFor(() => {
    expect(confirm).toHaveBeenCalledOnce();
    expect(screen.getByText("John Member The 1st")).toBeInTheDocument();
    expect(screen.getByText("john.member.i@example.com")).toBeInTheDocument();
    expect(screen.getByText("John Member The 2nd")).toBeInTheDocument();
    expect(screen.getByText("john.member.ii@example.com")).toBeInTheDocument();
  });
});
