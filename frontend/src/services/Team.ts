import z from "zod";
import { API_BASE_URL } from "./User";
import {
  getAuthHeaders,
  type StoredAuth,
} from "./Auth";
import type {
  CreateTeamRequestDto,
  CreateTeamResponseDto,
  InviteCodeDto,
  JoinTeamRequestDto,
  JoinTeamResponseDto,
  MembershipStatusDto,
  TeamDetailsDto,
  TeamMemberDto as TeamMemberResponseDto,
  TeamSummaryDto,
} from "./dtos";

export type { InviteCodeDto, TeamDetailsDto, TeamSummaryDto } from "./dtos";
export { clearStoredAuth, getAuthHeaders, getStoredAuth, type StoredAuth } from "./Auth";

export type TeamMemberStatus = "Active" | "Kicked" | "Left";

export type TeamMemberDto = Omit<TeamMemberResponseDto, "status"> & {
  status: TeamMemberStatus;
};

export type CreateTeamForm = CreateTeamRequestDto;

export type JoinTeamForm = JoinTeamRequestDto;

export type TeamErrorCode =
  | "auth-required"
  | "validation-error"
  | "invalid-credentials"
  | "email-already-exists"
  | "forbidden"
  | "not-found"
  | "already-member"
  | "code-not-found"
  | "code-expired"
  | "code-invalid"
  | "cannot-kick-self"
  | "creator-cannot-leave"
  | "unknown";

export class TeamRequestError extends Error {
  status: number;
  code: TeamErrorCode;

  constructor(status: number, code: TeamErrorCode, message: string) {
    super(message);
    this.name = "TeamRequestError";
    this.status = status;
    this.code = code;
  }
}

type RawErrorResponse = {
  error?: string | null;
  message?: string | null;
  Error?: string | null;
  Message?: string | null;
};

export const teamNameSchema = z
  .string()
  .trim()
  .nonempty({ error: "Team name is required." })
  .min(3, { error: "Team name must be at least 3 characters long." })
  .max(100, { error: "Team name must be 100 characters or shorter." });

export const inviteCodeSchema = z
  .string()
  .trim()
  .nonempty({ error: "Invite code is required." })
  .length(8, { error: "Invite code must be exactly 8 characters long." });

export const createTeamFormSchema = z
  .object({
    name: teamNameSchema
  })
  .required();

export const joinTeamFormSchema = z
  .object({
    code: inviteCodeSchema
  })
  .required();

export function getTeamErrorMessage(errorCode: TeamErrorCode): string {
  switch (errorCode) {
    case "auth-required":
      return "Your session is no longer valid. Please log in again.";
    case "validation-error":
      return "Please check the form fields and try again.";
    case "forbidden":
      return "You do not have permission to manage this team.";
    case "not-found":
      return "The selected team could not be found.";
    case "code-not-found":
      return "Invite code not found.";
    case "code-expired":
      return "Invite code has expired.";
    case "code-invalid":
      return "Invite code is invalid.";
    case "already-member":
      return "You are already a member of this team.";
    case "cannot-kick-self":
      return "You cannot kick yourself from the team.";
    case "creator-cannot-leave":
      return "Team creator cannot leave the team.";
    default:
      return "Something went wrong while managing teams. Please try again.";
  }
}

