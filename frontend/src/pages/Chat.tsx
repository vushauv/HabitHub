import { Link, useLocation, useNavigate, useParams } from "react-router-dom";
import { memo, useCallback, useEffect, useLayoutEffect, useMemo, useRef, useState } from "react";
import "./Chat.css";
import "../App.css";
import {
  ChatRequestError,
  clearStoredAuth,
  deleteMessage,
  getChatErrorMessage,
  getMessages,
  getStoredAuth,
  sendMessage,
  type ChatMessageDto,
} from "../services/Chat";
import { getTeam } from "../services/Team";
import { useCurrentUser } from "../hooks/useCurrentUser";

const PAGE_SIZE = 10;

function resolveErrorMessage(error: unknown): string {
  if (error instanceof ChatRequestError) {
    return error.message || getChatErrorMessage(error.code);
  }

  return "Something went wrong with the chat. Please try again.";
}

function sortByDateAscending(items: ChatMessageDto[]): ChatMessageDto[] {
  return [...items].sort(
    (left, right) =>
      new Date(left.sendDate).getTime() - new Date(right.sendDate).getTime(),
  );
}

const timeFormatter = new Intl.DateTimeFormat(undefined, {
  hour: "2-digit",
  minute: "2-digit",
});

function formatTime(iso: string): string {
  const date = new Date(iso);
  if (Number.isNaN(date.getTime())) return "";
  return timeFormatter.format(date);
}

type MessageRowProps = {
  message: ChatMessageDto;
  isOwn: boolean;
  canDelete: boolean;
  isDeleting: boolean;
  onDelete: (message: ChatMessageDto) => void;
};

const MessageRow = memo(function MessageRow({
  message,
  isOwn,
  canDelete,
  isDeleting,
  onDelete,
}: MessageRowProps) {
  return (
    <div
      className={"chat-message-row" + (isOwn ? " chat-message-row-own" : "")}
    >
      <div className={"chat-bubble" + (isOwn ? " chat-bubble-own" : "")}>
        {!isOwn ? (
          <span className="chat-bubble-author">{message.authorName}</span>
        ) : null}
        <p className="chat-bubble-content">{message.content}</p>
        <span className="chat-bubble-meta">{formatTime(message.sendDate)}</span>
      </div>

      {canDelete ? (
        <button
          type="button"
          className="button button-secondary chat-delete-button"
          onClick={() => onDelete(message)}
          disabled={isDeleting}
          aria-label="Delete message"
        >
          {isDeleting ? "..." : "Delete"}
        </button>
      ) : null}
    </div>
  );
});

