import { useMemo, useState, type ChangeEvent, type SubmitEvent } from "react";
import { Link, useNavigate } from "react-router-dom";
import "./Login.css";
import "../App.css";
import { type LoginForm } from "../services/Login";
import { validateForm, hasValidationErrors } from "../services/Login";
import {
  API_BASE_URL,
  type AccountType,
  mapUserTypeToEnum,
} from "../services/User";

type LoginResponse = {
  sessionId?: string | null;
  sessionID?: string | null;
  SessionId?: string | null;
  SessionID?: string | null;
  userId?: string | null;
  userID?: string | null;
  UserId?: string | null;
  UserID?: string | null;
  memberId?: string | null;
  memberID?: string | null;
  MemberId?: string | null;
  MemberID?: string | null;
  creatorId?: string | null;
  creatorID?: string | null;
  CreatorId?: string | null;
  CreatorID?: string | null;
  userType?: string | number | null;
  UserType?: string | number | null;
  name?: string | null;
  Name?: string | null;
};

function resolveAuthenticatedUserType(
  data: LoginResponse,
  fallback: AccountType,
): AccountType {
  const rawUserType = data.userType ?? data.UserType;

  if (
    rawUserType === "creator" ||
    rawUserType === "Creator" ||
    rawUserType === 0
  ) {
    return "Creator";
  }

  if (
    rawUserType === "member" ||
    rawUserType === "Member" ||
    rawUserType === 1
  ) {
    return "Member";
  }

  return fallback;
}

function resolveSessionId(data: LoginResponse): string | null {
  return (
    data.sessionId ??
    data.sessionID ??
    data.SessionId ??
    data.SessionID ??
    null
  );
}

function resolveUserId(data: LoginResponse): string | null {
  return (
    data.userId ??
    data.userID ??
    data.UserId ??
    data.UserID ??
    data.memberId ??
    data.memberID ??
    data.MemberId ??
    data.MemberID ??
    data.creatorId ??
    data.creatorID ??
    data.CreatorId ??
    data.CreatorID ??
    null
  );
}

function resolveName(data: LoginResponse): string {
  return data.name ?? data.Name ?? "John";
}

