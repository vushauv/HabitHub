import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { it, expect, vi, beforeEach, beforeAll, afterEach, afterAll } from "vitest";
import PathDisplay from "../PathDisplay";
import { http, HttpResponse } from "msw"
import { setupServer } from "msw/node";
import { API_URL } from "../../const";
import CreateTeam from "../../../src/pages/CreateTeam";

const handlers = [
  http.post(`${API_URL}/teams`, async ({ request }) => {
    const data = await request.json() as {name: string} | undefined;
    if (data == null || !("name" in data)) {
      return new HttpResponse(null, {status: 400});
    }
    if (data.name.includes("wrong")) {
      return HttpResponse.json({ error: "", message: "Invalid name!!!!!" }, {status: 400})
    }
    return HttpResponse.json({
      teamId: "A113",
      name: data.name
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
      <Route path="tested-page" element={<CreateTeam/>}/>
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

it("renders name field correctly", () => {
  render(App());

  expect(screen.getByLabelText("Name")).toBeInTheDocument();
});

it("submit button is disabled when form is empty", () => {
  render(App());

  expect(screen.getByRole("button", { name: "Submit" })).toBeDisabled();
});

it("shows validation errors on blur with empty fields", async () => {
  render(App());

  fireEvent.blur(screen.getByLabelText("Name"));

  await waitFor(() => {
    expect(screen.getByText("Team name is required.")).toBeInTheDocument();
  });
});

it("redirects to /teams-creator on 200 response", async () => {
  render(App());

  fireEvent.change(screen.getByLabelText("Name"), {
    target: { value: "12345678" },
  });
  fireEvent.submit(screen.getByLabelText("Name"));

  await waitFor(() => {
    expect(screen.getByText("This is /teams-creator!")).toBeInTheDocument();
  });
});

it("shows error message on 400 response", async () => {
  render(App());

  fireEvent.change(screen.getByLabelText("Name"), {
    target: { value: "wrong123" },
  });
  fireEvent.submit(screen.getByLabelText("Name"));

  await waitFor(() => {
    expect(screen.getByRole("alert")).toHaveTextContent(
      "Invalid name!!!!!",
    );
  });
});
