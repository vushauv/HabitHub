import { API_BASE_URL } from "./User";
import {
  getAuthHeaders,
  type StoredAuth,
} from "./Auth";
import type {
  NotificationCountDto,
  NotificationDto,
  NotificationType,
} from "./dtos";

export type {
  NotificationDto,
  NotificationStatus,
  NotificationType,
} from "./dtos";
export { clearStoredAuth, getStoredAuth } from "./Auth";

export type NotificationErrorCode =
  | "auth-required"
  | "validation-error"
  | "forbidden"
  | "not-found"
  | "internal-server-error"
  | "unknown";

type RawErrorResponse = {
  error?: string | null;
  message?: string | null;
  Error?: string | null;
  Message?: string | null;
};

export class NotificationRequestError extends Error {
  status: number;
  code: NotificationErrorCode;

  constructor(status: number, code: NotificationErrorCode, message: string) {
    super(message);
    this.name = "NotificationRequestError";
    this.status = status;
    this.code = code;
  }
}

export function getNotificationErrorMessage(
  errorCode: NotificationErrorCode,
): string {
  switch (errorCode) {
    case "auth-required":
      return "Your session is no longer valid. Please log in again.";
    case "validation-error":
      return "Please check the notification request and try again.";
    case "forbidden":
      return "You do not have permission to manage this notification.";
    case "not-found":
      return "The selected notification could not be found.";
    case "internal-server-error":
      return "The server could not finish this notification action right now.";
    default:
      return "Something went wrong while managing notifications. Please try again.";
  }
}

export function formatNotificationCreatedAt(dateString: string): string {
  const parsedDate = new Date(dateString);

  if (Number.isNaN(parsedDate.getTime())) {
    return "Unknown date";
  }

  return new Intl.DateTimeFormat(undefined, {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(parsedDate);
}

export function formatNotificationStatus(
  status: NotificationDto["status"],
): string {
  return status === "Unread" ? "New" : status;
}

export async function getNotifications(
  auth: StoredAuth | null,
  type?: NotificationType,
): Promise<NotificationDto[]> {
  return requestJson<NotificationDto[]>(getNotificationsPath(type), {
    method: "GET",
    headers: getAuthHeaders(auth),
  });
}

export async function getUnreadNotificationCount(
  auth: StoredAuth | null,
  type?: NotificationType,
): Promise<number> {
  const response = await requestJson<NotificationCountDto>(
    getUnreadCountPath(type),
    {
      method: "GET",
      headers: getAuthHeaders(auth),
    },
  );

  return response.count;
}

export async function markNotificationAsRead(
  auth: StoredAuth | null,
  notificationId: string,
): Promise<void> {
  await requestEmpty(
    `/notifications/${encodeURIComponent(notificationId)}/read`,
    {
      method: "PATCH",
      headers: getAuthHeaders(auth),
    },
  );
}

export async function markAllNotificationsAsRead(
  auth: StoredAuth | null,
  type?: NotificationType,
): Promise<void> {
  const query = type ? `?type=${encodeURIComponent(type)}` : "";

  await requestEmpty(`/notifications/read-all${query}`, {
    method: "PATCH",
    headers: getAuthHeaders(auth),
  });
}

export async function deleteNotification(
  auth: StoredAuth | null,
  notificationId: string,
): Promise<void> {
  await requestEmpty(`/notifications/${encodeURIComponent(notificationId)}`, {
    method: "DELETE",
    headers: getAuthHeaders(auth),
  });
}

function getNotificationsPath(type?: NotificationType): string {
  return type
    ? `/notifications?type=${encodeURIComponent(type)}`
    : "/notifications";
}

function getUnreadCountPath(type?: NotificationType): string {
  return type
    ? `/notifications/unread-count?type=${encodeURIComponent(type)}`
    : "/notifications/unread-count";
}

async function requestJson<T>(
  path: string,
  options: RequestInit,
): Promise<T> {
  const response = await fetch(`${API_BASE_URL}${path}`, options);

  if (!response.ok) {
    throw await createNotificationRequestError(response);
  }

  return (await response.json()) as T;
}

async function requestEmpty(
  path: string,
  options: RequestInit,
): Promise<void> {
  const response = await fetch(`${API_BASE_URL}${path}`, options);

  if (!response.ok) {
    throw await createNotificationRequestError(response);
  }
}

async function createNotificationRequestError(
  response: Response,
): Promise<NotificationRequestError> {
  const responseText = await response.text().catch(() => "");
  const parsedError = parseErrorResponse(responseText);
  const code = normalizeErrorCode(
    response.status,
    parsedError.error ?? parsedError.Error ?? responseText,
  );
  const message =
    parsedError.message ??
    parsedError.Message ??
    getNotificationErrorMessage(code);

  return new NotificationRequestError(response.status, code, message);
}

function parseErrorResponse(responseText: string): RawErrorResponse {
  if (!responseText) {
    return {};
  }

  try {
    return JSON.parse(responseText) as RawErrorResponse;
  } catch {
    return {};
  }
}

function normalizeErrorCode(
  status: number,
  rawCode: string | null | undefined,
): NotificationErrorCode {
  if (rawCode === "auth-required" || status === 401) {
    return "auth-required";
  }

  if (rawCode === "validation-error" || status === 400) {
    return "validation-error";
  }

  if (rawCode === "forbidden" || status === 403) {
    return "forbidden";
  }

  if (rawCode === "not-found" || status === 404) {
    return "not-found";
  }

  if (rawCode === "internal-server-error" || status >= 500) {
    return "internal-server-error";
  }

  return "unknown";
}
