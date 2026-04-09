import { type AccountType, validateEmail, validateName, validatePassword, validateTimezone, validateUserType } from "./User";

export type RegisterForm = {
  name: string;
  email: string;
  password: string;
  timezone: string;
  userType: AccountType;
};

export type RegisterErrors = {
  name?: string;
  email?: string;
  password?: string;
  timezone?: string;
  userType?: string;
};

export function validateForm(form: RegisterForm): RegisterErrors {
  return {
    name: validateName(form.name),
    email: validateEmail(form.email),
    password: validatePassword(form.password),
    timezone: validateTimezone(form.timezone),
    userType: validateUserType(form.userType),
  };
}

export function hasValidationErrors(errors: RegisterErrors): boolean {
  return Boolean(
    errors.name ||
      errors.email ||
      errors.password ||
      errors.timezone ||
      errors.userType,
  );
}