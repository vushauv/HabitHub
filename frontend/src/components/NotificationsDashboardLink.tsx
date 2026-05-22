import { Link, useNavigate } from "react-router-dom";
import { useEffect, useMemo, useState } from "react";
import {
  clearStoredAuth,
  getNotificationErrorMessage,
  getStoredAuth,
  getUnreadNotificationCount,
  NotificationRequestError,
} from "../services/Notification";

const REFRESH_INTERVAL_MS = 60_000;

export default function NotificationsDashboardLink() {
  const navigate = useNavigate();
  const auth = useMemo(() => getStoredAuth(), []);
  const [count, setCount] = useState<number | null>(null);
  const [label, setLabel] = useState("Notifications");

  useEffect(() => {
    let isMounted = true;

    const loadNotificationCount = async () => {
      if (!auth) {
        clearStoredAuth();
        navigate("/login", { replace: true });
        return;
      }

      try {
        const unreadCount = await getUnreadNotificationCount(auth);

        if (!isMounted) {
          return;
        }

        setCount(unreadCount);
        setLabel(
          unreadCount > 0
            ? `${unreadCount} new notification${unreadCount === 1 ? "" : "s"}`
            : "No new notifications",
        );
      } catch (error) {
        if (!isMounted) {
          return;
        }

        if (
          error instanceof NotificationRequestError &&
          error.code === "auth-required"
        ) {
          clearStoredAuth();
          navigate("/login", { replace: true });
          return;
        }

        setCount(0);
        setLabel(
          error instanceof NotificationRequestError
            ? error.message || getNotificationErrorMessage(error.code)
            : "Notifications are unavailable right now.",
        );
      }
    };

    void loadNotificationCount();

    const intervalId = window.setInterval(() => {
      void loadNotificationCount();
    }, REFRESH_INTERVAL_MS);

    const handleFocus = () => {
      void loadNotificationCount();
    };

    window.addEventListener("focus", handleFocus);

    return () => {
      isMounted = false;
      window.clearInterval(intervalId);
      window.removeEventListener("focus", handleFocus);
    };
  }, [auth, navigate]);

  return (
    <Link
      to="/notifications"
      className="button button-secondary dashboard-pill dashboard-notifications-link"
      aria-label={label}
    >
      <span>Notifications</span>
      {count !== null && count > 0 ? (
        <span className="dashboard-notifications-count">{count}</span>
      ) : null}
    </Link>
  );
}
