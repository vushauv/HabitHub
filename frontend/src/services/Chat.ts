import z from "zod";
import { API_BASE_URL } from "./User";
import { getAuthHeaders, type StoredAuth } from "./Auth";
import type {
  ChatMessageDto,
  SendChatMessageRequestDto,
} from "./dtos";

export type { ChatMessageDto, SendChatMessageRequestDto } from "./dtos";
export { clearStoredAuth, getStoredAuth, type StoredAuth } from "./Auth";

export type ChatErrorCode =
  | "auth-required"
  | "validation-error"
  | "forbidden"
  | "not-found"
  | "message-not-found"
  | "message-not-own"
  | "unknown";

export class ChatRequestError extends Error {
  status: number;
  code: ChatErrorCode;

  constructor(status: number, code: ChatErrorCode, message: string) {
    super(message);
    this.name = "ChatRequestError";
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

export const messageContentSchema = z
  .string()
  .trim()
  .nonempty({ error: "Message cannot be empty." })
  .max(2000, { error: "Message must be 2000 characters or shorter." });

export function getChatErrorMessage(errorCode: ChatErrorCode): string {
  switch (errorCode) {
    case "auth-required":
      return "Your session is no longer valid. Please log in again.";
    case "validation-error":
      return "Message content is invalid.";
    case "forbidden":
      return "You do not have permission to access this chat.";
    case "not-found":
      return "The selected team could not be found.";
    case "message-not-found":
      return "The message could not be found.";
    case "message-not-own":
      return "You can only delete your own messages.";
    default:
      return "Something went wrong with the chat. Please try again.";
  }
}

export async function getMessages(
  auth: StoredAuth | null,
  teamId: string,
  options?: { count?: number; offset?: number },
): Promise<ChatMessageDto[]> {
  const params = new URLSearchParams();

  if (options?.count !== undefined) {
    params.set("count", String(options.count));
  }

  if (options?.offset !== undefined) {
    params.set("offset", String(options.offset));
  }

  const query = params.toString();
  const suffix = query ? `?${query}` : "";

  return requestJson<ChatMessageDto[]>(
    `/teams/${teamId}/chat/messages${suffix}`,
    {
      method: "GET",
      headers: getAuthHeaders(auth),
    },
  );
}

export async function sendMessage(
  auth: StoredAuth | null,
  teamId: string,
  content: string,
): Promise<ChatMessageDto> {
  const body: SendChatMessageRequestDto = { content: content.trim() };

  return requestJson<ChatMessageDto>(
    `/teams/${teamId}/chat/messages`,
    {
      method: "POST",
      headers: getAuthHeaders(auth),
      body: JSON.stringify(body),
    },
  );
}

export async function deleteMessage(
  auth: StoredAuth | null,
  teamId: string,
  messageId: string,
): Promise<void> {
  await requestEmpty(`/teams/${teamId}/chat/messages/${messageId}`, {
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
    throw await createChatRequestError(response);
  }

  return (await response.json()) as T;
}

async function requestEmpty(
  path: string,
  options: RequestInit,
): Promise<void> {
  const response = await fetch(`${API_BASE_URL}${path}`, options);

  if (!response.ok) {
    throw await createChatRequestError(response);
  }
}

async function createChatRequestError(
  response: Response,
): Promise<ChatRequestError> {
  const responseText = await response.text().catch(() => "");
  const parsedError = parseErrorResponse(responseText);
  const code = normalizeErrorCode(
    parsedError.error ?? parsedError.Error ?? responseText,
  );
  const message =
    parsedError.message ??
    parsedError.Message ??
    getChatErrorMessage(code);

  return new ChatRequestError(response.status, code, message);
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

function normalizeErrorCode(rawCode: string | null | undefined): ChatErrorCode {
  const knownCodes: ChatErrorCode[] = [
    "auth-required",
    "validation-error",
    "forbidden",
    "not-found",
    "message-not-found",
    "message-not-own",
  ];

  return knownCodes.includes(rawCode as ChatErrorCode)
    ? (rawCode as ChatErrorCode)
    : "unknown";
}