export function formatInviteExpiryDate(dateString: string): string {
  const parsedDate = new Date(dateString);

  if (Number.isNaN(parsedDate.getTime())) {
    return "Unknown date";
  }

  return new Intl.DateTimeFormat(undefined, {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(parsedDate);
}

export async function getTeams(auth: StoredAuth | null): Promise<TeamSummaryDto[]> {
  return requestJson<TeamSummaryDto[]>("/teams", {
    method: "GET",
    headers: getAuthHeaders(auth),
  });
}

export async function getTeam(
  auth: StoredAuth | null,
  teamId: string,
): Promise<TeamDetailsDto> {
  return requestJson<TeamDetailsDto>(`/teams/${teamId}`, {
    method: "GET",
    headers: getAuthHeaders(auth),
  });
}

export async function createTeam(
  auth: StoredAuth | null,
  form: CreateTeamForm,
): Promise<CreateTeamResponseDto> {
  return requestJson<CreateTeamResponseDto>("/teams", {
    method: "POST",
    headers: getAuthHeaders(auth),
    body: JSON.stringify({
      name: form.name.trim(),
    }),
  });
}

export async function deleteTeam(
  auth: StoredAuth | null,
  teamId: string,
): Promise<void> {
  await requestEmpty(`/teams/${teamId}`, {
    method: "DELETE",
    headers: getAuthHeaders(auth),
  });
}

export async function getTeamMembers(
  auth: StoredAuth | null,
  teamId: string,
): Promise<TeamMemberDto[]> {
  const response = await requestJson<TeamMemberResponseDto[]>(
    `/teams/${teamId}/members`,
    {
      method: "GET",
      headers: getAuthHeaders(auth),
    },
  );

  return response.map(normalizeTeamMember);
}

export async function kickUser(
  auth: StoredAuth | null,
  teamId: string,
  memberId: string,
): Promise<void> {
  await requestEmpty(`/teams/${teamId}/members/${memberId}/kick`, {
    method: "POST",
    headers: getAuthHeaders(auth),
  });
}

export async function leaveTeam(
  auth: StoredAuth | null,
  teamId: string,
): Promise<void> {
  await requestEmpty(`/teams/${teamId}/leave`, {
    method: "POST",
    headers: getAuthHeaders(auth),
  });
}

export async function getInviteCodes(
  auth: StoredAuth | null,
  teamId: string,
): Promise<InviteCodeDto[]> {
  return requestJson<InviteCodeDto[]>(
    `/teams/${teamId}/invite-codes`,
    {
      method: "GET",
      headers: getAuthHeaders(auth),
    },
  );
}

export async function generateInviteCode(
  auth: StoredAuth | null,
  teamId: string,
): Promise<InviteCodeDto> {
  return requestJson<InviteCodeDto>(
    `/teams/${teamId}/invite-codes`,
    {
      method: "POST",
      headers: getAuthHeaders(auth),
    },
  );
}

export async function invalidateInviteCode(
  auth: StoredAuth | null,
  teamId: string,
  codeId: string,
): Promise<void> {
  await requestEmpty(`/teams/${teamId}/invite-codes/${codeId}`, {
    method: "DELETE",
    headers: getAuthHeaders(auth),
  });
}

export async function joinTeam(
  auth: StoredAuth | null,
  form: JoinTeamForm,
): Promise<JoinTeamResponseDto> {
  return requestJson<JoinTeamResponseDto>("/teams/join", {
    method: "POST",
    headers: getAuthHeaders(auth),
    body: JSON.stringify({
      code: form.code.trim(),
    }),
  });
}

async function requestJson<T>(
  path: string,
  options: RequestInit,
): Promise<T> {
  const response = await fetch(`${API_BASE_URL}${path}`, options);

  if (!response.ok) {
    throw await createTeamRequestError(response);
  }

  return (await response.json()) as T;
}

async function requestEmpty(
  path: string,
  options: RequestInit,
): Promise<void> {
  const response = await fetch(`${API_BASE_URL}${path}`, options);

  if (!response.ok) {
    throw await createTeamRequestError(response);
  }
}

async function createTeamRequestError(
  response: Response,
): Promise<TeamRequestError> {
  const responseText = await response.text().catch(() => "");
  const parsedError = parseErrorResponse(responseText);
  const code = normalizeErrorCode(
    parsedError.error ?? parsedError.Error ?? responseText,
  );
  const message =
    parsedError.message ??
    parsedError.Message ??
    getTeamErrorMessage(code);

  return new TeamRequestError(response.status, code, message);
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

function normalizeErrorCode(rawCode: string | null | undefined): TeamErrorCode {
  const knownCodes: TeamErrorCode[] = [
    "auth-required",
    "validation-error",
    "invalid-credentials",
    "email-already-exists",
    "forbidden",
    "not-found",
    "already-member",
    "code-not-found",
    "code-expired",
    "code-invalid",
    "cannot-kick-self",
    "creator-cannot-leave",
  ];

  return knownCodes.includes(rawCode as TeamErrorCode)
    ? (rawCode as TeamErrorCode)
    : "unknown";
}

function normalizeTeamMember(raw: TeamMemberResponseDto): TeamMemberDto {
  return {
    ...raw,
    status: normalizeMembershipStatus(raw.status),
  };
}

function normalizeMembershipStatus(status: MembershipStatusDto): TeamMemberStatus {
  switch (status) {
    case 1:
      return "Kicked";
    case 2:
      return "Left";
    default:
      return "Active";
  }
}
