import z from "zod";
import { emailSchema, nameSchema, passwordSchema, timezoneSchema, userTypeSchema, type AccountType } from "./User";

export type RegisterForm = {
  name: string;
  email: string;
  password: string;
  timezone: string;
  userType: AccountType;
};

export const registerFormSchema = z
  .object({
    name: nameSchema,
    email: emailSchema,
    password: passwordSchema,
    timezone: timezoneSchema,
    userType: userTypeSchema
  })
  .required();