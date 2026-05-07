import { Link, useNavigate, useSearchParams } from "react-router-dom";
import { useEffect, useMemo, useState } from "react";
import { useForm } from "react-hook-form";
import { useLens } from "@hookform/lenses";
import { zodResolver } from "@hookform/resolvers/zod";
import "./HabitDetails.css";
import "./MemberHabitDetails.css";
import "../App.css";
import TextInput from "../components/form/TextInput";
import {
  clearStoredAuth,
  formatEntryStatus,
  formatHabitEntryDateTime,
  formatHabitEntryValue,
  formatHabitType,
  formatHabitUnit,
  getHabit,
  getHabitErrorMessage,
  getMyTodayEntryStatus,
  getStoredAuth,
  HabitRequestError,
  logHabitProgress,
  logProgressFormSchema,
  undoHabitLog,
  type EntryStatusName,
  type HabitSummaryDto,
  type LogProgressForm,
  type TodayHabitEntryStatusDto,
} from "../services/Habit";
import {
  getTeam,
  getTeamErrorMessage,
  TeamRequestError,
  type TeamDetailsDto,
} from "../services/Team";

type PendingAction = "log" | "skip" | "undo" | null;

function resolveErrorMessage(error: unknown): string {
  if (error instanceof HabitRequestError) {
    return error.message || getHabitErrorMessage(error.code);
  }

  if (error instanceof TeamRequestError) {
    return error.message || getTeamErrorMessage(error.code);
  }

  return "Something went wrong while loading today's progress. Please try again.";
}

function getTodayStatusText(todayStatus: TodayHabitEntryStatusDto | null): string {
  if (!todayStatus) {
    return "Loading today's status...";
  }

  switch (todayStatus.status) {
    case 0:
      return "Progress was already logged today.";
    case 2:
      return "Progress was skipped today.";
    default:
      return "No progress has been logged today.";
  }
}

