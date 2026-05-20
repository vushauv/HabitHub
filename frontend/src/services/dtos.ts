import type { AccountType } from "./User";
import type { SessionState } from "./Auth";
import type { TeamMemberStatus } from "./Team";
import type {
  EntryStatusName,
  HabitState,
  HabitTypeName,
  UnitName,
} from "./Habit";

export type UserDto = {
  id: string;
  name: string;
  email: string;
  userType: AccountType;
  timezone: string | null;
};

export type SessionDto = {
  sessionId: string;
  userType: AccountType;
  createdAt: string;
  lastActiveAt: string;
  expiresAt: string;
  sessionState: SessionState;
  isCurrent: boolean;
  deviceInfo: string | null;
  ipAddress: string | null;
};

export type RegisterRequestDto = {
  name: string;
  email: string;
  password: string;
  timezone: string;
  userType: AccountType;
};

export type LoginRequestDto = {
  email: string;
  password: string;
  userType: AccountType;
};

export type ChangePasswordRequestDto = {
  currentPassword: string;
  newPassword: string;
};

export type ChangeEmailRequestDto = {
  newEmail: string;
  password: string;
};

export type AuthResponseDto = {
  sessionId: string;
  user: UserDto;
};

export type TeamSummaryDto = {
  teamId: string;
  name: string;
};

export type TeamDetailsDto = TeamSummaryDto;

export type TeamMemberDto = {
  memberId: string;
  name: string;
  email: string;
  status: TeamMemberStatus;
};

export type InviteCodeDto = {
  codeId: string;
  code: string;
  teamId: string;
  expiryDate: string;
};

export type CreateTeamRequestDto = {
  name: string;
};

export type CreateTeamResponseDto = TeamSummaryDto;

export type JoinTeamRequestDto = {
  code: string;
};

export type JoinTeamResponseDto = {
  teamId: string;
  memberId: string;
};

export type HabitSummaryDto = {
  habitId: string;
  name: string;
  goal: string | null;
  habitState: HabitState;
  habitType: HabitTypeName;
  unit: UnitName | null;
  expiryDate: string | null;
};

export type CreateHabitRequestDto = {
  name: string;
  goal: string | null;
  habitType: HabitTypeName;
  unit: UnitName | null;
  expiryDate: string | null;
};

export type CreateHabitResponseDto = HabitSummaryDto & {
  teamId: string;
  creatorId: string;
};

export type EditHabitRequestDto = {
  name?: string;
  goal?: string | null;
  expiryDate?: string | null;
  clearGoal?: boolean;
  clearExpiryDate?: boolean;
};

export type LeaderboardResponseDto = {
  memberId: string;
  memberName: string;
  totalValue: number | null;
  loggedCount: number;
  rank: number;
};

export type HabitEntryResponseDto = {
  entryId: string;
  habitId: string;
  memberId: string;
  loggedAt: string;
  logDate: string;
  status: EntryStatusName;
  value: number | null;
  notes: string | null;
};

export type LogProgressRequestDto = {
  value: number | null;
  notes: string | null;
  status: EntryStatusName;
};

export type ChatMessageDto = {
  messageId: string;
  chatId: string;
  userId: string;
  authorName: string;
  content: string;
  sendDate: string;
};

export type SendChatMessageRequestDto = {
  content: string;
};

export type TodayHabitEntryStatusDto = {
  status: EntryStatusName;
  entry: HabitEntryResponseDto | null;
};
