export const API_BASE_URL = import.meta.env.VITE_API_URL || "http://localhost:5000";

export type AccountType = "Creator" | "Member";

export const TIMEZONE_OPTIONS = [
  "Europe/Warsaw",
  "Europe/London",
  "Europe/Berlin",
  "Europe/Paris",
  "Europe/Madrid",
  "Europe/Rome",
  "UTC",
  "America/New_York",
  "America/Chicago",
  "America/Denver",
  "America/Los_Angeles",
  "Asia/Tokyo",
  "Asia/Seoul",
  "Asia/Singapore",
  "Australia/Sydney",
];

export const DEFAULT_TIMEZONE =
  Intl.DateTimeFormat().resolvedOptions().timeZone || "Europe/Warsaw";


export function validateEmail(email: string): string | undefined {
  if (!email.trim()) {
    return "Email is required.";
  }

  const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

  if (!emailPattern.test(email)) {
    return "Enter a valid email address.";
  }

  return undefined;
}

export function validateName(name: string): string | undefined {
  if (!name.trim()) {
    return "Name is required.";
  }

  if (name.trim().length < 2) {
    return "Name must be at least 2 characters long.";
  }

  return undefined;
}

export function validatePassword(password: string): string | undefined {
  if (!password) {
    return "Password is required.";
  }

  if (password.length < 8) {
    return "Password must be at least 8 characters long.";
  }

  return undefined;
}

export function validateUserType(userType: AccountType): string | undefined {
  if (userType !== "Creator" && userType !== "Member") {
    return "Choose an account type.";
  }

  return undefined;
}


export function mapUserTypeToEnum(userType: AccountType): number {
  return userType === "Creator" ? 0 : 1;
}

export function validateTimezone(timezone: string): string | undefined {
  if (!timezone.trim()) {
    return "Timezone is required.";
  }

  if (!TIMEZONE_OPTIONS.includes(timezone)) {
    return "Choose a valid timezone.";
  }

  return undefined;
}