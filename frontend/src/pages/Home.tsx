import { Link, useNavigate } from "react-router-dom";
import "./Home.css";
import "../App.css";

type AccountType = "Creator" | "Member";

type StoredAuth = {
  isLoggedIn?: boolean;
  userType?: AccountType;
  sessionId?: string | null;
  userId?: string | null;
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

export default function Home() {
  const navigate = useNavigate();
  const auth = getStoredAuth();

  const isLoggedIn = auth?.isLoggedIn === true;

  const dashboardPath =
    auth?.userType === "Creator"
      ? "/main-creator"
      : auth?.userType === "Member"
      ? "/main-member"
      : "/login";

  function handleLogout() {
    localStorage.removeItem("habithubAuth");
    navigate("/");
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

          <div className="buttons">
            {isLoggedIn ? (
              <>
                <Link to={dashboardPath} className="button button-primary">
                  Go to dashboard
                </Link>

                <button
                  type="button"
                  className="button button-secondary"
                  onClick={handleLogout}
                >
                  Log out
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