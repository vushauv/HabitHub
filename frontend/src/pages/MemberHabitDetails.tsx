import { Link, useNavigate, useParams } from "react-router-dom";
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
import {
  changeMyReminder,
  formatReminderTime,
  getMyReminder,
  getReminderErrorMessage,
  ReminderRequestError,
  type MyReminderResponseDto,
} from "../services/Reminder";

function resolveErrorMessage(error: unknown): string {
  if (error instanceof ReminderRequestError) {
    return error.message || getReminderErrorMessage(error.code);
  }

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
  const { teamId = "", habitId = "" } = useParams();
  const auth = useMemo(() => getStoredAuth(), []);
  const [team, setTeam] = useState<TeamDetailsDto | null>(null);
  const [habit, setHabit] = useState<HabitSummaryDto | null>(null);
  const [entries, setEntries] = useState<HabitEntryResponseDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [pageError, setPageError] = useState<string | null>(null);
  const [myReminder, setMyReminder] = useState<MyReminderResponseDto | null>(null);
  const [reminderPending, setReminderPending] = useState(false);
  const [reminderMessage, setReminderMessage] = useState<string | null>(null);
  const [reminderError, setReminderError] = useState<string | null>(null);

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
        const [loadedTeam, loadedHabit, loadedEntries, loadedReminder] = await Promise.all([
          getTeam(auth, teamId),
          getHabit(auth, habitId),
          getHabitEntries(auth, habitId),
          getMyReminder(auth, habitId),
        ]);

        if (!isMounted) {
          return;
        }

        setTeam(loadedTeam);
        setHabit(loadedHabit);
        setEntries(loadedEntries);
        setMyReminder(loadedReminder);
      } catch (error) {
        if (!isMounted) {
          return;
        }

        if (
          (error instanceof HabitRequestError ||
            error instanceof TeamRequestError ||
            error instanceof ReminderRequestError) &&
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

  const handleToggleReminder = async () => {
    if (!habit || !myReminder) {
      return;
    }

    const nextEnabled = !myReminder.enabled;

    setReminderPending(true);
    setReminderMessage(null);
    setReminderError(null);

    try {
      const response = await changeMyReminder(
        auth,
        habit.habitId,
        nextEnabled,
      );

      setMyReminder(response);
      setReminderMessage(
        response.enabled ? "Reminder enabled." : "Reminder disabled.",
      );
    } catch (error) {
      if (error instanceof ReminderRequestError && error.code === "auth-required") {
        clearStoredAuth();
        setReminderError(getReminderErrorMessage(error.code));

        setTimeout(() => {
          navigate("/login", { replace: true });
        }, 1200);

        return;
      }

      setReminderError(resolveErrorMessage(error));
    } finally {
      setReminderPending(false);
    }
  };

  const canToggleReminder = Boolean(
    habit?.habitState === "Active" &&
      myReminder?.reminderTime &&
      !reminderPending,
  );
  const hasReminderTime = Boolean(myReminder?.reminderTime);
  const reminderEnabled = Boolean(myReminder?.enabled && hasReminderTime);
  const reminderStatusText = hasReminderTime
    ? reminderEnabled
      ? "Enabled"
      : "Disabled"
    : "Not Set";

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
                to={`/member/teams/${encodeURIComponent(teamId)}/habits`}
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

                <section
                  className="habit-details-reminder-card"
                  aria-label="Reminder settings"
                >
                  <div className="habit-details-reminder-top">
                    <div>
                      <p className="habit-details-section-title">Reminder</p>
                      <p className="habit-details-reminder-text">
                        {myReminder?.reminderTime
                          ? formatReminderTime(myReminder.reminderTime)
                          : "No reminder time set"}
                      </p>
                    </div>

                    <span
                      className={`habit-details-reminder-status ${
                        reminderEnabled
                          ? "habit-details-reminder-status-enabled"
                          : ""
                      }`}
                    >
                      {reminderStatusText}
                    </span>
                  </div>

                  <div className="habit-details-reminder-actions">
                    <button
                      type="button"
                      className="button button-secondary"
                      onClick={() => void handleToggleReminder()}
                      disabled={!canToggleReminder}
                    >
                      {reminderPending
                        ? "Updating..."
                        : reminderEnabled
                          ? "Disable Reminder"
                          : "Enable Reminder"}
                    </button>
                  </div>

                  {reminderError ? (
                    <p className="form-error page-message" role="alert">
                      {reminderError}
                    </p>
                  ) : null}

                  {reminderMessage ? (
                    <p className="alert-success">{reminderMessage}</p>
                  ) : null}
                </section>

                <div className="habit-details-actions">
                  {habit.habitState === "Active" ? (
                    <Link
                      to={`/member/teams/${encodeURIComponent(
                        teamId,
                      )}/habits/${encodeURIComponent(habit.habitId)}/log`}
                      className="button button-primary"
                    >
                      Log Habit
                    </Link>
                  ) : null}

                  <Link
                    to={`/teams/${encodeURIComponent(
                      teamId,
                    )}/habits/${encodeURIComponent(habit.habitId)}/leaderboard`}
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
