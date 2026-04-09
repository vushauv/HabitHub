import {type AccountType, validateEmail, validatePassword, validateUserType } from "./User";

export type LoginForm = {
  email: string;
  password: string;
  userType: AccountType;
};

export type LoginErrors = {
  email?: string;
  password?: string;
  userType?: string;
};


export function validateForm(form: LoginForm): LoginErrors {
  return {
    email: validateEmail(form.email),
    password: validatePassword(form.password),
    userType: validateUserType(form.userType),
  };
}

export function hasValidationErrors(errors: LoginErrors): boolean {
  return Boolean(errors.email || errors.password || errors.userType);
}