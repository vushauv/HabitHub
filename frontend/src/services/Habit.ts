import z from "zod";
import { API_BASE_URL } from "./User";
import {
  getAuthHeaders,
  type StoredAuth,
} from "./Auth";
import type {
  CreateHabitRequestDto,
  CreateHabitResponseDto,
  EditHabitRequestDto,
  HabitStateDto,
  HabitSummaryDto,
  HabitTypeDto,
  LeaderboardResponseDto,
  UnitDto,
} from "./dtos";

export type {
  CreateHabitRequestDto,
  CreateHabitResponseDto,
  EditHabitRequestDto,
  HabitSummaryDto,
  LeaderboardResponseDto,
} from "./dtos";
export { clearStoredAuth, getStoredAuth } from "./Auth";

export type HabitStateFilter = "Active" | "Archived";

export type HabitTypeName = "Binary" | "Quantitative";

export type UnitName =
  | "Km"
  | "Hours"
  | "Minutes"
  | "Kg"
  | "Cups"
  | "Steps"
  | "Pages";

export type CreateHabitForm = {
  name: string;
  goal: string;
  habitType: HabitTypeName;
  unit: "" | UnitName;
  expiryDate: string;
};

export type EditHabitForm = {
  name: string;
  goal: string;
  expiryDate: string;
  clearGoal: boolean;
  clearExpiryDate: boolean;
};

export type HabitErrorCode =
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

const habitTypeValues = ["Binary", "Quantitative"] as const;
const unitValues = ["Km", "Hours", "Minutes", "Kg", "Cups", "Steps", "Pages"] as const;

export const habitTypeOptions: HabitTypeName[] = [...habitTypeValues];
export const unitOptions: UnitName[] = [...unitValues];

export class HabitRequestError extends Error {
  status: number;
  code: HabitErrorCode;

  constructor(status: number, code: HabitErrorCode, message: string) {
    super(message);
    this.name = "HabitRequestError";
    this.status = status;
    this.code = code;
  }
}

export const habitNameSchema = z
  .string()
  .trim()
  .nonempty({ error: "Habit name is required." })
  .max(256, { error: "Habit name must be 256 characters or shorter." });

export const habitGoalSchema = z
  .string()
  .max(512, { error: "Goal must be 512 characters or shorter." });

const futureDateSchema = z.string().refine(
  (value) => value === "" || new Date(value).getTime() > Date.now(),
  { error: "Expiry date must be in the future." },
);

export const createHabitFormSchema = z
  .object({
    name: habitNameSchema,
    goal: habitGoalSchema,
    habitType: z.enum(habitTypeValues, { error: "Choose a habit type." }),
    unit: z.union([z.literal(""), z.enum(unitValues)]),
    expiryDate: futureDateSchema,
  })
  .required()
  .superRefine((form, context) => {
    if (form.habitType === "Quantitative" && form.unit === "") {
      context.addIssue({
        code: "custom",
        path: ["unit"],
        message: "Choose a unit for quantitative habits.",
      });
    }

    if (form.habitType === "Binary" && form.unit !== "") {
      context.addIssue({
        code: "custom",
        path: ["unit"],
        message: "Binary habits cannot have a unit.",
      });
    }
  });

export const editHabitFormSchema = z
  .object({
    name: habitNameSchema,
    goal: habitGoalSchema,
    expiryDate: futureDateSchema,
    clearGoal: z.boolean(),
    clearExpiryDate: z.boolean(),
  })
  .required()
  .superRefine((form, context) => {
    if (form.clearGoal && form.goal.trim()) {
      context.addIssue({
        code: "custom",
        path: ["goal"],
        message: "Clear the goal field before choosing Clear goal.",
      });
    }

    if (form.clearExpiryDate && form.expiryDate) {
      context.addIssue({
        code: "custom",
        path: ["expiryDate"],
        message: "Clear the expiry date before choosing Clear expiry date.",
      });
    }
  });

