import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import "./Home.css";
import "../App.css";
import { API_BASE_URL, type AccountType } from "../services/User";

type StoredAuth = {
  isLoggedIn?: boolean;
  userType?: AccountType;
  sessionId?: string | null;
  userId?: string | null;
};

type LogoutErrorResponse = {
  message?: string | null;
};

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

  const isLoggedIn = auth?.isLoggedIn === true;

  const dashboardPath =
    auth?.userType === "Creator"
      ? "/main-creator"
      : auth?.userType === "Member"
      ? "/main-member"
      : "/login";

  function clearAuthSession() {
    clearStoredAuth();
    setAuth(null);
  }

  async function handleLogout() {
    setLogoutError("");

    if (!auth?.isLoggedIn || !auth.sessionId) {
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

          <div className="buttons">
            {isLoggedIn ? (
              <>
                <Link to={dashboardPath} className="button button-primary">
                  Go to dashboard
                </Link>

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
