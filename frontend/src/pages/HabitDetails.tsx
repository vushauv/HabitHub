import { Link, useNavigate, useSearchParams } from "react-router-dom";
import { useEffect, useMemo, useState } from "react";
import "./HabitDetails.css";
import "../App.css";
import {
  clearStoredAuth,
  formatHabitExpiryDate,
  formatHabitState,
  formatHabitType,
  formatHabitUnit,
  getHabit,
  getHabitErrorMessage,
  getHabitLeaderboard,
  getStoredAuth,
  HabitRequestError,
  type HabitSummaryDto,
  type LeaderboardResponseDto,
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

function formatLeaderboardValue(
  row: LeaderboardResponseDto,
  habit: HabitSummaryDto,
): string {
  if (row.totalValue === null) {
    return "No value";
  }

  const unit = formatHabitUnit(habit.unit);

  return habit.habitType === 1 && habit.unit !== null
    ? `${row.totalValue} ${unit}`
    : `${row.totalValue}`;
}

export default function HabitDetails() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const teamId = searchParams.get("teamId") ?? "";
  const habitId = searchParams.get("habitId") ?? "";
  const auth = useMemo(() => getStoredAuth(), []);
  const [team, setTeam] = useState<TeamDetailsDto | null>(null);
  const [habit, setHabit] = useState<HabitSummaryDto | null>(null);
  const [leaderboard, setLeaderboard] = useState<LeaderboardResponseDto[]>([]);
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
        const [loadedTeam, loadedHabit, loadedLeaderboard] = await Promise.all([
          getTeam(auth, teamId),
          getHabit(auth, habitId),
          getHabitLeaderboard(auth, habitId),
        ]);

        if (!isMounted) {
          return;
        }

        setTeam(loadedTeam);
        setHabit(loadedHabit);
        setLeaderboard(loadedLeaderboard);
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

  const canEdit = habit?.habitState === 0;

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
                to={`/habits-creator?teamId=${encodeURIComponent(teamId)}`}
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
                  We are retrieving details and leaderboard data.
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
                      {formatHabitState(habit.habitState)}
                    </p>
                  </div>

                  <div className="habit-details-item">
                    <p className="habit-details-label">Type</p>
                    <p className="habit-details-value">
                      {formatHabitType(habit.habitType)}
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

                {canEdit ? (
                  <div className="habit-details-actions">
                    <Link
                      to={`/edit-habit?teamId=${encodeURIComponent(
                        teamId,
                      )}&habitId=${encodeURIComponent(habit.habitId)}`}
                      className="button button-primary"
                    >
                      Edit Habit
                    </Link>
                  </div>
                ) : null}

                {leaderboard.length === 0 ? (
                  <div className="state-card">
                    <p className="state-title">No leaderboard entries</p>
                    <p className="state-text">
                      Members have not logged progress for this habit yet.
                    </p>
                  </div>
                ) : (
                  <section className="table-list" aria-label="Habit leaderboard">
                    <div className="data-table-row habit-details-table-row data-table-head habit-details-table-head">
                      <span>Rank</span>
                      <span>Member</span>
                      <span>Total</span>
                      <span>Logs</span>
                    </div>

                    {leaderboard.map((row) => (
                      <article
                        className="data-table-row habit-details-table-row"
                        key={row.memberId}
                      >
                        <span className="habit-details-rank">#{row.rank}</span>
                        <span className="habit-details-member">
                          {row.memberName}
                        </span>
                        <span className="habit-details-meta">
                          {formatLeaderboardValue(row, habit)}
                        </span>
                        <span className="habit-details-meta">
                          {row.loggedCount}
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
