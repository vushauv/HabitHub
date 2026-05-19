import { Link, useNavigate, useParams } from "react-router-dom";
import { useEffect, useMemo, useState } from "react";
import "./MemberHabits.css";
import "../App.css";
import {
  clearStoredAuth,
  formatHabitExpiryDate,
  formatHabitUnit,
  getHabitErrorMessage,
  getStoredAuth,
  getTeamHabits,
  HabitRequestError,
  type HabitStateFilter,
  type HabitSummaryDto,
} from "../services/Habit";
import {
  getTeam,
  getTeamErrorMessage,
  TeamRequestError,
  type TeamDetailsDto,
} from "../services/Team";

function resolveErrorMessage(error: unknown): string {
  if (error instanceof HabitRequestError) {
    return error.message || getHabitErrorMessage(error.code);
  }

  if (error instanceof TeamRequestError) {
    return error.message || getTeamErrorMessage(error.code);
  }

  return "Something went wrong while loading habits. Please try again.";
}

export default function MemberHabits() {
  const navigate = useNavigate();
  const { teamId = "" } = useParams();
  const auth = useMemo(() => getStoredAuth(), []);
  const [team, setTeam] = useState<TeamDetailsDto | null>(null);
  const [habits, setHabits] = useState<HabitSummaryDto[]>([]);
  const [selectedState, setSelectedState] = useState<HabitStateFilter>("Active");
  const [loading, setLoading] = useState(true);
  const [pageError, setPageError] = useState<string | null>(null);

  useEffect(() => {
    let isMounted = true;

    const loadHabits = async () => {
      setLoading(true);
      setPageError(null);

      if (!teamId) {
        if (isMounted) {
          setPageError("Choose a team first.");
          setLoading(false);
        }

        return;
      }

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

      try {
        const [loadedTeam, loadedHabits] = await Promise.all([
          getTeam(auth, teamId),
          getTeamHabits(auth, teamId, selectedState),
        ]);

        if (!isMounted) {
          return;
        }

        setTeam(loadedTeam);
        setHabits(loadedHabits);
      } catch (error) {
        if (!isMounted) {
          return;
        }

        if (
          (error instanceof HabitRequestError ||
            error instanceof TeamRequestError) &&
          error.code === "auth-required"
        ) {
          clearStoredAuth();
          setPageError(getHabitErrorMessage(error.code));

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

    void loadHabits();

    return () => {
      isMounted = false;
    };
  }, [auth, navigate, selectedState, teamId]);

  const emptyStateText =
    selectedState === "Active"
      ? "This team does not have any active habits right now."
      : "Archived habits for this team will appear here.";

  return (
    <main className="page">
      <section className="container">
        <div className="background-glow background-glow-left" />
        <div className="background-glow background-glow-right" />

        <div className="card page-card-shell">
          <div className="content member-habits-content">
            <div className="page-topbar">
              <Link
                to="/"
                className="button button-secondary page-nav-button"
              >
                Home
              </Link>

              <Link
                to="/member/teams"
                className="button button-secondary page-nav-button"
              >
                Teams
              </Link>
            </div>

            <div className="content-centered member-habits-header">
              <h1 className="title page-title member-habits-title pill-title">
                Team Habits
              </h1>

              {team ? (
                <p className="text member-habits-team-name">{team.name}</p>
              ) : null}
            </div>

            <div
              className="member-habits-state-toggle"
              role="tablist"
              aria-label="Habit state"
            >
              {(["Active", "Archived"] as HabitStateFilter[]).map((state) => (
                <button
                  key={state}
                  type="button"
                  className={`button button-secondary member-habits-state-button ${
                    selectedState === state
                      ? "member-habits-state-button-active"
                      : ""
                  }`}
                  onClick={() => setSelectedState(state)}
                  aria-selected={selectedState === state}
                  role="tab"
                >
                  {state}
                </button>
              ))}
            </div>

            {pageError ? (
              <p className="form-error page-message" role="alert">
                {pageError}
              </p>
            ) : null}

            {loading ? (
              <div className="state-card">
                <p className="state-title">Loading habits...</p>
                <p className="state-text">
                  We are retrieving this team&apos;s habits.
                </p>
              </div>
            ) : habits.length === 0 ? (
              <div className="state-card">
                <p className="state-title">No {selectedState.toLowerCase()} habits found</p>
                <p className="state-text">{emptyStateText}</p>
              </div>
            ) : (
              <section className="table-list" aria-label={`${selectedState} habits`}>
                <div className="data-table-row member-habits-table-row data-table-head member-habits-table-head">
                  <span>Name</span>
                  <span>Type</span>
                  <span>Unit</span>
                  <span>Expiry</span>
                  <span>Goal</span>
                  <span></span>
                </div>

                {habits.map((habit) => (
                  <article
                    className="data-table-row member-habits-table-row"
                    key={habit.habitId}
                  >
                    <span className="member-habits-name">{habit.name}</span>
                    <span className="member-habits-meta">
                      {habit.habitType}
                    </span>
                    <span className="member-habits-meta">
                      {formatHabitUnit(habit.unit)}
                    </span>
                    <span className="member-habits-meta">
                      {formatHabitExpiryDate(habit.expiryDate)}
                    </span>
                    <span className="member-habits-goal">
                      {habit.goal ?? "No goal"}
                    </span>

                    <div className="member-habits-row-actions">
                      <Link
                        to={`/member/teams/${encodeURIComponent(
                          teamId,
                        )}/habits/${encodeURIComponent(habit.habitId)}/details`}
                        className="button button-secondary table-row-button"
                      >
                        Details
                      </Link>

                      {habit.habitState === "Active" ? (
                        <Link
                          to={`/member/teams/${encodeURIComponent(
                            teamId,
                          )}/habits/${encodeURIComponent(habit.habitId)}/log`}
                          className="button button-secondary table-row-button"
                        >
                          Log
                        </Link>
                      ) : null}

                      <Link
                        to={`/teams/${encodeURIComponent(
                          teamId,
                        )}/habits/${encodeURIComponent(habit.habitId)}/leaderboard`}
                        className="button button-secondary table-row-button"
                      >
                        Leaderboard
                      </Link>
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