export default function Login() {
  const navigate = useNavigate();

  const [form, setForm] = useState<LoginForm>({
    email: "",
    password: "",
    userType: "Member",
  });

  const [touched, setTouched] = useState<Record<keyof LoginForm, boolean>>({
    email: false,
    password: false,
    userType: false,
  });

  const [serverError, setServerError] = useState("");
  const [loading, setLoading] = useState(false);

  const errors = useMemo(() => validateForm(form), [form]);
  const formIsValid = !hasValidationErrors(errors);

  function handleChange(
    field: keyof LoginForm,
    event: ChangeEvent<HTMLInputElement>,
  ) {
    const value = event.target.value;

    setForm((previousForm) => ({
      ...previousForm,
      [field]: value,
    }));

    setTouched((previousTouched) => ({
      ...previousTouched,
      [field]: true,
    }));

    setServerError("");
  }

  function handleBlur(field: keyof LoginForm) {
    setTouched((previousTouched) => ({
      ...previousTouched,
      [field]: true,
    }));
  }

  function handleUserTypeChange(userType: AccountType) {
    setForm((previousForm) => ({
      ...previousForm,
      userType,
    }));

    setTouched((previousTouched) => ({
      ...previousTouched,
      userType: true,
    }));

    setServerError("");
  }

  async function handleSubmit(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault();

    setTouched({
      email: true,
      password: true,
      userType: true,
    });

    setServerError("");

    const currentErrors = validateForm(form);

    if (hasValidationErrors(currentErrors)) {
      return;
    }

    setLoading(true);

    try {
      const payload = {
        email: form.email.trim(),
        password: form.password,
        userType: mapUserTypeToEnum(form.userType),
      };

      const response = await fetch(`${API_BASE_URL}/auth/login`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(payload),
      });

      if (!response.ok) {
        if (response.status === 401) {
          throw new Error("Invalid email, password, or account type.");
        }

        if (response.status === 400) {
          throw new Error("Please check the form fields and try again.");
        }

        const responseText = await response.text();
        throw new Error(responseText || `Login failed (${response.status}).`);
      }

      const data = (await response.json()) as LoginResponse;

      const authenticatedUserType = resolveAuthenticatedUserType(
        data,
        form.userType,
      );

      const sessionId = resolveSessionId(data);
      const userId = resolveUserId(data);
      const name = resolveName(data);

      if (!sessionId) {
        throw new Error(
          "Login succeeded but no session id was returned by the server.",
        );
      }

      localStorage.setItem(
        "habithubAuth",
        JSON.stringify({
          isLoggedIn: true,
          userType: authenticatedUserType,
          sessionId,
          userId,
          name,
        }),
      );

      navigate(
        authenticatedUserType === "Creator" ? "/main-creator" : "/main-member",
        { replace: true },
      );
    } catch (error) {
      localStorage.removeItem("habithubAuth");

      setServerError(
        error instanceof Error ? error.message : "Something went wrong.",
      );
    } finally {
      setLoading(false);
    }
  }

  return (
    <main className="page container login-page">
      <div className="background-glow background-glow-left"></div>
      <div className="background-glow background-glow-right"></div>

      <section className="card login-card">
        <div className="content login-content">
          <div className="login-top">
            <Link to="/" className="button button-secondary login-home-link">
              Home
            </Link>
          </div>

          <div className="content-centered login-header">
            <h1 className="title login-title">Log in</h1>

            <p className="text login-text">
              Welcome back. Log in to continue with HabitHub.
            </p>
          </div>

          {serverError && (
            <p className="form-error" role="alert">
              {serverError}
            </p>
          )}

          <form className="login-form" onSubmit={handleSubmit} noValidate>
            <div className="form-field">
              <label className="form-label" htmlFor="email">
                Email
              </label>
              <input
                id="email"
                className="form-input"
                type="email"
                name="email"
                value={form.email}
                onChange={(event) => handleChange("email", event)}
                onBlur={() => handleBlur("email")}
                placeholder="example@gmail.com"
                autoComplete="email"
                aria-invalid={Boolean(touched.email && errors.email)}
                aria-describedby={
                  touched.email && errors.email ? "login-email-error" : undefined
                }
                required
              />
              {touched.email && errors.email && (
                <p id="login-email-error" className="field-error">
                  {errors.email}
                </p>
              )}
            </div>

            <div className="form-field">
              <label className="form-label" htmlFor="password">
                Password
              </label>
              <input
                id="password"
                className="form-input"
                type="password"
                name="password"
                value={form.password}
                onChange={(event) => handleChange("password", event)}
                onBlur={() => handleBlur("password")}
                placeholder="Enter your password"
                autoComplete="current-password"
                aria-invalid={Boolean(touched.password && errors.password)}
                aria-describedby={
                  touched.password && errors.password
                    ? "login-password-error"
                    : undefined
                }
                required
              />
              {touched.password && errors.password && (
                <p id="login-password-error" className="field-error">
                  {errors.password}
                </p>
              )}
            </div>

            <div className="form-field">
              <span className="form-label">Account type</span>

              <div
                className="role-group"
                role="radiogroup"
                aria-label="Account type"
              >
                <button
                  type="button"
                  className={
                    form.userType === "Creator"
                      ? "role-button role-button-active"
                      : "role-button"
                  }
                  onClick={() => handleUserTypeChange("Creator")}
                  aria-pressed={form.userType === "Creator"}
                >
                  Creator
                </button>

                <button
                  type="button"
                  className={
                    form.userType === "Member"
                      ? "role-button role-button-active"
                      : "role-button"
                  }
                  onClick={() => handleUserTypeChange("Member")}
                  aria-pressed={form.userType === "Member"}
                >
                  Member
                </button>
              </div>

              {touched.userType && errors.userType && (
                <p className="field-error">{errors.userType}</p>
              )}
            </div>

            <button
              className="button button-primary form-submit"
              type="submit"
              disabled={loading || !formIsValid}
            >
              {loading ? "Logging in..." : "Log in"}
            </button>

            <p className="form-footer-text">
              Don&apos;t have an account?{" "}
              <Link to="/register" className="form-footer-link">
                Create one
              </Link>
            </p>
          </form>
        </div>
      </section>
    </main>
  );
}