export default function Chat() {
  const { teamId } = useParams<{ teamId: string }>();
  const navigate = useNavigate();
  const location = useLocation();
  const auth = useMemo(() => getStoredAuth(), []);
  const { currentUser } = useCurrentUser(auth);
  const isCreator = location.pathname.startsWith("/creator");
  const teamsHref = isCreator ? "/creator/teams" : "/member/teams";

  const [messages, setMessages] = useState<ChatMessageDto[]>([]);
  const [teamName, setTeamName] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [isLoadingMore, setIsLoadingMore] = useState(false);
  const [hasMore, setHasMore] = useState(true);
  const [draft, setDraft] = useState("");
  const [isSending, setIsSending] = useState(false);
  const [pendingDeleteId, setPendingDeleteId] = useState<string | null>(null);
  const [pageError, setPageError] = useState<string | null>(null);

  const messagesContainerRef = useRef<HTMLDivElement>(null);
  const shouldScrollToBottomRef = useRef(false);
  const preserveScrollRef = useRef<{ scrollHeight: number; scrollTop: number } | null>(null);

  useLayoutEffect(() => {
    const container = messagesContainerRef.current;
    if (!container) {
      return;
    }

    if (preserveScrollRef.current) {
      const { scrollHeight: prevScrollHeight, scrollTop: prevScrollTop } =
        preserveScrollRef.current;
      container.scrollTop =
        container.scrollHeight - prevScrollHeight + prevScrollTop;
      preserveScrollRef.current = null;
      return;
    }

    if (shouldScrollToBottomRef.current) {
      container.scrollTop = container.scrollHeight;
      shouldScrollToBottomRef.current = false;
    }
  }, [messages]);

  useEffect(() => {
    let isMounted = true;

    const loadInitial = async () => {
      setLoading(true);
      setPageError(null);

      if (!auth) {
        clearStoredAuth();

        if (isMounted) {
          setPageError("Your session is no longer valid. Please log in again.");
          setLoading(false);
        }

        setTimeout(() => {
          navigate("/login", { replace: true });
        }, 1200);

        return;
      }

      if (!teamId) {
        if (isMounted) {
          setPageError("Team not specified.");
          setLoading(false);
        }
        return;
      }

      try {
        const [loaded, team] = await Promise.all([
          getMessages(auth, teamId, { count: PAGE_SIZE, offset: 0 }),
          getTeam(auth, teamId),
        ]);

        if (!isMounted) {
          return;
        }

        shouldScrollToBottomRef.current = true;
        setMessages(sortByDateAscending(loaded));
        setHasMore(loaded.length === PAGE_SIZE);
        setTeamName(team.name);
      } catch (error) {
        if (!isMounted) {
          return;
        }

        handleActionError(error);
      } finally {
        if (isMounted) {
          setLoading(false);
        }
      }
    };

    void loadInitial();

    return () => {
      isMounted = false;
    };
  }, [auth, teamId, navigate]);

  const handleActionError = useCallback(
    (error: unknown) => {
      if (error instanceof ChatRequestError && error.code === "auth-required") {
        clearStoredAuth();
        setPageError(getChatErrorMessage(error.code));

        setTimeout(() => {
          navigate("/login", { replace: true });
        }, 1200);

        return;
      }

      setPageError(resolveErrorMessage(error));
    },
    [navigate],
  );

  const handleLoadOlder = async () => {
    if (isLoadingMore || !hasMore || !teamId) {
      return;
    }

    setIsLoadingMore(true);
    setPageError(null);

    try {
      const older = await getMessages(auth, teamId, {
        count: PAGE_SIZE,
        offset: messages.length,
      });

      const container = messagesContainerRef.current;
      if (container) {
        preserveScrollRef.current = {
          scrollHeight: container.scrollHeight,
          scrollTop: container.scrollTop,
        };
      }

      setMessages((current) => sortByDateAscending([...older, ...current]));
      setHasMore(older.length === PAGE_SIZE);
    } catch (error) {
      handleActionError(error);
    } finally {
      setIsLoadingMore(false);
    }
  };

  const handleSend = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const content = draft.trim();

    if (!content || isSending || !teamId) {
      return;
    }

    setIsSending(true);
    setPageError(null);

    try {
      const created = await sendMessage(auth, teamId, content);

      shouldScrollToBottomRef.current = true;
      setMessages((current) => sortByDateAscending([...current, created]));
      setDraft("");
    } catch (error) {
      handleActionError(error);
    } finally {
      setIsSending(false);
    }
  };

  const handleDelete = useCallback(
    async (message: ChatMessageDto) => {
      if (!teamId) {
        return;
      }

      const confirmed = window.confirm("Delete this message?");
      if (!confirmed) {
        return;
      }

      setPendingDeleteId(message.messageId);
      setPageError(null);

      try {
        await deleteMessage(auth, teamId, message.messageId);

        setMessages((current) =>
          current.filter((existing) => existing.messageId !== message.messageId),
        );
      } catch (error) {
        handleActionError(error);
      } finally {
        setPendingDeleteId(null);
      }
    },
    [auth, teamId, handleActionError],
  );

  const currentUserId = currentUser?.id ?? null;

  return (
    <main className="page">
      <section className="container">
        <div className="background-glow background-glow-left" />
        <div className="background-glow background-glow-right" />

        <div className="card page-card-shell">
          <div className="content chat-content">
            <div className="page-topbar chat-topbar">
              <div className="chat-topbar-nav">
                <Link to="/" className="button button-secondary page-nav-button">
                  Home
                </Link>
                <Link
                  to={teamsHref}
                  className="button button-secondary page-nav-button"
                >
                  Teams
                </Link>
              </div>

              <h1 className="chat-topbar-title">
                {teamName ?? "Team Chat"}
              </h1>
            </div>

            {pageError ? (
              <p className="form-error page-message" role="alert">
                {pageError}
              </p>
            ) : null}

            <div
              className="chat-messages"
              aria-label="Chat messages"
              ref={messagesContainerRef}
            >
              {hasMore && !loading ? (
                <button
                  type="button"
                  className="button button-secondary chat-load-more"
                  onClick={() => void handleLoadOlder()}
                  disabled={isLoadingMore}
                >
                  {isLoadingMore ? "Loading..." : "Load older messages"}
                </button>
              ) : null}

              {loading ? (
                <div className="state-card">
                  <p className="state-title">Loading messages...</p>
                </div>
              ) : messages.length === 0 ? (
                <div className="state-card">
                  <p className="state-title">No messages yet</p>
                  <p className="state-text">
                    Start the conversation by sending the first message.
                  </p>
                </div>
              ) : (
                messages.map((message) => {
                  const isOwn =
                    currentUserId !== null && message.userId === currentUserId;
                  return (
                    <MessageRow
                      key={message.messageId}
                      message={message}
                      isOwn={isOwn}
                      canDelete={isOwn || isCreator}
                      isDeleting={pendingDeleteId === message.messageId}
                      onDelete={handleDelete}
                    />
                  );
                })
              )}
            </div>

            <form className="chat-composer" onSubmit={(event) => void handleSend(event)}>
              <input
                type="text"
                className="form-input chat-composer-input"
                placeholder="Message"
                value={draft}
                onChange={(event) => setDraft(event.target.value)}
                disabled={isSending}
                aria-label="Message"
              />
              <button
                type="submit"
                className="button button-primary chat-composer-send"
                disabled={draft.trim().length === 0 || isSending}
              >
                {isSending ? "Sending..." : "Send"}
              </button>
            </form>
          </div>
        </div>
      </section>
    </main>
  );
}