export function getHabitErrorMessage(errorCode: HabitErrorCode): string {
  switch (errorCode) {
    case "auth-required":
      return "Your session is no longer valid. Please log in again.";
    case "validation-error":
      return "Please check the habit fields and try again.";
    case "forbidden":
      return "You do not have permission to manage this habit.";
    case "not-found":
      return "The selected habit could not be found.";
    case "habit-archived":
      return "Archived habits cannot be edited.";
    case "internal-server-error":
      return "The server could not finish this habit action right now.";
    default:
      return "Something went wrong while managing habits. Please try again.";
  }
}

export function formatHabitState(habitState: HabitStateDto): string {
  switch (habitState) {
    case 1:
      return "Archived";
    case 2:
      return "Closed";
    default:
      return "Active";
  }
}

export function formatHabitType(habitType: HabitTypeDto): HabitTypeName {
  return habitType === 1 ? "Quantitative" : "Binary";
}

export function formatHabitUnit(unit: UnitDto | null): string {
  if (unit === null) {
    return "No unit";
  }

  return unitOptions[unit] ?? "Unknown unit";
}

export function formatHabitExpiryDate(dateString: string | null): string {
  if (!dateString) {
    return "No expiry date";
  }

  const parsedDate = new Date(dateString);

  if (Number.isNaN(parsedDate.getTime())) {
    return "Unknown date";
  }

  return new Intl.DateTimeFormat(undefined, {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(parsedDate);
}

export function formatDateTimeInputValue(dateString: string | null): string {
  if (!dateString) {
    return "";
  }

  const parsedDate = new Date(dateString);

  if (Number.isNaN(parsedDate.getTime())) {
    return "";
  }

  const offsetDate = new Date(
    parsedDate.getTime() - parsedDate.getTimezoneOffset() * 60_000,
  );

  return offsetDate.toISOString().slice(0, 16);
}

export function createEditHabitDefaultValues(habit: HabitSummaryDto): EditHabitForm {
  return {
    name: habit.name,
    goal: habit.goal ?? "",
    expiryDate: formatDateTimeInputValue(habit.expiryDate),
    clearGoal: false,
    clearExpiryDate: false,
  };
}

export async function getTeamHabits(
  auth: StoredAuth | null,
  teamId: string,
  state: HabitStateFilter,
): Promise<HabitSummaryDto[]> {
  return requestJson<HabitSummaryDto[]>(
    `/teams/${teamId}/habits?state=${state}`,
    {
      method: "GET",
      headers: getAuthHeaders(auth),
    },
  );
}

export async function getHabit(
  auth: StoredAuth | null,
  habitId: string,
): Promise<HabitSummaryDto> {
  return requestJson<HabitSummaryDto>(`/habits/${habitId}`, {
    method: "GET",
    headers: getAuthHeaders(auth),
  });
}

export async function createHabit(
  auth: StoredAuth | null,
  teamId: string,
  form: CreateHabitForm,
): Promise<CreateHabitResponseDto> {
  return requestJson<CreateHabitResponseDto>(`/teams/${teamId}/habits`, {
    method: "POST",
    headers: getAuthHeaders(auth),
    body: JSON.stringify(toCreateHabitRequest(form)),
  });
}

export async function editHabit(
  auth: StoredAuth | null,
  habitId: string,
  form: EditHabitForm,
  currentHabit: HabitSummaryDto,
): Promise<HabitSummaryDto> {
  return requestJson<HabitSummaryDto>(`/habits/${habitId}`, {
    method: "PATCH",
    headers: getAuthHeaders(auth),
    body: JSON.stringify(toEditHabitRequest(form, currentHabit)),
  });
}

export async function archiveHabit(
  auth: StoredAuth | null,
  habitId: string,
): Promise<void> {
  await requestEmpty(`/habits/${habitId}/archive`, {
    method: "POST",
    headers: getAuthHeaders(auth),
  });
}

export async function deleteHabit(
  auth: StoredAuth | null,
  habitId: string,
): Promise<void> {
  await requestEmpty(`/habits/${habitId}`, {
    method: "DELETE",
    headers: getAuthHeaders(auth),
  });
}

export async function getHabitLeaderboard(
  auth: StoredAuth | null,
  habitId: string,
): Promise<LeaderboardResponseDto[]> {
  return requestJson<LeaderboardResponseDto[]>(`/habits/${habitId}/leaderboard`, {
    method: "GET",
    headers: getAuthHeaders(auth),
  });
}

function toCreateHabitRequest(form: CreateHabitForm): CreateHabitRequestDto {
  return {
    name: form.name.trim(),
    goal: normalizeNullableString(form.goal),
    habitType: mapHabitTypeToDto(form.habitType),
    unit: form.habitType === "Quantitative" ? mapUnitToDto(form.unit) : null,
    expiryDate: normalizeDateTime(form.expiryDate),
  };
}

function toEditHabitRequest(
  form: EditHabitForm,
  currentHabit: HabitSummaryDto,
): EditHabitRequestDto {
  const request: EditHabitRequestDto = {};
  const name = form.name.trim();
  const goal = normalizeNullableString(form.goal);
  const expiryDate = normalizeDateTime(form.expiryDate);
  const currentExpiryInput = formatDateTimeInputValue(currentHabit.expiryDate);

  if (name !== currentHabit.name) {
    request.name = name;
  }

  if (form.clearGoal || (goal === null && currentHabit.goal !== null)) {
    request.clearGoal = true;
    request.goal = null;
  } else if (goal !== currentHabit.goal) {
    request.goal = goal;
  }

  if (
    form.clearExpiryDate ||
    (form.expiryDate === "" && currentHabit.expiryDate !== null)
  ) {
    request.clearExpiryDate = true;
    request.expiryDate = null;
  } else if (form.expiryDate !== currentExpiryInput) {
    request.expiryDate = expiryDate;
  }

  return request;
}

function mapHabitTypeToDto(habitType: HabitTypeName): HabitTypeDto {
  return habitType === "Quantitative" ? 1 : 0;
}

function mapUnitToDto(unit: "" | UnitName): UnitDto | null {
  if (unit === "") {
    return null;
  }

  const unitIndex = unitOptions.indexOf(unit);

  return unitIndex === -1 ? null : (unitIndex as UnitDto);
}

function normalizeNullableString(value: string): string | null {
  const normalizedValue = value.trim();
  return normalizedValue ? normalizedValue : null;
}

function normalizeDateTime(value: string): string | null {
  return value ? new Date(value).toISOString() : null;
}

async function requestJson<T>(
  path: string,
  options: RequestInit,
): Promise<T> {
  const response = await fetch(`${API_BASE_URL}${path}`, options);

  if (!response.ok) {
    throw await createHabitRequestError(response);
  }

  return (await response.json()) as T;
}

async function requestEmpty(
  path: string,
  options: RequestInit,
): Promise<void> {
  const response = await fetch(`${API_BASE_URL}${path}`, options);

  if (!response.ok) {
    throw await createHabitRequestError(response);
  }
}

async function createHabitRequestError(
  response: Response,
): Promise<HabitRequestError> {
  const responseText = await response.text().catch(() => "");
  const parsedError = parseErrorResponse(responseText);
  const code = normalizeErrorCode(
    parsedError.error ?? parsedError.Error ?? responseText,
  );
  const message =
    parsedError.message ??
    parsedError.Message ??
    getHabitErrorMessage(code);

  return new HabitRequestError(response.status, code, message);
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

function normalizeErrorCode(rawCode: string | null | undefined): HabitErrorCode {
  const knownCodes: HabitErrorCode[] = [
    "auth-required",
    "validation-error",
    "forbidden",
    "not-found",
    "habit-archived",
    "internal-server-error",
  ];

  return knownCodes.includes(rawCode as HabitErrorCode)
    ? (rawCode as HabitErrorCode)
    : "unknown";
}
