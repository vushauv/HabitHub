export type UserTypeDto = 0 | 1;

export type SessionStateDto = 0 | 1 | 2;

export type MembershipStatusDto = 0 | 1 | 2;

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
