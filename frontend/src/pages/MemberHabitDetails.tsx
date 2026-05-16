import { Link, useNavigate, useSearchParams } from "react-router-dom";
import { useEffect, useMemo, useState } from "react";
import "./HabitDetails.css";
import "./MemberHabitDetails.css";
import "../App.css";
import {
  clearStoredAuth,
  formatHabitEntryDate,
  formatHabitEntryDateTime,
  formatHabitEntryValue,
  formatHabitExpiryDate,
  formatHabitUnit,
  getHabit,
  getHabitEntries,
  getHabitErrorMessage,
  getStoredAuth,
  HabitRequestError,
  type HabitEntryResponseDto,
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

  return "Something went wrong while loading habit details. Please try again.";
}

export default function MemberHabitDetails() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const teamId = searchParams.get("teamId") ?? "";
  const habitId = searchParams.get("habitId") ?? "";
  const auth = useMemo(() => getStoredAuth(), []);
  const [team, setTeam] = useState<TeamDetailsDto | null>(null);
  const [habit, setHabit] = useState<HabitSummaryDto | null>(null);
  const [entries, setEntries] = useState<HabitEntryResponseDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [pageError, setPageError] = useState<string | null>(null);

  useEffect(() => {
    let isMounted = true;

    const loadDetails = async () => {
      setLoading(true);
      setPageError(null);

      if (!teamId || !habitId) {
        if (isMounted) {
          setPageError("Choose a habit first.");
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
        const [loadedTeam, loadedHabit, loadedEntries] = await Promise.all([
          getTeam(auth, teamId),
          getHabit(auth, habitId),
          getHabitEntries(auth, habitId),
        ]);

        if (!isMounted) {
          return;
        }

        setTeam(loadedTeam);
        setHabit(loadedHabit);
        setEntries(loadedEntries);
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

    void loadDetails();

    return () => {
      isMounted = false;
    };
  }, [auth, habitId, navigate, teamId]);

  return (
    <main className="page">
      <section className="container">
        <div className="background-glow background-glow-left" />
        <div className="background-glow background-glow-right" />

        <div className="card page-card-shell">
          <div className="content habit-details-content">
            <div className="page-topbar">
              <Link
                to="/"
                className="button button-secondary page-nav-button"
              >
                Home
              </Link>

              <Link
                to={`/habits-member?teamId=${encodeURIComponent(teamId)}`}
                className="button button-secondary page-nav-button"
              >
                Habits
              </Link>
            </div>

            <div className="content-centered habit-details-header">
              <h1 className="title page-title-md habit-details-title pill-title">
                Habit Details
              </h1>

              {team ? (
                <p className="text habit-details-team-name">{team.name}</p>
              ) : null}
            </div>

            {pageError ? (
              <p className="form-error page-message" role="alert">
                {pageError}
              </p>
            ) : null}

            {loading ? (
              <div className="state-card">
                <p className="state-title">Loading habit...</p>
                <p className="state-text">
                  We are retrieving your progress data.
                </p>
              </div>
            ) : habit ? (
              <>
                <section className="habit-details-grid" aria-label="Habit details">
                  <div className="habit-details-item">
                    <p className="habit-details-label">Name</p>
                    <p className="habit-details-value">{habit.name}</p>
                  </div>

                  <div className="habit-details-item">
                    <p className="habit-details-label">State</p>
                    <p className="habit-details-value">
                      {habit.habitState}
                    </p>
                  </div>

                  <div className="habit-details-item">
                    <p className="habit-details-label">Type</p>
                    <p className="habit-details-value">
                      {habit.habitType}
                    </p>
                  </div>

                  <div className="habit-details-item">
                    <p className="habit-details-label">Unit</p>
                    <p className="habit-details-value">
                      {formatHabitUnit(habit.unit)}
                    </p>
                  </div>

                  <div className="habit-details-item">
                    <p className="habit-details-label">Expiry</p>
                    <p className="habit-details-value">
                      {formatHabitExpiryDate(habit.expiryDate)}
                    </p>
                  </div>

                  <div className="habit-details-item habit-details-goal">
                    <p className="habit-details-label">Goal</p>
                    <p className="habit-details-value">
                      {habit.goal ?? "No goal"}
                    </p>
                  </div>
                </section>

                <div className="habit-details-actions">
                  {habit.habitState === "Active" ? (
                    <Link
                      to={`/member-log-habit?teamId=${encodeURIComponent(
                        teamId,
                      )}&habitId=${encodeURIComponent(habit.habitId)}`}
                      className="button button-primary"
                    >
                      Log Habit
                    </Link>
                  ) : null}

                  <Link
                    to={`/habit-leaderboard?teamId=${encodeURIComponent(
                      teamId,
                    )}&habitId=${encodeURIComponent(habit.habitId)}`}
                    className="button button-secondary"
                  >
                    Leaderboard
                  </Link>
                </div>

                <p className="habit-details-section-title">Your Progress</p>

                {entries.length === 0 ? (
                  <div className="state-card">
                    <p className="state-title">No progress entries</p>
                    <p className="state-text">
                      Your habit progress will appear here after you log it.
                    </p>
                  </div>
                ) : (
                  <section className="table-list" aria-label="Your habit progress">
                    <div className="data-table-row member-habit-progress-row data-table-head member-habit-progress-head">
                      <span>Date</span>
                      <span>Status</span>
                      <span>Value</span>
                      <span>Notes</span>
                      <span>Logged at</span>
                    </div>

                    {entries.map((entry) => (
                      <article
                        className="data-table-row member-habit-progress-row"
                        key={entry.entryId}
                      >
                        <span className="member-habit-progress-meta">
                          {formatHabitEntryDate(entry.logDate)}
                        </span>
                        <span className="member-habit-progress-meta">
                          {entry.status}
                        </span>
                        <span className="member-habit-progress-meta">
                          {formatHabitEntryValue(entry, habit)}
                        </span>
                        <span className="member-habit-progress-notes">
                          {entry.notes ?? "No notes"}
                        </span>
                        <span className="member-habit-progress-meta">
                          {formatHabitEntryDateTime(entry.loggedAt)}
                        </span>
                      </article>
                    ))}
                  </section>
                )}
              </>
            ) : null}
          </div>
        </div>
      </section>
    </main>
  );
}