export default function MemberLogHabit() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const teamId = searchParams.get("teamId") ?? "";
  const habitId = searchParams.get("habitId") ?? "";
  const auth = useMemo(() => getStoredAuth(), []);
  const [team, setTeam] = useState<TeamDetailsDto | null>(null);
  const [habit, setHabit] = useState<HabitSummaryDto | null>(null);
  const [todayStatus, setTodayStatus] =
    useState<TodayHabitEntryStatusDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [pageError, setPageError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [pendingAction, setPendingAction] = useState<PendingAction>(null);

  const { control, formState, reset, watch } = useForm<LogProgressForm>({
    defaultValues: {
      value: "",
      notes: "",
    },
    disabled: pendingAction !== null,
    resolver: zodResolver(logProgressFormSchema),
    mode: "all",
  });

  const lens = useLens({ control });
  const logValue = watch("value");
  const logNotes = watch("notes");

  const loadTodayStatus = async () => {
    if (!auth || !habitId) {
      return;
    }

    const loadedTodayStatus = await getMyTodayEntryStatus(auth, habitId);
    setTodayStatus(loadedTodayStatus);
  };

  useEffect(() => {
    let isMounted = true;

    const loadLogPage = async () => {
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
        const [loadedTeam, loadedHabit, loadedTodayStatus] = await Promise.all([
          getTeam(auth, teamId),
          getHabit(auth, habitId),
          getMyTodayEntryStatus(auth, habitId),
        ]);

        if (!isMounted) {
          return;
        }

        setTeam(loadedTeam);
        setHabit(loadedHabit);
        setTodayStatus(loadedTodayStatus);
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

    void loadLogPage();

    return () => {
      isMounted = false;
    };
  }, [auth, habitId, navigate, teamId]);

  const handleLogProgress = async (
    status: Exclude<EntryStatusName, "Pending">,
  ) => {
    setPageError(null);
    setSuccessMessage(null);

    if (!habit) {
      setPageError("Choose a habit first.");
      return;
    }

    if (habit.habitState !== 0) {
      setPageError("Archived habits cannot be changed.");
      return;
    }

    if (status === "Logged" && habit.habitType === 1) {
      const numericValue = Number(logValue);

      if (logValue.trim() === "" || Number.isNaN(numericValue)) {
        setPageError("Enter a numeric value before logging progress.");
        return;
      }
    }

    setPendingAction(status === "Skipped" ? "skip" : "log");

    try {
      await logHabitProgress(
        auth,
        habit,
        {
          value: logValue,
          notes: logNotes,
        },
        status,
      );

      reset({
        value: "",
        notes: "",
      });
      await loadTodayStatus();
      setSuccessMessage(
        status === "Skipped" ? "Progress skipped." : "Progress logged.",
      );
    } catch (error) {
      handleActionError(error);
    } finally {
      setPendingAction(null);
    }
  };

  const handleUndoLog = async () => {
    setPageError(null);
    setSuccessMessage(null);

    if (!habit || !todayStatus?.entry) {
      setPageError("Today's progress log could not be found.");
      return;
    }

    const confirmed = window.confirm("Undo today's progress log?");

    if (!confirmed) {
      return;
    }

    setPendingAction("undo");

    try {
      await undoHabitLog(auth, habit.habitId, todayStatus.entry.entryId);
      await loadTodayStatus();
      setSuccessMessage("Today's progress log was undone.");
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

  const canLogToday =
    habit?.habitState === 0 &&
    todayStatus?.status === 1 &&
    todayStatus.entry === null;
  const canUndoToday = habit?.habitState === 0 && todayStatus?.entry != null;
  const isQuantitativeHabit = habit?.habitType === 1;

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
                Log Habit
              </h1>

              {team && habit ? (
                <p className="text habit-details-team-name">
                  {team.name} · {habit.name}
                </p>
              ) : null}
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
                <p className="state-title">Loading today...</p>
                <p className="state-text">
                  We are checking whether this habit has already been logged.
                </p>
              </div>
            ) : habit ? (
              <>
                <section className="habit-details-grid" aria-label="Habit summary">
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
                </section>

                <section className="member-habit-log-card" aria-label="Today's progress">
                  <div className="member-habit-log-top">
                    <p className="member-habit-log-title">Today</p>
                    <span className="member-habit-status-pill">
                      {todayStatus ? formatEntryStatus(todayStatus.status) : "Loading"}
                    </span>
                  </div>

                  <p className="member-habit-log-text">
                    {getTodayStatusText(todayStatus)}
                  </p>

                  {todayStatus?.entry ? (
                    <div className="habit-details-grid" aria-label="Today entry">
                      <div className="habit-details-item">
                        <p className="habit-details-label">Value</p>
                        <p className="habit-details-value">
                          {formatHabitEntryValue(todayStatus.entry, habit)}
                        </p>
                      </div>

                      <div className="habit-details-item">
                        <p className="habit-details-label">Logged at</p>
                        <p className="habit-details-value">
                          {formatHabitEntryDateTime(todayStatus.entry.loggedAt)}
                        </p>
                      </div>

                      <div className="habit-details-item habit-details-goal">
                        <p className="habit-details-label">Notes</p>
                        <p className="habit-details-value">
                          {todayStatus.entry.notes ?? "No notes"}
                        </p>
                      </div>
                    </div>
                  ) : null}

                  {canLogToday ? (
                    <div className="member-habit-log-form">
                      {isQuantitativeHabit ? (
                        <TextInput
                          label="Value"
                          lens={lens.focus("value")}
                          type="number"
                          placeholder="10"
                          autoComplete="off"
                          required
                        />
                      ) : null}

                      <TextInput
                        label="Notes"
                        lens={lens.focus("notes")}
                        type="text"
                        placeholder="Optional note"
                        autoComplete="off"
                      />

                      <div className="member-habit-log-actions">
                        <button
                          type="button"
                          className="button button-primary"
                          onClick={() => void handleLogProgress("Logged")}
                          disabled={pendingAction !== null || !formState.isValid}
                        >
                          {pendingAction === "log"
                            ? "Logging..."
                            : isQuantitativeHabit
                              ? "Log Progress"
                              : "Mark Completed"}
                        </button>

                        <button
                          type="button"
                          className="button button-secondary"
                          onClick={() => void handleLogProgress("Skipped")}
                          disabled={pendingAction !== null || !formState.isValid}
                        >
                          {pendingAction === "skip" ? "Skipping..." : "Skip Today"}
                        </button>
                      </div>
                    </div>
                  ) : canUndoToday ? (
                    <div className="member-habit-log-actions">
                      <button
                        type="button"
                        className="button button-secondary member-habit-danger-button"
                        onClick={() => void handleUndoLog()}
                        disabled={pendingAction !== null}
                      >
                        {pendingAction === "undo" ? "Undoing..." : "Undo Log"}
                      </button>
                    </div>
                  ) : habit.habitState !== 0 ? (
                    <p className="member-habit-log-text">
                      Changes are disabled because this habit is archived.
                    </p>
                  ) : null}
                </section>
              </>
            ) : null}
          </div>
        </div>
      </section>
    </main>
  );
}
