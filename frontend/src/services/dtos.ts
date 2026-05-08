export type UserTypeDto = 0 | 1;

export type SessionStateDto = 0 | 1 | 2;

export type MembershipStatusDto = 0 | 1 | 2;

export type HabitStateDto = 0 | 1 | 2;

export type HabitTypeDto = 0 | 1;

export type UnitDto = 0 | 1 | 2 | 3 | 4 | 5 | 6;

export type UserDto = {
  id: string;
  name: string;
  email: string;
  userType: UserTypeDto;
  timezone: string | null;
};

export type SessionDto = {
  sessionId: string;
  userType: UserTypeDto;
  createdAt: string;
  lastActiveAt: string;
  expiresAt: string;
  sessionState: SessionStateDto;
  isCurrent: boolean;
  deviceInfo: string | null;
  ipAddress: string | null;
};

export type RegisterRequestDto = {
  name: string;
  email: string;
  password: string;
  timezone: string;
  userType: UserTypeDto;
};

export type LoginRequestDto = {
  email: string;
  password: string;
  userType: UserTypeDto;
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
  status: MembershipStatusDto;
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
  habitState: HabitStateDto;
  habitType: HabitTypeDto;
  unit: UnitDto | null;
  expiryDate: string | null;
};

export type CreateHabitRequestDto = {
  name: string;
  goal: string | null;
  habitType: HabitTypeDto;
  unit: UnitDto | null;
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
