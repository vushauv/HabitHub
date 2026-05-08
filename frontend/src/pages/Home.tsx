import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import "./Home.css";
import "../App.css";
import { API_BASE_URL } from "../services/User";
import {
  clearStoredAuth,
  getDashboardPathForUser,
  getStoredAuth,
  type StoredAuth,
} from "../services/Auth";
import { useCurrentUser } from "../hooks/useCurrentUser";

type LogoutErrorResponse = {
  message?: string | null;
};

function getLogoutErrorMessage(responseText: string, status: number): string {
  const fallbackMessage = `Logout failed (${status}).`;

  if (!responseText) {
    return fallbackMessage;
  }

  try {
    const parsedResponse = JSON.parse(responseText) as LogoutErrorResponse;
    return parsedResponse.message ?? fallbackMessage;
  } catch {
    return responseText;
  }
}

async function logoutCurrentSession(sessionId: string): Promise<void> {
  const response = await fetch(`${API_BASE_URL}/auth/logout`, {
    method: "DELETE",
    headers: {
      "Content-Type": "application/json",
      "X-Session-Id": sessionId,
    },
  });

  if (response.ok || response.status === 401) {
    return;
  }

  const responseText = await response.text().catch(() => "");
  throw new Error(getLogoutErrorMessage(responseText, response.status));
}

export default function Home() {
  const navigate = useNavigate();
  const [auth, setAuth] = useState<StoredAuth | null>(() => getStoredAuth());
  const [isLoggingOut, setIsLoggingOut] = useState(false);
  const [logoutError, setLogoutError] = useState("");
  const { currentUser, isLoading, error } = useCurrentUser(auth);

  const isLoggedIn = auth !== null;
  const dashboardPath = currentUser ? getDashboardPathForUser(currentUser) : "/login";

  useEffect(() => {
    if (!error) {
      return;
    }

    if (error.code === "auth-required" || error.code === "not-found") {
      clearStoredAuth();
      setAuth(null);
    }
  }, [error]);

  function clearAuthSession() {
    clearStoredAuth();
    setAuth(null);
  }

  async function handleLogout() {
    setLogoutError("");

    if (!auth) {
      clearAuthSession();
      navigate("/", { replace: true });
      return;
    }

    setIsLoggingOut(true);

    try {
      await logoutCurrentSession(auth.sessionId);
      clearAuthSession();
      navigate("/", { replace: true });
    } catch (error) {
      setLogoutError(
        error instanceof Error
          ? error.message
          : "Something went wrong while logging out.",
      );
    } finally {
      setIsLoggingOut(false);
    }
  }

  return (
    <main className="page container">
      <div className="background-glow background-glow-left"></div>
      <div className="background-glow background-glow-right"></div>

      <section className="card">
        <div className="content content-centered">
          <h1 className="title">HabitHub</h1>

          <p className="subtitle">Build better habits together.</p>

          <p className="text">
            Stay accountable, track shared progress, and keep your team motivated
            in one place.
          </p>

          {logoutError ? (
            <p className="form-error" role="alert">
              {logoutError}
            </p>
          ) : null}

          {error?.code === "unknown" ? (
            <p className="form-error" role="alert">
              {error.message}
            </p>
          ) : null}

          <div className="buttons">
            {isLoggedIn ? (
              <>
                {currentUser ? (
                  <Link to={dashboardPath} className="button button-primary">
                    Go to dashboard
                  </Link>
                ) : (
                  <button
                    type="button"
                    className="button button-primary"
                    disabled
                  >
                    {isLoading ? "Loading dashboard..." : "Go to dashboard"}
                  </button>
                )}

                <button
                  type="button"
                  className="button button-secondary"
                  onClick={() => void handleLogout()}
                  disabled={isLoggingOut}
                >
                  {isLoggingOut ? "Logging out..." : "Log out"}
                </button>
              </>
            ) : (
              <>
                <Link to="/register" className="button button-primary">
                  Get started
                </Link>

                <Link to="/login" className="button button-secondary">
                  Log in
                </Link>
              </>
            )}
          </div>
        </div>
      </section>
    </main>
  );
}
