import { Link, useNavigate, useOutletContext } from "react-router-dom";
import { useEffect, useMemo, useState } from "react";
import "./Notifications.css";
import "../App.css";
import {
  clearStoredAuth,
  deleteNotification,
  formatNotificationCreatedAt,
  formatNotificationStatus,
  getNotificationErrorMessage,
  getNotifications,
  getStoredAuth,
  markAllNotificationsAsRead,
  markNotificationAsRead,
  NotificationRequestError,
  type NotificationDto,
} from "../services/Notification";
import type { UserDto } from "../services/dtos";

type PendingAction = {
  notificationId: string;
  action: "read" | "delete";
};

function canManageNotification(notification: NotificationDto): boolean {
  return notification.type !== "Reminder";
}

function resolveErrorMessage(error: unknown): string {
  if (error instanceof NotificationRequestError) {
    return error.message || getNotificationErrorMessage(error.code);
  }

  return "Something went wrong while loading notifications. Please try again.";
}

function isAuthRequiredError(error: unknown): boolean {
  return (
    error instanceof NotificationRequestError &&
    error.code === "auth-required"
  );
}

export default function Notifications() {
  const navigate = useNavigate();
  const currentUser = useOutletContext<UserDto>();
  const auth = useMemo(() => getStoredAuth(), []);
  const [notifications, setNotifications] = useState<NotificationDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [pageError, setPageError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [pendingAction, setPendingAction] = useState<PendingAction | null>(null);
  const [markingAllAsRead, setMarkingAllAsRead] = useState(false);
  const dashboardPath =
    currentUser.userType === "Creator" ? "/creator" : "/member";
  const unreadCount = notifications.filter(
    (notification) => notification.status === "Unread",
  ).length;
  const manageableUnreadCount = notifications.filter(
    (notification) =>
      notification.status === "Unread" && canManageNotification(notification),
  ).length;

  useEffect(() => {
    let isMounted = true;

    const loadNotifications = async () => {
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
        const loadedNotifications = await getNotifications(auth);

        if (!isMounted) {
          return;
        }

        setNotifications(loadedNotifications);
      } catch (error) {
        if (!isMounted) {
          return;
        }

        if (isAuthRequiredError(error)) {
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

    void loadNotifications();

    return () => {
      isMounted = false;
    };
  }, [auth, navigate]);

  const handleAuthError = (error: unknown): boolean => {
    if (!isAuthRequiredError(error)) {
      return false;
    }

    clearStoredAuth();
    setPageError(resolveErrorMessage(error));

    setTimeout(() => {
      navigate("/login", { replace: true });
    }, 1200);

    return true;
  };

  const handleMarkAsRead = async (notification: NotificationDto) => {
    if (!canManageNotification(notification)) {
      return;
    }

    setPendingAction({
      notificationId: notification.notificationId,
      action: "read",
    });
    setPageError(null);
    setSuccessMessage(null);

    try {
      await markNotificationAsRead(auth, notification.notificationId);

      setNotifications((currentNotifications) =>
        currentNotifications.map((currentNotification) =>
          currentNotification.notificationId === notification.notificationId
            ? { ...currentNotification, status: "Read" }
            : currentNotification,
        ),
      );
      setSuccessMessage("Notification marked as read.");
    } catch (error) {
      if (!handleAuthError(error)) {
        setPageError(resolveErrorMessage(error));
      }
    } finally {
      setPendingAction(null);
    }
  };

  const handleMarkAllAsRead = async () => {
    if (manageableUnreadCount === 0) {
      return;
    }

    setMarkingAllAsRead(true);
    setPageError(null);
    setSuccessMessage(null);

    try {
      await markAllNotificationsAsRead(auth, "System");

      setNotifications((currentNotifications) =>
        currentNotifications.map((notification) =>
          notification.status === "Unread" && canManageNotification(notification)
            ? { ...notification, status: "Read" }
            : notification,
        ),
      );
      setSuccessMessage("All notifications marked as read.");
    } catch (error) {
      if (!handleAuthError(error)) {
        setPageError(resolveErrorMessage(error));
      }
    } finally {
      setMarkingAllAsRead(false);
    }
  };

  const handleDeleteNotification = async (notification: NotificationDto) => {
    if (!canManageNotification(notification)) {
      return;
    }

    const confirmed = window.confirm("Delete this notification?");

    if (!confirmed) {
      return;
    }

    setPendingAction({
      notificationId: notification.notificationId,
      action: "delete",
    });
    setPageError(null);
    setSuccessMessage(null);

    try {
      await deleteNotification(auth, notification.notificationId);

      setNotifications((currentNotifications) =>
        currentNotifications.filter(
          (currentNotification) =>
            currentNotification.notificationId !== notification.notificationId,
        ),
      );
      setSuccessMessage("Notification deleted.");
    } catch (error) {
      if (!handleAuthError(error)) {
        setPageError(resolveErrorMessage(error));
      }
    } finally {
      setPendingAction(null);
    }
  };

  return (
    <main className="page">
      <section className="container">
        <div className="background-glow background-glow-left" />
        <div className="background-glow background-glow-right" />

        <div className="card page-card-shell notifications-card">
          <div className="content notifications-content">
            <div className="page-topbar">
              <Link
                to={dashboardPath}
                className="button button-secondary page-nav-button"
              >
                Dashboard
              </Link>
            </div>

            <div className="content-centered notifications-header">
              <h1 className="title page-title notifications-title pill-title">
                Notifications
              </h1>

              <p className="notifications-summary">
                <span className="notifications-summary-count">
                  {unreadCount}
                </span>
                <span>new notification{unreadCount === 1 ? "" : "s"}</span>
              </p>
            </div>

            {pageError ? (
              <p className="form-error page-message" role="alert">
                {pageError}
              </p>
            ) : null}

            {successMessage ? (
              <p className="alert-success">{successMessage}</p>
            ) : null}

            {!loading && notifications.length > 0 ? (
              <div className="notifications-actions">
                <button
                  type="button"
                  className="button button-secondary notifications-mark-all"
                  onClick={() => void handleMarkAllAsRead()}
                  disabled={markingAllAsRead || manageableUnreadCount === 0}
                >
                  {markingAllAsRead ? "Marking..." : "Mark all as read"}
                </button>
              </div>
            ) : null}

            {loading ? (
              <div className="state-card">
                <p className="state-title">Loading notifications...</p>
                <p className="state-text">
                  We are retrieving your notifications.
                </p>
              </div>
            ) : notifications.length === 0 ? (
              <div className="state-card">
                <p className="state-title">No notifications found</p>
                <p className="state-text">
                  New account and habit updates will appear here.
                </p>
              </div>
            ) : (
              <section className="table-list" aria-label="Notifications">
                <div className="data-table-row notifications-table-row data-table-head notifications-table-head">
                  <span>Date</span>
                  <span>Type</span>
                  <span>Message</span>
                  <span>Status</span>
                  <span></span>
                </div>

                {notifications.map((notification) => {
                  const isUnread = notification.status === "Unread";
                  const canManage = canManageNotification(notification);
                  const isMarkingRead =
                    pendingAction?.notificationId === notification.notificationId &&
                    pendingAction.action === "read";
                  const isDeleting =
                    pendingAction?.notificationId === notification.notificationId &&
                    pendingAction.action === "delete";

                  return (
                    <article
                      className={`data-table-row notifications-table-row ${
                        isUnread ? "notifications-table-row-unread" : ""
                      }`}
                      key={notification.notificationId}
                    >
                      <span className="notifications-date">
                        {formatNotificationCreatedAt(notification.createdAt)}
                      </span>
                      <span className="notifications-type">
                        {notification.type}
                      </span>
                      <span className="notifications-message">
                        {notification.content}
                      </span>
                      <span
                        className={`notifications-status ${
                          isUnread ? "notifications-status-new" : ""
                        }`}
                      >
                        {formatNotificationStatus(notification.status)}
                      </span>

                      <div className="notifications-row-actions">
                        {canManage && isUnread ? (
                          <button
                            type="button"
                            className="button button-secondary table-row-button"
                            onClick={() =>
                              void handleMarkAsRead(notification)
                            }
                            disabled={Boolean(pendingAction)}
                          >
                            {isMarkingRead ? "Saving..." : "Mark read"}
                          </button>
                        ) : null}

                        {canManage ? (
                          <button
                            type="button"
                            className="button button-secondary table-row-button notifications-delete-button"
                            onClick={() =>
                              void handleDeleteNotification(notification)
                            }
                            disabled={Boolean(pendingAction)}
                          >
                            {isDeleting ? "Deleting..." : "Delete"}
                          </button>
                        ) : null}
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
