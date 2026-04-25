import { Link, useNavigate } from "react-router-dom";
import { useMemo, useState, type ChangeEvent, type FormEvent } from "react";
import "./ChangeEmail.css";
import "../App.css";
import type { ChangeEmailRequestDto } from "../services/dtos";
import {
  API_BASE_URL,
  validateEmail,
  type AccountType,
} from "../services/User";

type StoredAuth = {
  isLoggedIn?: boolean;
  userType?: AccountType;
  sessionId?: string | null;
  userId?: string | null;
};

type ChangeEmailErrors = {
  newEmail?: string;
  password?: string;
};

type ChangeEmailErrorCode =
  | "auth-required"
  | "validation-error"
  | "invalid-credentials"
  | "email-already-exists"
  | "unknown";

function getStoredAuth(): StoredAuth | null {
  const rawAuth = localStorage.getItem("habithubAuth");

  if (!rawAuth) {
    return null;
  }

  try {
    return JSON.parse(rawAuth) as StoredAuth;
  } catch {
    localStorage.removeItem("habithubAuth");
    return null;
  }
}

function clearStoredAuth(): void {
  localStorage.removeItem("habithubAuth");
}

function getAuthHeaders(auth: StoredAuth | null): HeadersInit {
  return {
    "Content-Type": "application/json",
    ...(auth?.sessionId ? { "X-Session-Id": auth.sessionId } : {}),
  };
}

function validateCurrentPassword(password: string): string | undefined {
  if (!password) {
    return "Password is required.";
  }

  return undefined;
}

function validateChangeEmailForm(form: ChangeEmailRequestDto): ChangeEmailErrors {
  return {
    newEmail: validateEmail(form.newEmail),
    password: validateCurrentPassword(form.password),
  };
}

function hasErrors(errors: ChangeEmailErrors): boolean {
  return Object.values(errors).some(Boolean);
}

function getFriendlyErrorMessage(errorCode: ChangeEmailErrorCode): string {
  switch (errorCode) {
    case "auth-required":
      return "Your session is no longer valid. Please log in again.";
    case "validation-error":
      return "The email change request is invalid. Please check your input.";
    case "invalid-credentials":
      return "Your password is incorrect.";
    case "email-already-exists":
      return "This email address is already in use.";
    default:
      return "Something went wrong while changing your email. Please try again.";
  }
}

function resolveChangeEmailErrorCode(
  status: number,
  responseText: string,
): ChangeEmailErrorCode {
  if (status === 400) {
    return "validation-error";
  }

  if (status === 409) {
    return "email-already-exists";
  }

  if (status === 401) {
    return responseText.includes("invalid-credentials")
      ? "invalid-credentials"
      : "auth-required";
  }

  return "unknown";
}

export default function ChangeEmail() {
  const navigate = useNavigate();
  const auth = useMemo(() => getStoredAuth(), []);
  const [form, setForm] = useState<ChangeEmailRequestDto>({
    newEmail: "",
    password: "",
  });
  const [errors, setErrors] = useState<ChangeEmailErrors>({});
  const [formError, setFormError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const handleChange = (event: ChangeEvent<HTMLInputElement>) => {
    const { name, value } = event.target;

    setForm((currentForm) => ({
      ...currentForm,
      [name]: value,
    }));

    setErrors((currentErrors) => ({
      ...currentErrors,
      [name]:
        name === "newEmail"
          ? validateEmail(value)
          : validateCurrentPassword(value),
    }));

    setFormError(null);
    setSuccessMessage(null);
  };

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();

    const validationErrors = validateChangeEmailForm(form);
    setErrors(validationErrors);
    setFormError(null);
    setSuccessMessage(null);

    if (hasErrors(validationErrors)) {
      return;
    }

    if (!auth?.isLoggedIn || !auth.sessionId) {
      setFormError("Your session is no longer valid. Please log in again.");
      clearStoredAuth();

      setTimeout(() => {
        navigate("/login", { replace: true });
      }, 1200);

      return;
    }

    setLoading(true);

    try {
      const payload: ChangeEmailRequestDto = {
        newEmail: form.newEmail.trim(),
        password: form.password,
      };

      const response = await fetch(`${API_BASE_URL}/auth/change-email`, {
        method: "POST",
        headers: getAuthHeaders(auth),
        body: JSON.stringify(payload),
      });

      if (!response.ok) {
        const responseText = await response.text().catch(() => "");
        const errorCode = resolveChangeEmailErrorCode(
          response.status,
          responseText,
        );

        if (errorCode === "auth-required") {
          clearStoredAuth();
          setFormError(getFriendlyErrorMessage(errorCode));

          setTimeout(() => {
            navigate("/login", { replace: true });
          }, 1200);

          return;
        }

        throw new Error(getFriendlyErrorMessage(errorCode));
      }

      setSuccessMessage(
        "Email changed successfully. Your current session may no longer be valid, so please log in again if needed.",
      );

      setForm({
        newEmail: "",
        password: "",
      });

      setErrors({});
    } catch (error) {
      const message =
        error instanceof Error ? error.message : "Failed to change email.";

      setFormError(message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <main className="page">
      <section className="container">
        <div className="background-glow background-glow-left" />
        <div className="background-glow background-glow-right" />

        <div className="card change-email-card-shell">
          <div className="content change-email-content">
            <div className="change-email-topbar">
              <Link
                to="/settings"
                className="button button-secondary change-email-back-button"
              >
                Back to settings
              </Link>
            </div>

            <div className="content-centered change-email-header">
              <h1 className="title change-email-title">Change email</h1>
              <p className="text change-email-text">
                Update the email connected to your account. The new email must be
                unique and your password is required to confirm identity.
              </p>
            </div>

            <form className="change-email-form" onSubmit={handleSubmit} noValidate>
              <div className="form-field">
                <label className="form-label" htmlFor="newEmail">
                  New email
                </label>
                <input
                  id="newEmail"
                  name="newEmail"
                  type="email"
                  className="form-input"
                  value={form.newEmail}
                  onChange={handleChange}
                  placeholder="Enter your new email"
                  aria-invalid={errors.newEmail ? "true" : "false"}
                  autoComplete="email"
                />
                {errors.newEmail ? (
                  <p className="field-error">{errors.newEmail}</p>
                ) : null}
              </div>

              <div className="form-field">
                <label className="form-label" htmlFor="password">
                  Password
                </label>
                <input
                  id="password"
                  name="password"
                  type="password"
                  className="form-input"
                  value={form.password}
                  onChange={handleChange}
                  placeholder="Enter your current password"
                  aria-invalid={errors.password ? "true" : "false"}
                  autoComplete="current-password"
                />
                {errors.password ? (
                  <p className="field-error">{errors.password}</p>
                ) : null}
              </div>

              {formError ? <p className="form-error">{formError}</p> : null}

              {successMessage ? (
                <p className="change-email-success">{successMessage}</p>
              ) : null}

              <button
                type="submit"
                className="button button-primary form-submit change-email-submit"
                disabled={loading}
              >
                {loading ? "Updating..." : "Update email"}
              </button>
            </form>
          </div>
        </div>
      </section>
    </main>
  );
}
