import { Link, useNavigate } from "react-router-dom";
import { useEffect, useMemo, useState } from "react";
import "./ChangePassword.css";
import "../App.css";
import type { ChangePasswordRequestDto } from "../services/dtos";
import {
  API_BASE_URL,
  passwordSchema,
  type AccountType,
} from "../services/User";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import z from "zod";
import { useLens } from "@hookform/lenses";
import TextInput from "../components/form/TextInput";
import SubmitButton from "../components/form/SubmitButton";

type StoredAuth = {
  isLoggedIn?: boolean;
  userType?: AccountType;
  sessionId?: string | null;
  userId?: string | null;
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

const currentPasswordSchema = z
  .string()
  .nonempty({ error: "Current password is required." });

const changePasswordFormSchema = z
  .object({
    currentPassword: currentPasswordSchema,
    newPassword: passwordSchema
  })
  .required();

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
  const [loading, setLoading] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  const { handleSubmit, control, formState, subscribe, setValues } = useForm<ChangePasswordRequestDto>({
    defaultValues: {
      currentPassword: "",
      newPassword: "",
    },
    disabled: loading,
    resolver: zodResolver(changePasswordFormSchema),
    mode: "all"
  });

  const lens = useLens({ control });
  
  // To clear the server error on any field change
  useEffect(() => {
    const callback = subscribe({
      formState: {
        values: true,
      },
      callback: () => {
        setFormError(null);
        setSuccessMessage(null);
      },
    })

    return () => callback()
  }, [subscribe]);

  const onSubmit = async (form: ChangePasswordRequestDto) => {
    setFormError(null);
    setSuccessMessage(null);

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

      setValues({
        currentPassword: "",
        newPassword: "",
      });

      setSuccessMessage(
        "Password changed successfully. Other active sessions should now be invalidated.",
      );
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

        <div className="card page-card-shell">
          <div className="content change-password-content">
            <div className="page-topbar">
              <Link
                to="/settings"
                className="button button-secondary page-nav-button"
              >
                Back to settings
              </Link>
            </div>

            <div className="content-centered">
              <h1 className="title page-title">Change password</h1>
              <p className="text change-password-text">
                Update your password securely. After a successful change, other
                active sessions should be invalidated by the server.
              </p>
            </div>

            <form
              className="change-password-form"
              onSubmit={handleSubmit(onSubmit)}
              noValidate
            >
              <TextInput
                label="Current password"
                lens={lens.focus("currentPassword")}
                type="password"
                required
                placeholder="Enter your current password"
                autoComplete="current-password"
              />

              <TextInput
                label="New password"
                lens={lens.focus("newPassword")}
                type="password"
                required
                placeholder="Enter your new password"
                autoComplete="new-password"
              />

              {formError ? <p className="form-error" role="alert">{formError}</p> : null}

              {successMessage ? (
                <p className="alert-success">{successMessage}</p>
              ) : null}

              <SubmitButton
                formState={formState}
                disabled={loading}
              >
                {loading ? "Updating..." : "Update password"}
              </SubmitButton>
            </form>
          </div>
        </div>
      </section>
    </main>
  );
}
