import { useMemo, useState, type ChangeEvent, type SubmitEvent } from "react";
import { Link, useNavigate } from "react-router-dom";
import "./Register.css";
import "../App.css";
import type { AuthResponseDto, RegisterRequestDto } from "../services/dtos";
import {type RegisterForm} from "../services/Register.ts";
import {validateForm, hasValidationErrors} from "../services/Register.ts"
import {
  API_BASE_URL,
  DEFAULT_TIMEZONE,
  TIMEZONE_OPTIONS,
  type AccountType,
  mapUserTypeFromEnum,
  mapUserTypeToEnum,
} from "../services/User.ts"



export default function Register() {
  const navigate = useNavigate();

  const [form, setForm] = useState<RegisterForm>({
    name: "",
    email: "",
    password: "",
    timezone: TIMEZONE_OPTIONS.includes(DEFAULT_TIMEZONE)
      ? DEFAULT_TIMEZONE
      : "Europe/Warsaw",
    userType: "Member",
  });

  const [touched, setTouched] = useState<Record<keyof RegisterForm, boolean>>({
    name: false,
    email: false,
    password: false,
    timezone: false,
    userType: false,
  });

  const [serverError, setServerError] = useState("");
  const [loading, setLoading] = useState(false);

  const errors = useMemo(() => validateForm(form), [form]);
  const formIsValid = !hasValidationErrors(errors);

  function handleChange(
    field: keyof RegisterForm,
    event: ChangeEvent<HTMLInputElement | HTMLSelectElement>,
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

  function handleBlur(field: keyof RegisterForm) {
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
      name: true,
      email: true,
      password: true,
      timezone: true,
      userType: true,
    });

    setServerError("");

    const currentErrors = validateForm(form);

    if (hasValidationErrors(currentErrors)) {
      return;
    }

    setLoading(true);

    try {
      const payload: RegisterRequestDto = {
        name: form.name.trim(),
        email: form.email.trim(),
        password: form.password,
        timezone: form.timezone,
        userType: mapUserTypeToEnum(form.userType),
      };

      const response = await fetch(`${API_BASE_URL}/auth/register`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(payload),
      });

      if (!response.ok) {
        if (response.status === 409) {
          throw new Error("This email is already in use.");
        }

        if (response.status === 400) {
          throw new Error("Please check the form fields and try again.");
        }

        const responseText = await response.text();
        throw new Error(responseText || `Registration failed (${response.status}).`);
      }

      const data = (await response.json()) as AuthResponseDto;
      const sessionId = data.sessionId;

      if (!sessionId) {
        throw new Error(
          "Registration succeeded but no session id was returned by the server.",
        );
      }

      const authenticatedUserType = mapUserTypeFromEnum(data.user.userType);
      const userId = data.user.id;
      const name = data.user.name || form.name.trim();

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
      setServerError(
        error instanceof Error ? error.message : "Something went wrong.",
      );
    } finally {
      setLoading(false);
    }
  }

  return (
  <main className="page container register-page">
    <div className="background-glow background-glow-left" />
    <div className="background-glow background-glow-right" />

    <section className="card register-card">
      <div className="content register-content">
        <div className="register-top">
          <Link to="/" className="button button-secondary register-home-link">
            Home
          </Link>
        </div>

        <div className="content-centered register-header">
          <h1 className="title register-title">Create your account</h1>

          <p className="text register-text">
            Join HabitHub and start building habits with your team.
          </p>
        </div>

        {serverError && (
          <p className="form-error register-form-error" role="alert">
            {serverError}
          </p>
        )}

        <form className="register-form" onSubmit={handleSubmit} noValidate>
          <div className="form-field">
            <label className="form-label" htmlFor="name">
              Name
            </label>
            <input
              id="name"
              className="form-input"
              type="text"
              name="name"
              value={form.name}
              onChange={(event) => handleChange("name", event)}
              onBlur={() => handleBlur("name")}
              placeholder="John"
              autoComplete="name"
              aria-invalid={Boolean(touched.name && errors.name)}
              aria-describedby={touched.name && errors.name ? "name-error" : undefined}
              required
            />
            {touched.name && errors.name && (
              <p id="name-error" className="field-error">
                {errors.name}
              </p>
            )}
          </div>

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
              placeholder="john@gmail.com"
              autoComplete="email"
              aria-invalid={Boolean(touched.email && errors.email)}
              aria-describedby={touched.email && errors.email ? "email-error" : undefined}
              required
            />
            {touched.email && errors.email && (
              <p id="email-error" className="field-error">
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
              autoComplete="new-password"
              aria-invalid={Boolean(touched.password && errors.password)}
              aria-describedby={
                touched.password && errors.password ? "password-error" : undefined
              }
              required
            />
            {touched.password && errors.password && (
              <p id="password-error" className="field-error">
                {errors.password}
              </p>
            )}
          </div>

          <div className="form-field">
            <label className="form-label" htmlFor="timezone">
              Timezone
            </label>
            <select
              id="timezone"
              className="form-input"
              name="timezone"
              value={form.timezone}
              onChange={(event) => handleChange("timezone", event)}
              onBlur={() => handleBlur("timezone")}
              aria-invalid={Boolean(touched.timezone && errors.timezone)}
              aria-describedby={
                touched.timezone && errors.timezone ? "timezone-error" : undefined
              }
              required
            >
              {TIMEZONE_OPTIONS.map((timezone) => (
                <option key={timezone} value={timezone}>
                  {timezone}
                </option>
              ))}
            </select>
            {touched.timezone && errors.timezone && (
              <p id="timezone-error" className="field-error">
                {errors.timezone}
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
            {loading ? "Creating account..." : "Create account"}
          </button>

          <p className="form-footer-text">
            Already have an account?{" "}
            <Link to="/login" className="form-footer-link">
              Log in
            </Link>
          </p>
        </form>
      </div>
    </section>
  </main>
  );
}
