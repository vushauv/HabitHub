import { Link, useNavigate, useOutletContext } from "react-router-dom";
import { useEffect, useMemo, useState } from "react";
import "./Reminders.css";
import "../App.css";
import {
  clearStoredAuth,
  formatReminderCreatedAt,
  getReminderErrorMessage,
  getStoredAuth,
  ReminderRequestError,
  type ReminderAlertDto,
} from "../services/Reminder";
import {
  getTeamErrorMessage,
  TeamRequestError,
} from "../services/Team";
import {
  getHabitErrorMessage,
  HabitRequestError,
} from "../services/Habit";
import {
  formatReminderContext,
  formatReminderHabitName,
  formatReminderTeamName,
  getVisibleUnreadReminderData,
  type ReminderSourcesByHabitName,
} from "../services/ReminderView";
import type { UserDto } from "../services/dtos";

function resolveErrorMessage(error: unknown): string {
  if (error instanceof ReminderRequestError) {
    return error.message || getReminderErrorMessage(error.code);
  }

  if (error instanceof TeamRequestError) {
    return error.message || getTeamErrorMessage(error.code);
  }

  if (error instanceof HabitRequestError) {
    return error.message || getHabitErrorMessage(error.code);
  }

  return "Something went wrong while loading reminders. Please try again.";
}

function formatReminderStatus(status: ReminderAlertDto["status"]): string {
  return status === "Unread" ? "New" : status;
}

export default function Reminders() {
  const navigate = useNavigate();
  const currentUser = useOutletContext<UserDto>();
  const auth = useMemo(() => getStoredAuth(), []);
  const [reminders, setReminders] = useState<ReminderAlertDto[]>([]);
  const [sourcesByHabitName, setSourcesByHabitName] =
    useState<ReminderSourcesByHabitName>({});
  const [loading, setLoading] = useState(true);
  const [pageError, setPageError] = useState<string | null>(null);
  const teamsPath =
    currentUser.userType === "Creator" ? "/creator/teams" : "/member/teams";

  useEffect(() => {
    let isMounted = true;

    const loadReminders = async () => {
      setLoading(true);
      setPageError(null);

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
        const {
          reminders: loadedReminders,
          sourcesByHabitName: loadedSourcesByHabitName,
        } = await getVisibleUnreadReminderData(auth);

        if (!isMounted) {
          return;
        }

        setReminders(loadedReminders);
        setSourcesByHabitName(loadedSourcesByHabitName);
      } catch (error) {
        if (!isMounted) {
          return;
        }

        if (
          (error instanceof ReminderRequestError ||
            error instanceof TeamRequestError ||
            error instanceof HabitRequestError) &&
          error.code === "auth-required"
        ) {
          clearStoredAuth();
          setPageError(resolveErrorMessage(error));

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

    void loadReminders();

    return () => {
      isMounted = false;
    };
  }, [auth, navigate]);

  return (
    <main className="page">
      <section className="container">
        <div className="background-glow background-glow-left" />
        <div className="background-glow background-glow-right" />

        <div className="card page-card-shell reminders-card">
          <div className="content reminders-content">
            <div className="page-topbar">
              <Link
                to="/"
                className="button button-secondary page-nav-button"
              >
                Home
              </Link>

              <Link
                to={teamsPath}
                className="button button-secondary page-nav-button"
              >
                Teams
              </Link>
            </div>

            <div className="content-centered reminders-header">
              <h1 className="title page-title reminders-title pill-title">
                Reminders
              </h1>
            </div>

            {pageError ? (
              <p className="form-error page-message" role="alert">
                {pageError}
              </p>
            ) : null}

            {loading ? (
              <div className="state-card">
                <p className="state-title">Loading reminders...</p>
                <p className="state-text">
                  We are retrieving your reminder alerts.
                </p>
              </div>
            ) : reminders.length === 0 ? (
              <div className="state-card">
                <p className="state-title">No unread reminders found</p>
                <p className="state-text">
                  Missed habit reminders will appear here until the habit is logged.
                </p>
              </div>
            ) : (
              <section className="table-list" aria-label="Reminders">
                <div className="data-table-row reminders-table-row data-table-head reminders-table-head">
                  <span>Date</span>
                  <span>Team</span>
                  <span>Habit</span>
                  <span>Context</span>
                  <span>Status</span>
                </div>

                {reminders.map((reminder) => {
                  const habitName = formatReminderHabitName(reminder);
                  const teamName = formatReminderTeamName(
                    reminder,
                    sourcesByHabitName,
                  );
                  const isNew = reminder.status === "Unread";
                  const reminderContext = formatReminderContext(
                    reminder,
                    sourcesByHabitName,
                  );

                  return (
                    <article
                      className="data-table-row reminders-table-row"
                      key={reminder.notificationId}
                    >
                      <span className="reminders-meta">
                        {formatReminderCreatedAt(reminder.createdAt)}
                      </span>
                      <span className="reminders-team-name">{teamName}</span>
                      <span className="reminders-habit-name">{habitName}</span>
                      <span className="reminders-message">
                        {reminderContext}
                      </span>
                      <span
                        className={`reminders-status ${
                          isNew ? "reminders-status-new" : ""
                        }`}
                      >
                        {formatReminderStatus(reminder.status)}
                      </span>
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
