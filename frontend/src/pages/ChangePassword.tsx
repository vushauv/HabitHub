import { Link, useNavigate } from "react-router-dom";
import { useMemo, useState, type ChangeEvent, type FormEvent } from "react";
import "./ChangePassword.css";
import "../App.css";
import type { ChangePasswordRequestDto } from "../services/dtos";
import {
  API_BASE_URL,
  validatePassword,
  type AccountType,
} from "../services/User";

type StoredAuth = {
  isLoggedIn?: boolean;
  userType?: AccountType;
  sessionId?: string | null;
  userId?: string | null;
};

type ChangePasswordErrors = {
  currentPassword?: string;
  newPassword?: string;
};

type ChangePasswordErrorCode =
  | "auth-required"
  | "validation-error"
  | "invalid-credentials"
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

function validateCurrentPassword(currentPassword: string): string | undefined {
  if (!currentPassword) {
    return "Current password is required.";
  }

  return undefined;
}

function validateChangePasswordForm(
  form: ChangePasswordRequestDto,
): ChangePasswordErrors {
  return {
    currentPassword: validateCurrentPassword(form.currentPassword),
    newPassword: validatePassword(form.newPassword),
  };
}

function hasErrors(errors: ChangePasswordErrors): boolean {
  return Object.values(errors).some(Boolean);
}

function getErrorCode(
  status: number,
  responseText: string,
): ChangePasswordErrorCode {
  if (status === 401) {
    if (responseText.includes("invalid-credentials")) {
      return "invalid-credentials";
    }

    return "auth-required";
  }

  if (status === 400) {
    return "validation-error";
  }

  return "unknown";
}

function getFriendlyErrorMessage(errorCode: ChangePasswordErrorCode): string {
  switch (errorCode) {
    case "auth-required":
      return "Your session is no longer valid. Please log in again.";
    case "validation-error":
      return "The password change request is invalid. Please check your input.";
    case "invalid-credentials":
      return "Your current password is incorrect.";
    default:
      return "Something went wrong while changing your password. Please try again.";
  }
}

export default function ChangePassword() {
  const navigate = useNavigate();
  const auth = useMemo(() => getStoredAuth(), []);
  const [form, setForm] = useState<ChangePasswordRequestDto>({
    currentPassword: "",
    newPassword: "",
  });
  const [errors, setErrors] = useState<ChangePasswordErrors>({});
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
        name === "currentPassword"
          ? validateCurrentPassword(value)
          : validatePassword(value),
    }));

    setFormError(null);
    setSuccessMessage(null);
  };

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();

    const validationErrors = validateChangePasswordForm(form);
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
      const payload: ChangePasswordRequestDto = {
        currentPassword: form.currentPassword,
        newPassword: form.newPassword,
      };

      const response = await fetch(`${API_BASE_URL}/auth/change-password`, {
        method: "POST",
        headers: getAuthHeaders(auth),
        body: JSON.stringify(payload),
      });

      if (!response.ok) {
        const responseText = await response.text().catch(() => "");
        const errorCode = getErrorCode(response.status, responseText);

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
        "Password changed successfully. Other active sessions should now be invalidated.",
      );

      setForm({
        currentPassword: "",
        newPassword: "",
      });

      setErrors({});
    } catch (error) {
      const message =
        error instanceof Error
          ? error.message
          : "Failed to change password.";

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

        <div className="card change-password-card-shell">
          <div className="content change-password-content">
            <div className="change-password-topbar">
              <Link
                to="/settings"
                className="button button-secondary change-password-back-button"
              >
                Back to settings
              </Link>
            </div>

            <div className="content-centered change-password-header">
              <h1 className="title change-password-title">Change password</h1>
              <p className="text change-password-text">
                Update your password securely. After a successful change, other
                active sessions should be invalidated by the server.
              </p>
            </div>

            <form
              className="change-password-form"
              onSubmit={handleSubmit}
              noValidate
            >
              <div className="form-field">
                <label className="form-label" htmlFor="currentPassword">
                  Current password
                </label>
                <input
                  id="currentPassword"
                  name="currentPassword"
                  type="password"
                  className="form-input"
                  value={form.currentPassword}
                  onChange={handleChange}
                  placeholder="Enter your current password"
                  aria-invalid={errors.currentPassword ? "true" : "false"}
                  autoComplete="current-password"
                />
                {errors.currentPassword ? (
                  <p className="field-error">{errors.currentPassword}</p>
                ) : null}
              </div>

              <div className="form-field">
                <label className="form-label" htmlFor="newPassword">
                  New password
                </label>
                <input
                  id="newPassword"
                  name="newPassword"
                  type="password"
                  className="form-input"
                  value={form.newPassword}
                  onChange={handleChange}
                  placeholder="Enter your new password"
                  aria-invalid={errors.newPassword ? "true" : "false"}
                  autoComplete="new-password"
                />
                {errors.newPassword ? (
                  <p className="field-error">{errors.newPassword}</p>
                ) : null}
              </div>

              {formError ? <p className="form-error">{formError}</p> : null}

              {successMessage ? (
                <p className="change-password-success">{successMessage}</p>
              ) : null}

              <button
                type="submit"
                className="button button-primary form-submit change-password-submit"
                disabled={loading}
              >
                {loading ? "Updating..." : "Update password"}
              </button>
            </form>
          </div>
        </div>
      </section>
    </main>
  );
}
