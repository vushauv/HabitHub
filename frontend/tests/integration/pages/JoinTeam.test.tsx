import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { it, expect, vi, beforeEach, beforeAll, afterEach, afterAll } from "vitest";
import PathDisplay from "../PathDisplay";
import { http, HttpResponse } from "msw"
import { setupServer } from "msw/node";
import { API_URL } from "../../const";
import JoinTeam from "../../../src/pages/JoinTeam";

const handlers = [
  http.post(`${API_URL}/teams/join`, async ({ request }) => {
    const data = await request.json() as {code: string} | undefined;
    if (data == null || !("code" in data)) {
      return new HttpResponse(null, {status: 400});
    }
    if (data.code.includes("wrong")) {
      console.log(data)
      return HttpResponse.json({ error: "code-not-found", message: "Invite code not found!!!!!" }, {status: 404})
    }
    return HttpResponse.json({
      teamId: "A113",
      memberId: "no"
    }, {status: 200})
  })
]
const server = setupServer(...handlers);
beforeAll(() => server.listen())
afterEach(() => server.resetHandlers())
afterAll(() => server.close())

const App = () => (
  <MemoryRouter initialEntries={["/tested-page"]}>
    <Routes>
      <Route path="tested-page" element={<JoinTeam/>}/>
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

it("renders invite code field correctly", () => {
  render(App());

  expect(screen.getByLabelText("Invite Code")).toBeInTheDocument();
});

it("submit button is disabled when form is empty", () => {
  render(App());

  expect(screen.getByRole("button", { name: "Submit" })).toBeDisabled();
});

it("shows validation errors on blur with empty fields", async () => {
  render(App());

  fireEvent.blur(screen.getByLabelText("Invite Code"));

  await waitFor(() => {
    expect(screen.getByText("Invite code is required.")).toBeInTheDocument();
  });
});

it("redirects to /teams-member on 200 response", async () => {
  render(App());

  fireEvent.change(screen.getByLabelText("Invite Code"), {
    target: { value: "12345678" },
  });
  fireEvent.submit(screen.getByLabelText("Invite Code"));

  await waitFor(() => {
    expect(screen.getByText("This is /teams-member!")).toBeInTheDocument();
  });
});

it("shows error message on 404 response", async () => {
  render(App());

  fireEvent.change(screen.getByLabelText("Invite Code"), {
    target: { value: "wrong123" },
  });
  fireEvent.submit(screen.getByLabelText("Invite Code"));

  await waitFor(() => {
    expect(screen.getByRole("alert")).toHaveTextContent(
      "Invite code not found!!!!!",
    );
  });
});
