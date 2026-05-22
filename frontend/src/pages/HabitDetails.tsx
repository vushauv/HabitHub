import { Link, useNavigate, useParams } from "react-router-dom";
import { useEffect, useMemo, useState, type FormEvent } from "react";
import "./HabitDetails.css";
import "../App.css";
import {
  clearStoredAuth,
  formatHabitExpiryDate,
  formatHabitUnit,
  getHabit,
  getHabitErrorMessage,
  getStoredAuth,
  HabitRequestError,
  type HabitSummaryDto,
} from "../services/Habit";
import {
  getTeam,
  getTeamErrorMessage,
  TeamRequestError,
  type TeamDetailsDto,
} from "../services/Team";
import {
  clearHabitReminder,
  formatReminderTime,
  formatReminderTimeInputValue,
  getReminderErrorMessage,
  ReminderRequestError,
  setHabitReminder,
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

export default function HabitDetails() {
  const navigate = useNavigate();
  const { teamId = "", habitId = "" } = useParams();
  const auth = useMemo(() => getStoredAuth(), []);
  const [team, setTeam] = useState<TeamDetailsDto | null>(null);
  const [habit, setHabit] = useState<HabitSummaryDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [pageError, setPageError] = useState<string | null>(null);
  const [reminderTime, setReminderTime] = useState("");
  const [savedReminderTime, setSavedReminderTime] = useState<string | null>(null);
  const [reminderAction, setReminderAction] = useState<"save" | "clear" | null>(null);
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
        const [loadedTeam, loadedHabit] = await Promise.all([
          getTeam(auth, teamId),
          getHabit(auth, habitId),
        ]);

        if (!isMounted) {
          return;
        }

        setTeam(loadedTeam);
        setHabit(loadedHabit);
        setSavedReminderTime(loadedHabit.reminderTime ?? null);
        setReminderTime(formatReminderTimeInputValue(loadedHabit.reminderTime ?? null));
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

  const handleSetReminder = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    if (!habit || !reminderTime) {
      return;
    }

    setReminderAction("save");
    setReminderMessage(null);
    setReminderError(null);

    try {
      const response = await setHabitReminder(
        auth,
        habit.habitId,
        reminderTime,
      );

      setSavedReminderTime(response.reminderTime);
      setReminderTime(formatReminderTimeInputValue(response.reminderTime));
      setReminderMessage("Reminder saved.");
    } catch (error) {
      handleReminderActionError(error);
    } finally {
      setReminderAction(null);
    }
  };

  const handleClearReminder = async () => {
    if (!habit) {
      return;
    }

    setReminderAction("clear");
    setReminderMessage(null);
    setReminderError(null);

    try {
      await clearHabitReminder(auth, habit.habitId);

      setSavedReminderTime(null);
      setReminderTime("");
      setReminderMessage("Reminder cleared.");
    } catch (error) {
      handleReminderActionError(error);
    } finally {
      setReminderAction(null);
    }
  };

  const handleReminderActionError = (error: unknown) => {
    if (error instanceof ReminderRequestError && error.code === "auth-required") {
      clearStoredAuth();
      setReminderError(getReminderErrorMessage(error.code));

      setTimeout(() => {
        navigate("/login", { replace: true });
      }, 1200);

      return;
    }

    setReminderError(resolveErrorMessage(error));
  };

  const canEdit = habit?.habitState === "Active";
  const canChangeReminder = Boolean(canEdit && habit && !reminderAction);

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
                to={`/creator/teams/${encodeURIComponent(teamId)}/habits`}
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
                  We are retrieving habit details.
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
                      {savedReminderTime ? (
                        <p className="habit-details-reminder-text">
                          {formatReminderTime(savedReminderTime)}
                        </p>
                      ) : null}
                    </div>
                  </div>

                  <form
                    className="habit-details-reminder-form"
                    onSubmit={(event) => void handleSetReminder(event)}
                  >
                    <label className="form-field">
                      <span className="form-label">Reminder time</span>
                      <input
                        type="time"
                        className="form-input"
                        value={reminderTime}
                        onChange={(event) => setReminderTime(event.target.value)}
                        disabled={!canChangeReminder}
                        required
                      />
                    </label>

                    <div className="habit-details-reminder-actions">
                      <button
                        type="submit"
                        className="button button-primary"
                        disabled={!canChangeReminder || !reminderTime}
                      >
                        {reminderAction === "save"
                          ? "Saving..."
                          : "Save Reminder"}
                      </button>

                      <button
                        type="button"
                        className="button button-secondary"
                        onClick={() => void handleClearReminder()}
                        disabled={!canChangeReminder}
                      >
                        {reminderAction === "clear"
                          ? "Clearing..."
                          : "Clear Reminder"}
                      </button>
                    </div>
                  </form>

                  {reminderError ? (
                    <p className="form-error page-message" role="alert">
                      {reminderError}
                    </p>
                  ) : null}

                  {reminderMessage ? (
                    <p className="alert-success">{reminderMessage}</p>
                  ) : null}
                </section>

                {canEdit ? (
                  <div className="habit-details-actions">
                    <Link
                      to={`/creator/teams/${encodeURIComponent(
                        teamId,
                      )}/habits/${encodeURIComponent(habit.habitId)}/edit`}
                      className="button button-primary"
                    >
                      Edit Habit
                    </Link>

                    <Link
                      to={`/teams/${encodeURIComponent(
                        teamId,
                      )}/habits/${encodeURIComponent(habit.habitId)}/leaderboard`}
                      className="button button-secondary"
                    >
                      Leaderboard
                    </Link>
                  </div>
                ) : (
                  <div className="habit-details-actions">
                    <Link
                      to={`/teams/${encodeURIComponent(
                        teamId,
                      )}/habits/${encodeURIComponent(habit.habitId)}/leaderboard`}
                      className="button button-primary"
                    >
                      Leaderboard
                    </Link>
                  </div>
                )}
              </>
            ) : null}
          </div>
        </div>
      </section>
    </main>
  );
}
