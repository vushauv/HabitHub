import { Link, useNavigate } from "react-router-dom";
import { useEffect, useMemo, useState } from "react";
import {
  clearStoredAuth,
  getReminderErrorMessage,
  getStoredAuth,
  ReminderRequestError,
} from "../services/Reminder";
import {
  getHabitErrorMessage,
  HabitRequestError,
} from "../services/Habit";
import {
  getTeamErrorMessage,
  TeamRequestError,
} from "../services/Team";
import { getVisibleUnreadReminderCount } from "../services/ReminderView";

function isAuthRequiredError(error: unknown): boolean {
  return (
    (error instanceof ReminderRequestError ||
      error instanceof TeamRequestError ||
      error instanceof HabitRequestError) &&
    error.code === "auth-required"
  );
}

function getFallbackMessage(error: unknown): string {
  if (error instanceof ReminderRequestError) {
    return error.message || getReminderErrorMessage(error.code);
  }

  if (error instanceof TeamRequestError) {
    return error.message || getTeamErrorMessage(error.code);
  }

  if (error instanceof HabitRequestError) {
    return error.message || getHabitErrorMessage(error.code);
  }

  return "Reminders are unavailable right now.";
}

export default function ReminderDashboardLink() {
  const navigate = useNavigate();
  const auth = useMemo(() => getStoredAuth(), []);
  const [count, setCount] = useState<number | null>(null);
  const [label, setLabel] = useState("Reminders");

  useEffect(() => {
    let isMounted = true;

    const loadReminderCount = async () => {
      if (!auth) {
        clearStoredAuth();
        navigate("/login", { replace: true });
        return;
      }

      try {
        const unreadCount = await getVisibleUnreadReminderCount(auth);

        if (!isMounted) {
          return;
        }

        setCount(unreadCount);
        setLabel(
          unreadCount > 0
            ? `${unreadCount} new reminder${unreadCount === 1 ? "" : "s"}`
            : "No new reminders",
        );
      } catch (error) {
        if (!isMounted) {
          return;
        }

        if (isAuthRequiredError(error)) {
          clearStoredAuth();
          navigate("/login", { replace: true });
          return;
        }

        setLabel(getFallbackMessage(error));
      }
    };

    void loadReminderCount();

    return () => {
      isMounted = false;
    };
  }, [auth, navigate]);

  return (
    <Link
      to="/reminders"
      className="button button-secondary dashboard-pill dashboard-reminders-link"
      aria-label={label}
    >
      <span>Reminders</span>
      {count !== null && count > 0 ? (
        <span className="dashboard-reminders-count">{count}</span>
      ) : null}
    </Link>
  );
}
