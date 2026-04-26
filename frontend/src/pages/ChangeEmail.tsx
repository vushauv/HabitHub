import { Link, useNavigate } from "react-router-dom";
import { useEffect, useMemo, useState } from "react";
import "./ChangeEmail.css";
import "../App.css";
import type { ChangeEmailRequestDto } from "../services/dtos";
import {
  API_BASE_URL,
  emailSchema,
  type AccountType,
} from "../services/User";
import z from "zod";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useLens } from "@hookform/lenses";
import TextInput from "../components/form/TextInput";
import SubmitButton from "../components/form/SubmitButton";

type StoredAuth = {
  isLoggedIn?: boolean;
  userType?: AccountType;
  sessionId?: string | null;
  userId?: string | null;
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

const currentPasswordSchema = z
  .string()
  .nonempty({ error: "Current password is required." });

const changeEmailFormSchema = z
  .object({
    newEmail: emailSchema,
    password: currentPasswordSchema
  })
  .required();

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
  const [loading, setLoading] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  const { handleSubmit, control, formState, subscribe, setValues } = useForm<ChangeEmailRequestDto>({
    defaultValues: {
      newEmail: "",
      password: ""
    },
    disabled: loading,
    resolver: zodResolver(changeEmailFormSchema),
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

  const onSubmit = async (form: ChangeEmailRequestDto) => {
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
      const payload: ChangeEmailRequestDto = {
        newEmail: form.newEmail,
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

      setValues({
        newEmail: "",
        password: "",
      });

      setSuccessMessage(
        "Email changed successfully. Your current session may no longer be valid, so please log in again if needed.",
      );
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

            <form className="change-email-form" onSubmit={handleSubmit(onSubmit)} noValidate>
              <TextInput
                label="New email"
                lens={lens.focus("newEmail")}
                required
                type="email"
                placeholder="Enter your new email"
                autoComplete="email"
              />

              <TextInput
                label="Password"
                lens={lens.focus("password")}
                required
                type="password"
                autoComplete="current-password"
                placeholder="Enter your current password"
              />

              {formError ? <p className="form-error" role="alert">{formError}</p> : null}

              {successMessage ? (
                <p className="change-email-success" role="alert">{successMessage}</p>
              ) : null}

              <SubmitButton
                formState={formState}
                disabled={loading}
              >
                {loading ? "Updating..." : "Update email"}
              </SubmitButton>
            </form>
          </div>
        </div>
      </section>
    </main>
  );
}
