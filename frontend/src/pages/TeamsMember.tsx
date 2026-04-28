import { Link, useNavigate } from "react-router-dom";
import { useEffect, useMemo, useState } from "react";
import "./TeamsMember.css";
import "../App.css";
import {
  clearStoredAuth,
  getStoredAuth,
  getTeamErrorMessage,
  getTeams,
  leaveTeam,
  TeamRequestError,
  type TeamSummaryDto,
} from "../services/Team";

function resolveErrorMessage(error: unknown): string {
  if (error instanceof TeamRequestError) {
    return error.message || getTeamErrorMessage(error.code);
  }

  return "Something went wrong while loading your memberships. Please try again.";
}

export default function TeamsMember() {
  const navigate = useNavigate();
  const auth = useMemo(() => getStoredAuth(), []);
  const [teams, setTeams] = useState<TeamSummaryDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [pageError, setPageError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [pendingTeamId, setPendingTeamId] = useState<string | null>(null);

  useEffect(() => {
    let isMounted = true;

    const loadTeams = async () => {
      setLoading(true);
      setPageError(null);

      if (!auth?.isLoggedIn || !auth.sessionId) {
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

      try {
        const loadedTeams = await getTeams(auth);

        if (!isMounted) {
          return;
        }

        setTeams(loadedTeams);
      } catch (error) {
        if (!isMounted) {
          return;
        }

        if (error instanceof TeamRequestError && error.code === "auth-required") {
          clearStoredAuth();
          setPageError(getTeamErrorMessage(error.code));

          setTimeout(() => {
            navigate("/login", { replace: true });
          }, 1200);
        } else {
          setPageError(resolveErrorMessage(error));
        }
      } finally {
        if (isMounted) {
          setLoading(false);
        }
      }
    };

    void loadTeams();

    return () => {
      isMounted = false;
    };
  }, [auth, navigate]);

  const handleLeaveTeam = async (team: TeamSummaryDto) => {
    const confirmed = window.confirm(`Leave team "${team.name}"?`);

    if (!confirmed) {
      return;
    }

    setPendingTeamId(team.teamId);
    setPageError(null);
    setSuccessMessage(null);

    try {
      await leaveTeam(auth, team.teamId);

      setTeams((currentTeams) =>
        currentTeams.filter((currentTeam) => currentTeam.teamId !== team.teamId),
      );

      setSuccessMessage("You left the team.");
    } catch (error) {
      if (error instanceof TeamRequestError && error.code === "auth-required") {
        clearStoredAuth();
        setPageError(getTeamErrorMessage(error.code));

        setTimeout(() => {
          navigate("/login", { replace: true });
        }, 1200);
      } else {
        setPageError(resolveErrorMessage(error));
      }
    } finally {
      setPendingTeamId(null);
    }
  };

  return (
    <main className="page">
      <section className="container">
        <div className="background-glow background-glow-left" />
        <div className="background-glow background-glow-right" />

        <div className="card teams-member-card-shell">
          <div className="content teams-member-content">
            <div className="teams-member-topbar">
              <Link
                to="/"
                className="button button-secondary teams-member-nav-button"
              >
                Home
              </Link>
            </div>

            <div className="content-centered teams-member-header">
              <h1 className="title teams-member-title">Your Memberships</h1>

              <Link
                to="/join-team"
                className="button button-primary teams-member-join-button"
              >
                Join a New Team
              </Link>
            </div>

            {pageError ? (
              <p className="form-error teams-member-message" role="alert">
                {pageError}
              </p>
            ) : null}

            {successMessage ? (
              <p className="alert-success">{successMessage}</p>
            ) : null}

            {loading ? (
              <div className="teams-member-state-card">
                <p className="teams-member-state-title">
                  Loading memberships...
                </p>
                <p className="teams-member-state-text">
                  We are retrieving the teams you belong to.
                </p>
              </div>
            ) : teams.length === 0 ? (
              <div className="teams-member-state-card">
                <p className="teams-member-state-title">
                  No memberships found
                </p>
                <p className="teams-member-state-text">
                  Join a team with an invite code to start tracking habits.
                </p>
              </div>
            ) : (
              <section
                className="teams-member-table"
                aria-label="Your memberships"
              >
                <div className="teams-member-table-row teams-member-table-head">
                  <span>Name</span>
                  <span>Habits</span>
                  <span>Chat</span>
                  <span></span>
                </div>

                {teams.map((team) => {
                  const isLeaving = pendingTeamId === team.teamId;

                  return (
                    <article className="teams-member-table-row" key={team.teamId}>
                      <span className="teams-member-team-name">{team.name}</span>

                      <button
                        type="button"
                        className="button button-secondary teams-member-row-button"
                        disabled
                      >
                        Show Habits
                      </button>

                      <button
                        type="button"
                        className="button button-secondary teams-member-row-button"
                        disabled
                      >
                        Show Chat
                      </button>

                      <button
                        type="button"
                        className="button button-secondary teams-member-row-button teams-member-danger-button"
                        onClick={() => void handleLeaveTeam(team)}
                        disabled={isLeaving}
                      >
                        {isLeaving ? "Leaving..." : "Leave the team"}
                      </button>
                    </article>
                  );
                })}
              </section>
            )}
          </div>
        </div>
      </section>
    </main>
  );
}
