import { API_BASE_URL } from "./User";
import {
  getAuthHeaders,
  type StoredAuth,
} from "./Auth";
import type {
  ChangeMyReminderRequestDto,
  HabitReminderResponseDto,
  MyReminderResponseDto,
  SetReminderRequestDto,
} from "./dtos";

export type {
  HabitReminderResponseDto,
  MyReminderResponseDto,
} from "./dtos";
export { clearStoredAuth, getStoredAuth } from "./Auth";

export type ReminderAlertStatus = "Unread" | "Read" | "Deleted";

export type ReminderAlertDto = {
  notificationId: string;
  content: string;
  createdAt: string;
  status: ReminderAlertStatus;
  type: "Reminder";
};

type ReminderAlertResponseDto = Omit<ReminderAlertDto, "type"> & {
  type: "System" | "Reminder";
};

type ReminderAlertCountDto = {
  count: number;
};

export type ReminderErrorCode =
  | "auth-required"
  | "validation-error"
  | "forbidden"
  | "not-found"
  | "habit-archived"
  | "internal-server-error"
  | "unknown";

type RawErrorResponse = {
  error?: string | null;
  message?: string | null;
  Error?: string | null;
  Message?: string | null;
};

export class ReminderRequestError extends Error {
  status: number;
  code: ReminderErrorCode;

  constructor(status: number, code: ReminderErrorCode, message: string) {
    super(message);
    this.name = "ReminderRequestError";
    this.status = status;
    this.code = code;
  }
}

export function getReminderErrorMessage(errorCode: ReminderErrorCode): string {
  switch (errorCode) {
    case "auth-required":
      return "Your session is no longer valid. Please log in again.";
    case "validation-error":
      return "Please check the reminder fields and try again.";
    case "forbidden":
      return "You do not have permission to manage this reminder.";
    case "not-found":
      return "The selected reminder could not be found.";
    case "habit-archived":
      return "Archived habits cannot have reminders changed.";
    case "internal-server-error":
      return "The server could not finish this reminder action right now.";
    default:
      return "Something went wrong while managing reminders. Please try again.";
  }
}

export function formatReminderTime(reminderTime: string | null): string {
  if (!reminderTime) {
    return "No reminder time";
  }

  const match = /^(\d{1,2}):(\d{2})/.exec(reminderTime);

  if (!match) {
    return "Unknown time";
  }

  return `${match[1].padStart(2, "0")}:${match[2]}`;
}

export function formatReminderTimeInputValue(reminderTime: string | null): string {
  const formattedTime = formatReminderTime(reminderTime);

  return formattedTime === "No reminder time" || formattedTime === "Unknown time"
    ? ""
    : formattedTime;
}

export function normalizeReminderTimeInput(reminderTime: string): string {
  return reminderTime.length === 5 ? `${reminderTime}:00` : reminderTime;
}

export function formatReminderCreatedAt(dateString: string): string {
  const parsedDate = new Date(dateString);

  if (Number.isNaN(parsedDate.getTime())) {
    return "Unknown date";
  }

  return new Intl.DateTimeFormat(undefined, {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(parsedDate);
}

export type ReminderContentDetails = {
  habitName: string | null;
  teamName: string | null;
};

export function extractReminderContentDetails(
  content: string,
): ReminderContentDetails {
  const normalizedContent = content.trim();
  const patterns: {
    pattern: RegExp;
    habitIndex: number;
    teamIndex?: number;
  }[] = [
    {
      pattern: /^Reminder: you have not logged "([^"]+)" for "([^"]+)" today\.?$/,
      habitIndex: 1,
      teamIndex: 2,
    },
    {
      pattern: /^Reminder: you have not logged "([^"]+)" from "([^"]+)" today\.?$/,
      habitIndex: 1,
      teamIndex: 2,
    },
    {
      pattern: /^Reminder: you have not logged "([^"]+)" today\.?$/,
      habitIndex: 1,
    },
    {
      pattern: /^Don't forget "?(.*?)"? today\.?$/,
      habitIndex: 1,
    },
    {
      pattern: /^Time to log "?(.*?)"?\.?$/,
      habitIndex: 1,
    },
    {
      pattern: /^You missed yesterday's "?(.*?)"?\.?$/,
      habitIndex: 1,
    },
  ];

  for (const { pattern, habitIndex, teamIndex } of patterns) {
    const match = pattern.exec(normalizedContent);
    const habitName = match?.[habitIndex]?.trim();

    if (habitName) {
      return {
        habitName,
        teamName: teamIndex ? match?.[teamIndex]?.trim() ?? null : null,
      };
    }
  }

  return {
    habitName: null,
    teamName: null,
  };
}

