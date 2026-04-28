import { Link, useNavigate } from "react-router-dom";
import { useEffect, useMemo, useState } from "react";
import "./Sessions.css";
import "../App.css";
import type { SessionDto } from "../services/dtos";
import { API_BASE_URL, type AccountType } from "../services/User";

type StoredAuth = {
  isLoggedIn?: boolean;
  userType?: AccountType;
  sessionId?: string | null;
  userId?: string | null;
};

type SessionsErrorCode = "auth-required" | "not-found" | "unknown";

function getStoredAuth(): StoredAuth | null {
  const rawAuth = localStorage.getItem("habithubAuth");

  if (!rawAuth) {
    return null;
  }

  try {
    return JSON.parse(rawAuth) as StoredAuth;
  } catch {
    localStorage.removeItem("habithubAuth");
    return null;
  }
}

function clearStoredAuth(): void {
  localStorage.removeItem("habithubAuth");
}

function getAuthHeaders(auth: StoredAuth | null): HeadersInit {
  return {
    "Content-Type": "application/json",
    ...(auth?.sessionId ? { "X-Session-Id": auth.sessionId } : {}),
  };
}

function getErrorCode(status: number): SessionsErrorCode {
  if (status === 401) {
    return "auth-required";
  }

  if (status === 404) {
    return "not-found";
  }

  return "unknown";
}

function formatDate(dateString: string): string {
  const parsedDate = new Date(dateString);

  if (Number.isNaN(parsedDate.getTime())) {
    return "Unknown date";
  }

  return new Intl.DateTimeFormat(undefined, {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(parsedDate);
}

function getFriendlyErrorMessage(errorCode: SessionsErrorCode): string {
  switch (errorCode) {
    case "auth-required":
      return "Your session is no longer valid. Please log in again.";
    case "not-found":
      return "The selected session could not be found or is no longer active.";
    default:
      return "Something went wrong while loading your sessions. Please try again.";
  }
}

export default function Sessions() {
  const navigate = useNavigate();
  const auth = useMemo(() => getStoredAuth(), []);
  const [sessions, setSessions] = useState<SessionDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [pageError, setPageError] = useState<string | null>(null);
  const [pendingSessionID, setPendingSessionID] = useState<string | null>(null);

  useEffect(() => {
    let isMounted = true;

    const loadSessions = async () => {
      setLoading(true);
      setPageError(null);

      if (!auth?.isLoggedIn || !auth.sessionId) {
        if (!isMounted) {
          return;
        }

        setPageError("Your session is no longer valid. Please log in again.");
        setLoading(false);
        return;
      }

      try {
        const response = await fetch(`${API_BASE_URL}/auth/sessions`, {
          method: "GET",
          headers: getAuthHeaders(auth),
        });

        if (!response.ok) {
          const errorCode = getErrorCode(response.status);

          if (errorCode === "auth-required") {
            clearStoredAuth();

            if (isMounted) {
              setPageError(getFriendlyErrorMessage(errorCode));
              setSessions([]);
              setLoading(false);
            }

            setTimeout(() => {
              navigate("/login", { replace: true });
            }, 1200);

            return;
          }

          throw new Error(getFriendlyErrorMessage(errorCode));
        }

        const rawData = (await response.json()) as SessionDto[];

        if (!isMounted) {
          return;
        }

        const activeSessions = rawData.filter(
          (session) => session.sessionState === 0,
        );

        setSessions(activeSessions);
      } catch (error) {
        if (!isMounted) {
          return;
        }

        const message =
          error instanceof Error
            ? error.message
            : "Failed to load active sessions.";

        setPageError(message);
        setSessions([]);
      } finally {
        if (isMounted) {
          setLoading(false);
        }
      }
    };

    void loadSessions();

    return () => {
      isMounted = false;
    };
  }, [auth, navigate]);

  const handleInvalidateSession = async (sessionID: string) => {
    setPageError(null);
    setPendingSessionID(sessionID);

    try {
      const response = await fetch(
        `${API_BASE_URL}/auth/sessions/${sessionID}`,
        {
          method: "DELETE",
          headers: getAuthHeaders(auth),
        },
      );

     if (!response.ok) {
        const errorCode = getErrorCode(response.status);

        if (errorCode === "auth-required") {
          clearStoredAuth();
          setPageError(getFriendlyErrorMessage(errorCode));
          setSessions([]);

          setTimeout(() => {
            navigate("/login", { replace: true });
          }, 1200);

          return;
        }

        if (response.status === 404) {
          window.location.reload();
          return;
        }

        throw new Error(getFriendlyErrorMessage(errorCode));
      }

      window.location.reload();

      setSessions((currentSessions) =>
        currentSessions.filter((session) => session.sessionId !== sessionID),
      );
    } catch (error) {
      const message =
        error instanceof Error
          ? error.message
          : "Failed to invalidate the selected session.";

      setPageError(message);
    } finally {
      setPendingSessionID(null);
    }
  };

  return (
    <main className="page">
      <section className="container">
        <div className="background-glow background-glow-left" />
        <div className="background-glow background-glow-right" />

        <div className="card page-card-shell">
          <div className="content sessions-content">
            <div className="page-topbar">
              <Link
                to="/settings"
                className="button button-secondary page-nav-button"
              >
                Back to settings
              </Link>
            </div>

            <div className="content-centered sessions-header">
              <h1 className="title sessions-title">Active sessions</h1>
              <p className="text sessions-text">
                Review devices currently signed in to your account and invalidate
                any session you do not recognize.
              </p>
            </div>

            {loading ? (
              <div className="state-card">
                <p className="state-title">Loading sessions...</p>
                <p className="state-text">
                  We are retrieving your currently active sessions.
                </p>
              </div>
            ) : pageError ? (
              <div className="state-card">
                <p className="state-title">Could not load sessions</p>
                <p className="state-text">{pageError}</p>
              </div>
            ) : sessions.length === 0 ? (
              <div className="state-card">
                <p className="state-title">No active sessions found</p>
                <p className="state-text">
                  There are no active sessions available to display right now.
                </p>
              </div>
            ) : (
              <section className="sessions-list" aria-label="Active sessions list">
                {sessions.map((session) => (
                  <article key={session.sessionId} className="session-item">
                    <div className="session-item-main">
                      <div className="session-item-row">
                        <span className="session-label">Session ID</span>
                        <span className="session-value">{session.sessionId}</span>
                      </div>

                      <div className="session-item-row">
                        <span className="session-label">Device type</span>
                        <span className="session-value">
                          {session.deviceInfo ?? "Unknown device"}
                        </span>
                      </div>

                      <div className="session-item-row">
                        <span className="session-label">IP address</span>
                        <span className="session-value">
                          {session.ipAddress ?? "Unknown IP"}
                        </span>
                      </div>

                      <div className="session-item-row">
                        <span className="session-label">Start date</span>
                        <span className="session-value">
                          {formatDate(session.createdAt)}
                        </span>
                      </div>
                    </div>

                    <div className="session-item-actions">
                      <button
                        type="button"
                        className="button button-primary session-invalidate-button"
                        onClick={() => void handleInvalidateSession(session.sessionId)}
                        disabled={pendingSessionID === session.sessionId}
                      >
                        {pendingSessionID === session.sessionId
                          ? "Invalidating..."
                          : "Invalidate"}
                      </button>
                    </div>
                  </article>
                ))}
              </section>
            )}
          </div>
        </div>
      </section>
    </main>
  );
}
