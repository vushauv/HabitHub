import type { UserDto } from "./dtos";
import { API_BASE_URL, mapUserTypeFromEnum, type AccountType } from "./User";

const AUTH_STORAGE_KEY = "habithubAuth";

export type StoredAuth = {
  sessionId: string;
};

export type AuthErrorCode = "auth-required" | "not-found" | "unknown";

type RawErrorResponse = {
  error?: string | null;
  message?: string | null;
  Error?: string | null;
  Message?: string | null;
};

export class AuthRequestError extends Error {
  status: number;
  code: AuthErrorCode;

  constructor(status: number, code: AuthErrorCode, message: string) {
    super(message);
    this.name = "AuthRequestError";
    this.status = status;
    this.code = code;
  }
}

export function getStoredAuth(): StoredAuth | null {
  const rawAuth = localStorage.getItem(AUTH_STORAGE_KEY);

  if (!rawAuth) {
    return null;
  }

  try {
    const parsedAuth = JSON.parse(rawAuth) as unknown;
    const normalizedAuth = normalizeStoredAuth(parsedAuth);

    if (!normalizedAuth) {
      clearStoredAuth();
      return null;
    }

    // Rewrite older payloads so only session state remains in local storage.
    if (rawAuth !== JSON.stringify(normalizedAuth)) {
      setStoredAuth(normalizedAuth.sessionId);
    }

    return normalizedAuth;
  } catch {
    clearStoredAuth();
    return null;
  }
}

export function setStoredAuth(sessionId: string): StoredAuth {
  const auth = {
    sessionId,
  };

  localStorage.setItem(
    AUTH_STORAGE_KEY,
    JSON.stringify(auth),
  );

  return auth;
}

export function clearStoredAuth(): void {
  localStorage.removeItem(AUTH_STORAGE_KEY);
}

export function getAuthHeaders(auth: StoredAuth | null): HeadersInit {
  return {
    "Content-Type": "application/json",
    ...(auth?.sessionId ? { "X-Session-Id": auth.sessionId } : {}),
  };
}

export function getAccountTypeForUser(
  user: Pick<UserDto, "userType">,
): AccountType {
  return mapUserTypeFromEnum(user.userType);
}

export function getDashboardPathForUser(
  user: Pick<UserDto, "userType">,
): string {
  return getAccountTypeForUser(user) === "Creator"
    ? "/creator"
    : "/member";
}

export async function getCurrentUser(auth: StoredAuth | null): Promise<UserDto> {
  if (!auth) {
    throw new AuthRequestError(
      401,
      "auth-required",
      "Your session is no longer valid. Please log in again.",
    );
  }

  const response = await fetch(`${API_BASE_URL}/auth/me`, {
    method: "GET",
    headers: getAuthHeaders(auth),
  });

  if (!response.ok) {
    throw await createAuthRequestError(response);
  }

  return (await response.json()) as UserDto;
}

export async function storeSessionAndLoadCurrentUser(
  sessionId: string,
): Promise<UserDto> {
  const auth = setStoredAuth(sessionId);

  try {
    return await getCurrentUser(auth);
  } catch (error) {
    if (
      error instanceof AuthRequestError &&
      (error.code === "auth-required" || error.code === "not-found")
    ) {
      clearStoredAuth();
    }

    throw error;
  }
}

function normalizeStoredAuth(
  rawAuth: unknown,
): StoredAuth | null {
  if (typeof rawAuth !== "object" || rawAuth === null) {
    return null;
  }

  const candidate = rawAuth as Record<string, unknown>;
  const sessionId =
    typeof candidate.sessionId === "string" && candidate.sessionId.trim()
      ? candidate.sessionId
      : null;

  if (!sessionId) {
    return null;
  }

  return {
    sessionId,
  };
}

function getAuthErrorMessage(errorCode: AuthErrorCode): string {
  switch (errorCode) {
    case "auth-required":
      return "Your session is no longer valid. Please log in again.";
    case "not-found":
      return "We could not find your account. Please log in again.";
    default:
      return "We could not load your account right now. Please try again.";
  }
}

async function createAuthRequestError(
  response: Response,
): Promise<AuthRequestError> {
  const responseText = await response.text().catch(() => "");
  const parsedError = parseErrorResponse(responseText);
  const code = normalizeAuthErrorCode(
    response.status,
    parsedError.error ?? parsedError.Error,
  );
  const message =
    parsedError.message ??
    parsedError.Message ??
    getAuthErrorMessage(code);

  return new AuthRequestError(response.status, code, message);
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

function normalizeAuthErrorCode(
  status: number,
  rawCode: string | null | undefined,
): AuthErrorCode {
  if (rawCode === "auth-required" || status === 401) {
    return "auth-required";
  }

  if (rawCode === "not-found" || status === 404) {
    return "not-found";
  }

  return "unknown";
}
