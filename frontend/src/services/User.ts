import type { UserTypeDto } from "./dtos";
import * as z from "zod";

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


export const emailSchema = z
  .email({error: "Enter a valid email address."})
  .trim()
  .nonempty({error: "Email is required."});

export const nameSchema = z
  .string()
  .trim()
  .nonempty({error: "Name is required."})
  .min(2, { error: "Name must be at least 2 characters long." });

export const passwordSchema = z
  .string()
  .nonempty({ error: "Password is required." })
  .min(8, { error: "Password must be at least 8 characters long." });

export const userTypeSchema = z
  .enum(["Creator", "Member"])
  .nonoptional({ error: "Choose an account type." });

export function mapUserTypeToEnum(userType: AccountType): UserTypeDto {
  return userType === "Creator" ? 0 : 1;
}

export function mapUserTypeFromEnum(userType: UserTypeDto): AccountType {
  return userType === 0 ? "Creator" : "Member";
}

export const timezoneSchema = z
  .enum(TIMEZONE_OPTIONS, { error: "Choose a valid timezone." });