export function extractReminderHabitName(content: string): string | null {
  return extractReminderContentDetails(content).habitName;
}

export function extractReminderTeamName(content: string): string | null {
  return extractReminderContentDetails(content).teamName;
}

export async function getReminderAlerts(
  auth: StoredAuth | null,
): Promise<ReminderAlertDto[]> {
  const alerts = await requestJson<ReminderAlertResponseDto[]>(
    "/notifications?type=Reminder",
    {
      method: "GET",
      headers: getAuthHeaders(auth),
    },
  );

  return alerts.filter(
    (alert): alert is ReminderAlertDto => alert.type === "Reminder",
  );
}

export async function getUnreadReminderCount(
  auth: StoredAuth | null,
): Promise<number> {
  const response = await requestJson<ReminderAlertCountDto>(
    "/notifications/unread-count?type=Reminder",
    {
      method: "GET",
      headers: getAuthHeaders(auth),
    },
  );

  return response.count;
}

export async function getMyReminder(
  auth: StoredAuth | null,
  habitId: string,
): Promise<MyReminderResponseDto> {
  return requestJson<MyReminderResponseDto>(`/habits/${habitId}/my-reminder`, {
    method: "GET",
    headers: getAuthHeaders(auth),
  });
}

export async function changeMyReminder(
  auth: StoredAuth | null,
  habitId: string,
  enabled: boolean,
): Promise<MyReminderResponseDto> {
  const request: ChangeMyReminderRequestDto = {
    enabled,
  };

  return requestJson<MyReminderResponseDto>(`/habits/${habitId}/my-reminder`, {
    method: "PATCH",
    headers: getAuthHeaders(auth),
    body: JSON.stringify(request),
  });
}

export async function setHabitReminder(
  auth: StoredAuth | null,
  habitId: string,
  reminderTime: string,
): Promise<HabitReminderResponseDto> {
  const request: SetReminderRequestDto = {
    reminderTime: normalizeReminderTimeInput(reminderTime),
  };

  return requestJson<HabitReminderResponseDto>(`/habits/${habitId}/reminder`, {
    method: "PATCH",
    headers: getAuthHeaders(auth),
    body: JSON.stringify(request),
  });
}

export async function clearHabitReminder(
  auth: StoredAuth | null,
  habitId: string,
): Promise<void> {
  await requestEmpty(`/habits/${habitId}/reminder`, {
    method: "DELETE",
    headers: getAuthHeaders(auth),
  });
}

async function requestJson<T>(
  path: string,
  options: RequestInit,
): Promise<T> {
  const response = await fetch(`${API_BASE_URL}${path}`, options);

  if (!response.ok) {
    throw await createReminderRequestError(response);
  }

  return (await response.json()) as T;
}

async function requestEmpty(
  path: string,
  options: RequestInit,
): Promise<void> {
  const response = await fetch(`${API_BASE_URL}${path}`, options);

  if (!response.ok) {
    throw await createReminderRequestError(response);
  }
}

async function createReminderRequestError(
  response: Response,
): Promise<ReminderRequestError> {
  const responseText = await response.text().catch(() => "");
  const parsedError = parseErrorResponse(responseText);
  const code = normalizeErrorCode(
    parsedError.error ?? parsedError.Error ?? responseText,
  );
  const message =
    parsedError.message ??
    parsedError.Message ??
    getReminderErrorMessage(code);

  return new ReminderRequestError(response.status, code, message);
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
  rawCode: string | null | undefined,
): ReminderErrorCode {
  const knownCodes: ReminderErrorCode[] = [
    "auth-required",
    "validation-error",
    "forbidden",
    "not-found",
    "habit-archived",
    "internal-server-error",
  ];

  return knownCodes.includes(rawCode as ReminderErrorCode)
    ? (rawCode as ReminderErrorCode)
    : "unknown";
}
