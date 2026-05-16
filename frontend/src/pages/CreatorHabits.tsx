import { Link, useNavigate, useSearchParams } from "react-router-dom";
import { useEffect, useMemo, useState } from "react";
import "./CreatorHabits.css";
import "../App.css";
import {
  archiveHabit,
  clearStoredAuth,
  deleteHabit,
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

type PendingAction = {
  habitId: string;
  action: "archive" | "delete";
} | null;

function resolveErrorMessage(error: unknown): string {
  if (error instanceof HabitRequestError) {
    return error.message || getHabitErrorMessage(error.code);
  }

  if (error instanceof TeamRequestError) {
    return error.message || getTeamErrorMessage(error.code);
  }

  return "Something went wrong while loading habits. Please try again.";
}

export default function CreatorHabits() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const teamId = searchParams.get("teamId") ?? "";
  const auth = useMemo(() => getStoredAuth(), []);
  const [team, setTeam] = useState<TeamDetailsDto | null>(null);
  const [habits, setHabits] = useState<HabitSummaryDto[]>([]);
  const [selectedState, setSelectedState] = useState<HabitStateFilter>("Active");
  const [loading, setLoading] = useState(true);
  const [pageError, setPageError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [pendingAction, setPendingAction] = useState<PendingAction>(null);

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

  const handleArchiveHabit = async (habit: HabitSummaryDto) => {
    const confirmed = window.confirm(`Archive habit "${habit.name}"?`);

    if (!confirmed) {
      return;
    }

    setPendingAction({ habitId: habit.habitId, action: "archive" });
    setPageError(null);
    setSuccessMessage(null);

    try {
      await archiveHabit(auth, habit.habitId);

      setHabits((currentHabits) =>
        currentHabits.filter(
          (currentHabit) => currentHabit.habitId !== habit.habitId,
        ),
      );

      setSuccessMessage("Habit archived.");
    } catch (error) {
      handleActionError(error);
    } finally {
      setPendingAction(null);
    }
  };

  const handleDeleteHabit = async (habit: HabitSummaryDto) => {
    const confirmed = window.confirm(`Delete habit "${habit.name}" permanently?`);

    if (!confirmed) {
      return;
    }

    setPendingAction({ habitId: habit.habitId, action: "delete" });
    setPageError(null);
    setSuccessMessage(null);

    try {
      await deleteHabit(auth, habit.habitId);

      setHabits((currentHabits) =>
        currentHabits.filter(
          (currentHabit) => currentHabit.habitId !== habit.habitId,
        ),
      );

      setSuccessMessage("Habit deleted.");
    } catch (error) {
      handleActionError(error);
    } finally {
      setPendingAction(null);
    }
  };

  const handleActionError = (error: unknown) => {
    if (error instanceof HabitRequestError && error.code === "auth-required") {
      clearStoredAuth();
      setPageError(getHabitErrorMessage(error.code));

      setTimeout(() => {
        navigate("/login", { replace: true });
      }, 1200);

      return;
    }

    setPageError(resolveErrorMessage(error));
  };

  const emptyStateText =
    selectedState === "Active"
      ? "Create a habit so members can start tracking progress."
      : "Archived habits for this team will appear here.";

  return (
    <main className="page">
      <section className="container">
        <div className="background-glow background-glow-left" />
        <div className="background-glow background-glow-right" />

        <div className="card page-card-shell">
          <div className="content creator-habits-content">
            <div className="page-topbar">
              <Link
                to="/"
                className="button button-secondary page-nav-button"
              >
                Home
              </Link>

              <Link
                to="/teams-creator"
                className="button button-secondary page-nav-button"
              >
                Teams
              </Link>
            </div>

            <div className="content-centered creator-habits-header">
              <h1 className="title page-title creator-habits-title pill-title">
                Team Habits
              </h1>

              {team ? (
                <p className="text creator-habits-team-name">{team.name}</p>
              ) : null}

              <div className="creator-habits-actions">
                <Link
                  to={`/create-habit?teamId=${encodeURIComponent(teamId)}`}
                  className="button button-primary"
                >
                  Create Habit
                </Link>
              </div>
            </div>

            <div
              className="creator-habits-state-toggle"
              role="tablist"
              aria-label="Habit state"
            >
              {(["Active", "Archived"] as HabitStateFilter[]).map((state) => (
                <button
                  key={state}
                  type="button"
                  className={`button button-secondary creator-habits-state-button ${
                    selectedState === state
                      ? "creator-habits-state-button-active"
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

            {successMessage ? (
              <p className="alert-success">{successMessage}</p>
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
                <div className="data-table-row creator-habits-table-row data-table-head creator-habits-table-head">
                  <span>Name</span>
                  <span>Type</span>
                  <span>Unit</span>
                  <span>Expiry</span>
                  <span>Goal</span>
                  <span></span>
                </div>

                {habits.map((habit) => {
                  const isArchiving =
                    pendingAction?.habitId === habit.habitId &&
                    pendingAction.action === "archive";
                  const isDeleting =
                    pendingAction?.habitId === habit.habitId &&
                    pendingAction.action === "delete";
                  const detailsLink = `/habit-details?teamId=${encodeURIComponent(
                    teamId,
                  )}&habitId=${encodeURIComponent(habit.habitId)}`;

                  return (
                    <article
                      className="data-table-row creator-habits-table-row"
                      key={habit.habitId}
                    >
                      <span className="creator-habits-name">{habit.name}</span>
                      <span className="creator-habits-meta">
                        {habit.habitType}
                      </span>
                      <span className="creator-habits-meta">
                        {formatHabitUnit(habit.unit)}
                      </span>
                      <span className="creator-habits-meta">
                        {formatHabitExpiryDate(habit.expiryDate)}
                      </span>
                      <span className="creator-habits-goal">
                        {habit.goal ?? "No goal"}
                      </span>

                      <div className="creator-habits-row-actions">
                        <Link
                          to={detailsLink}
                          className="button button-secondary table-row-button"
                        >
                          Details
                        </Link>

                        <Link
                          to={`/habit-leaderboard?teamId=${encodeURIComponent(
                            teamId,
                          )}&habitId=${encodeURIComponent(habit.habitId)}`}
                          className="button button-secondary table-row-button"
                        >
                          Leaderboard
                        </Link>

                        {selectedState === "Active" ? (
                          <Link
                            to={`/edit-habit?teamId=${encodeURIComponent(
                              teamId,
                            )}&habitId=${encodeURIComponent(habit.habitId)}`}
                            className="button button-secondary table-row-button"
                          >
                            Edit
                          </Link>
                        ) : null}

                        {selectedState === "Active" ? (
                          <button
                            type="button"
                            className="button button-secondary table-row-button"
                            onClick={() => void handleArchiveHabit(habit)}
                            disabled={isArchiving}
                          >
                            {isArchiving ? "Archiving..." : "Archive"}
                          </button>
                        ) : null}

                        <button
                          type="button"
                          className="button button-secondary table-row-button creator-habits-danger-button"
                          onClick={() => void handleDeleteHabit(habit)}
                          disabled={isDeleting}
                        >
                          {isDeleting ? "Deleting..." : "Delete"}
                        </button>
                      </div>
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
