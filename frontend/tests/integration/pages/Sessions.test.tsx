import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { it, expect, vi, beforeEach, beforeAll, afterEach, afterAll } from "vitest";
import Sessions from "../../../src/pages/Sessions";
import PathDisplay from "../PathDisplay";
import { http, HttpResponse } from "msw"
import { setupServer } from "msw/node";
import { API_URL } from "../../const";

const handlers = [
  http.get(`${API_URL}/auth/sessions`, async () => {
    return HttpResponse.json([
      {
        sessionId: "1234",
        userType: 0,
        createdAt: new Date().toISOString(),
        lastActiveAt: new Date().toISOString(),
        expiresAt: new Date().toISOString(),
        sessionState: 0,
        isCurrent: true,
        deviceInfo: "Device1Info",
        ipAddress: "Ip1Address"
      },
      {
        sessionId: "1235",
        userType: 0,
        createdAt: new Date().toISOString(),
        lastActiveAt: new Date().toISOString(),
        expiresAt: new Date().toISOString(),
        sessionState: 0,
        isCurrent: false,
        deviceInfo: "Device2Info",
        ipAddress: "Ip2Address"
      }
    ], {status: 200})
  }),
  http.delete(`${API_URL}/auth/sessions/:sessionId`, async () => {
    return new HttpResponse(null, { status: 200 });
  })
]
const server = setupServer(...handlers);
beforeAll(() => server.listen())
afterEach(() => server.resetHandlers())
afterAll(() => server.close())

const App = () => (
  <MemoryRouter initialEntries={["/settings/sessions"]}>
    <Routes>
      <Route path="settings/sessions" element={<Sessions/>}/>
      <Route path="/*" element={<PathDisplay/>}/>
    </Routes>
  </MemoryRouter>
)
beforeEach(() => {
  localStorage.clear();
  localStorage.setItem(
    "habithubAuth",
    JSON.stringify({ isLoggedIn: true, sessionId: "1234", userType: "Creator" }),
  );
  vi.restoreAllMocks();
});

it("renders sessions correctly", () => {
  render(App());

  waitFor(() => {
    expect(screen.getByText("Device1Info")).toBeInTheDocument()
    expect(screen.getByText("Ip1Address")).toBeInTheDocument()
  });
});

it("on delete removes session from list", async () => {
  render(App());

  await waitFor(() => {
    expect(screen.getByText("Device1Info")).toBeInTheDocument();
    expect(screen.getByText("Ip1Address")).toBeInTheDocument();
    expect(screen.getByText("Device2Info")).toBeInTheDocument();
    expect(screen.getByText("Ip2Address")).toBeInTheDocument();
    fireEvent.click(screen.getAllByRole("button", { name: "Invalidate" })[1]);
  });

  await waitFor(() => {
    expect(screen.getByText("Device1Info")).toBeInTheDocument();
    expect(screen.getByText("Ip1Address")).toBeInTheDocument();
    expect(screen.queryByText("Device2Info")).not.toBeInTheDocument();
    expect(screen.queryByText("Ip2Address")).not.toBeInTheDocument();
  });
});