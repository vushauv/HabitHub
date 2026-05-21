import z from "zod";
import {emailSchema, passwordSchema, userTypeSchema, type AccountType } from "./User";

export type LoginForm = {
  email: string;
  password: string;
  userType: AccountType;
};

export const loginFormSchema = z
  .object({
    email: emailSchema,
    password: passwordSchema,
    userType: userTypeSchema
  })
  .required();