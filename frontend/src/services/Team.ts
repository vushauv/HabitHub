import { API_BASE_URL, type AccountType } from "./User";

export type StoredAuth = {
  isLoggedIn?: boolean;
  userType?: AccountType;
  sessionId?: string | null;
  userId?: string | null;
};

export type TeamSummaryDto = {
  teamId: string;
  name: string;
};

export type TeamDetailsDto = {
  teamId: string;
  name: string;
};

export type InviteCodeDto = {
  codeId: string;
  code: string;
  teamId: string;
  expiryDate: string;
};

export type TeamMemberStatus = "Active" | "Kicked" | "Left";

export type TeamMemberDto = {
  memberId: string;
  name: string;
  email: string;
  status: TeamMemberStatus;
};

export type CreateTeamForm = {
  name: string;
};

export type JoinTeamForm = {
  code: string;
};

export type CreateTeamErrors = {
  name?: string;
};

export type JoinTeamErrors = {
  code?: string;
};

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

type RawTeamSummaryDto = {
  teamId?: string | null;
  TeamId?: string | null;
  teamID?: string | null;
  TeamID?: string | null;
  name?: string | null;
  Name?: string | null;
};

type RawInviteCodeDto = {
  codeId?: string | null;
  CodeId?: string | null;
  codeID?: string | null;
  CodeID?: string | null;
  code?: string | null;
  Code?: string | null;
  teamId?: string | null;
  TeamId?: string | null;
  teamID?: string | null;
  TeamID?: string | null;
  expiryDate?: string | null;
  ExpiryDate?: string | null;
};

type RawTeamMemberDto = {
  memberId?: string | null;
  MemberId?: string | null;
  memberID?: string | null;
  MemberID?: string | null;
  name?: string | null;
  Name?: string | null;
  email?: string | null;
  Email?: string | null;
  status?: TeamMemberStatus | string | number | null;
  Status?: TeamMemberStatus | string | number | null;
};

export function getStoredAuth(): StoredAuth | null {
  const rawAuth = localStorage.getItem("habithubAuth");

  if (!rawAuth) {
    return null;
  }

  try {
    return JSON.parse(rawAuth) as StoredAuth;
  } catch {
    localStorage.removeItem("habithubAuth");
    return null;
  }
}

export function clearStoredAuth(): void {
  localStorage.removeItem("habithubAuth");
}

export function getAuthHeaders(auth: StoredAuth | null): HeadersInit {
  return {
    "Content-Type": "application/json",
    ...(auth?.sessionId ? { "X-Session-Id": auth.sessionId } : {}),
  };
}

export function validateTeamName(name: string): string | undefined {
  const trimmedName = name.trim();

  if (!trimmedName) {
    return "Team name is required.";
  }

  if (trimmedName.length < 3) {
    return "Team name must be at least 3 characters long.";
  }

  if (trimmedName.length > 100) {
    return "Team name must be 100 characters or shorter.";
  }

  return undefined;
}

export function validateInviteCode(code: string): string | undefined {
  if (!code.trim()) {
    return "Invite code is required.";
  }

  if (code.trim().length !== 8) {
    return "Invite code must be exactly 8 characters long.";
  }

  return undefined;
}

export function validateCreateTeamForm(
  form: CreateTeamForm,
): CreateTeamErrors {
  return {
    name: validateTeamName(form.name),
  };
}

export function validateJoinTeamForm(form: JoinTeamForm): JoinTeamErrors {
  return {
    code: validateInviteCode(form.code),
  };
}

export function hasCreateTeamErrors(errors: CreateTeamErrors): boolean {
  return Boolean(errors.name);
}

export function hasJoinTeamErrors(errors: JoinTeamErrors): boolean {
  return Boolean(errors.code);
}

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
  const response = await requestJson<RawTeamSummaryDto[]>("/teams", {
    method: "GET",
    headers: getAuthHeaders(auth),
  });

  return response.map(normalizeTeamSummary).filter((team) => team.teamId);
}

export async function getTeam(
  auth: StoredAuth | null,
  teamId: string,
): Promise<TeamDetailsDto> {
  const response = await requestJson<RawTeamSummaryDto>(`/teams/${teamId}`, {
    method: "GET",
    headers: getAuthHeaders(auth),
  });

  return normalizeTeamSummary(response);
}

export async function createTeam(
  auth: StoredAuth | null,
  form: CreateTeamForm,
): Promise<TeamDetailsDto> {
  const response = await requestJson<RawTeamSummaryDto>("/teams", {
    method: "POST",
    headers: getAuthHeaders(auth),
    body: JSON.stringify({
      name: form.name.trim(),
    }),
  });

  return normalizeTeamSummary(response);
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
  const response = await requestJson<RawTeamMemberDto[]>(
    `/teams/${teamId}/members`,
    {
      method: "GET",
      headers: getAuthHeaders(auth),
    },
  );

  return response.map(normalizeTeamMember).filter((member) => member.memberId);
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
  const response = await requestJson<RawInviteCodeDto[]>(
    `/teams/${teamId}/invite-codes`,
    {
      method: "GET",
      headers: getAuthHeaders(auth),
    },
  );

  return response.map(normalizeInviteCode).filter((code) => code.codeId);
}

export async function generateInviteCode(
  auth: StoredAuth | null,
  teamId: string,
): Promise<InviteCodeDto> {
  const response = await requestJson<RawInviteCodeDto>(
    `/teams/${teamId}/invite-codes`,
    {
      method: "POST",
      headers: getAuthHeaders(auth),
    },
  );

  return normalizeInviteCode(response);
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
): Promise<TeamDetailsDto> {
  const response = await requestJson<RawTeamSummaryDto>("/teams/join", {
    method: "POST",
    headers: getAuthHeaders(auth),
    body: JSON.stringify({
      code: form.code.trim(),
    }),
  });

  return normalizeTeamSummary(response);
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

function normalizeTeamSummary(raw: RawTeamSummaryDto): TeamSummaryDto {
  return {
    teamId: raw.teamId ?? raw.TeamId ?? raw.teamID ?? raw.TeamID ?? "",
    name: raw.name ?? raw.Name ?? "Unnamed team",
  };
}

function normalizeInviteCode(raw: RawInviteCodeDto): InviteCodeDto {
  return {
    codeId: raw.codeId ?? raw.CodeId ?? raw.codeID ?? raw.CodeID ?? "",
    code: raw.code ?? raw.Code ?? "",
    teamId: raw.teamId ?? raw.TeamId ?? raw.teamID ?? raw.TeamID ?? "",
    expiryDate: raw.expiryDate ?? raw.ExpiryDate ?? "",
  };
}

function normalizeTeamMember(raw: RawTeamMemberDto): TeamMemberDto {
  return {
    memberId:
      raw.memberId ?? raw.MemberId ?? raw.memberID ?? raw.MemberID ?? "",
    name: raw.name ?? raw.Name ?? "Unknown",
    email: raw.email ?? raw.Email ?? "Unknown",
    status: normalizeMembershipStatus(raw.status ?? raw.Status),
  };
}

function normalizeMembershipStatus(
  status: TeamMemberStatus | string | number | null | undefined,
): TeamMemberStatus {
  if (status === "Active" || status === 0) {
    return "Active";
  }

  if (status === "Kicked" || status === 1) {
    return "Kicked";
  }

  if (status === "Left" || status === 2) {
    return "Left";
  }

  return "Active";
}
