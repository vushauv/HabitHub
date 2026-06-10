import { render, screen, fireEvent, waitFor, act } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { it, expect, vi, beforeEach, beforeAll, afterEach, afterAll } from "vitest";
import { http, HttpResponse } from "msw";
import { setupServer } from "msw/node";
import { API_URL } from "../../const";
import Chat from "../../../src/pages/Chat";

// --- SignalR mock -----------------------------------------------------------

type HubHandler = (...args: unknown[]) => void;

const { hubHandlers, mockStart, mockStop, mockInvoke } = vi.hoisted(() => {
  const hubHandlers: Record<string, HubHandler[]> = {};
  const mockStart = vi.fn().mockResolvedValue(undefined);
  const mockStop = vi.fn().mockResolvedValue(undefined);
  const mockInvoke = vi.fn().mockResolvedValue(undefined);
  return { hubHandlers, mockStart, mockStop, mockInvoke };
});

vi.mock("@microsoft/signalr", () => ({
  HubConnectionBuilder: class {
    withUrl() { return this; }
    withAutomaticReconnect() { return this; }
    build() {
      return {
        on(event: string, handler: HubHandler) {
          hubHandlers[event] = hubHandlers[event] ?? [];
          hubHandlers[event].push(handler);
        },
        start: mockStart,
        stop: mockStop,
        invoke: mockInvoke,
        state: "Connected",
      };
    }
  },
  HubConnectionState: { Connected: "Connected", Disconnected: "Disconnected" },
}));

function triggerReceiveMessage(msg: object) {
  hubHandlers["ReceiveMessage"]?.forEach((h) => h(msg));
}

function triggerMessageDeleted(id: string) {
  hubHandlers["MessageDeleted"]?.forEach((h) => h(id));
}

// --- msw handlers -----------------------------------------------------------

const TEAM_ID = "team-abc-123";
const USER_ID = "user-1";

const existingMessage = {
  messageId: "msg-1",
  chatId: "chat-1",
  userId: USER_ID,
  authorName: "Alice",
  content: "Hello from history",
  sendDate: new Date(Date.now() - 60_000).toISOString(),
};

const handlers = [
  http.get(`${API_URL}/auth/me`, () =>
    HttpResponse.json({ id: USER_ID, name: "Alice", userType: "Creator" }),
  ),
  http.get(`${API_URL}/teams/:teamId`, () =>
    HttpResponse.json({ teamId: TEAM_ID, name: "My Team" }),
  ),
  http.get(`${API_URL}/teams/:teamId/chat/messages`, () =>
    HttpResponse.json([existingMessage]),
  ),
  http.post(`${API_URL}/teams/:teamId/chat/messages`, async ({ request }) => {
    const body = (await request.json()) as { content: string };
    return HttpResponse.json(
      {
        messageId: "msg-new",
        chatId: "chat-1",
        userId: USER_ID,
        authorName: "Alice",
        content: body.content,
        sendDate: new Date().toISOString(),
      },
      { status: 201 },
    );
  }),
  http.delete(`${API_URL}/teams/:teamId/chat/messages/:messageId`, () =>
    new HttpResponse(null, { status: 204 }),
  ),
];

const server = setupServer(...handlers);
beforeAll(() => server.listen());
afterEach(() => {
  server.resetHandlers();
  for (const key of Object.keys(hubHandlers)) {
    delete hubHandlers[key];
  }
});
afterAll(() => server.close());

// --- helpers ----------------------------------------------------------------

const App = () => (
  <MemoryRouter initialEntries={[`/creator/teams/${TEAM_ID}/chat`]}>
    <Routes>
      <Route path="/creator/teams/:teamId/chat" element={<Chat />} />
    </Routes>
  </MemoryRouter>
);

beforeEach(() => {
  localStorage.clear();
  localStorage.setItem("habithubAuth", JSON.stringify({ sessionId: "sess-1" }));
  vi.restoreAllMocks();
});

// --- tests ------------------------------------------------------------------

it("shows existing messages on load", async () => {
  render(App());

  await waitFor(() => {
    expect(screen.getByText("Hello from history")).toBeInTheDocument();
  });
});

it("sends a message and shows it in the list", async () => {
  render(App());
  await waitFor(() => screen.getByText("Hello from history"));

  fireEvent.change(screen.getByRole("textbox"), {
    target: { value: "New message" },
  });
  fireEvent.submit(screen.getByRole("textbox").closest("form")!);

  await waitFor(() => {
    expect(screen.getByText("New message")).toBeInTheDocument();
  });
});

it("ReceiveMessage event appends message without duplicate", async () => {
  render(App());
  await waitFor(() => screen.getByText("Hello from history"));

  const incoming = {
    messageId: "msg-incoming",
    chatId: "chat-1",
    userId: "user-2",
    authorName: "Bob",
    content: "Live message from Bob",
    sendDate: new Date().toISOString(),
  };

  act(() => triggerReceiveMessage(incoming));

  await waitFor(() => {
    expect(screen.getByText("Live message from Bob")).toBeInTheDocument();
  });

  act(() => triggerReceiveMessage(incoming));
  expect(screen.getAllByText("Live message from Bob")).toHaveLength(1);
});

it("MessageDeleted event removes message from list", async () => {
  render(App());
  await waitFor(() => screen.getByText("Hello from history"));

  act(() => triggerMessageDeleted("msg-1"));

  await waitFor(() => {
    expect(screen.queryByText("Hello from history")).not.toBeInTheDocument();
  });
});

it("delete button calls DELETE API", async () => {
  let deleteCalled = false;
  server.use(
    http.delete(`${API_URL}/teams/:teamId/chat/messages/:messageId`, () => {
      deleteCalled = true;
      return new HttpResponse(null, { status: 204 });
    }),
  );

  render(App());
  await waitFor(() => screen.getByText("Hello from history"));

  vi.spyOn(window, "confirm").mockReturnValue(true);
  fireEvent.click(screen.getByRole("button", { name: "Delete message" }));

  await waitFor(() => {
    expect(deleteCalled).toBe(true);
  });
});
