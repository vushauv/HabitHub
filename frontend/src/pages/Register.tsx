import { useMemo, useState, type ChangeEvent, type FormEvent } from "react";
import { Link, useNavigate } from "react-router-dom";
import "./Register.css";

type AccountType = "Creator" | "Member";

type RegisterForm = {
  name: string;
  email: string;
  password: string;
  timezone: string;
  userType: AccountType;
};

type RegisterErrors = {
  name?: string;
  email?: string;
  password?: string;
  timezone?: string;
  userType?: string;
};

const API_BASE_URL = import.meta.env.VITE_API_URL ?? "http://localhost:4999";

const TIMEZONE_OPTIONS = [
  "Europe/Warsaw",
  "Europe/London",
  "Europe/Berlin",
  "Europe/Paris",
  "Europe/Madrid",
  "Europe/Rome",
  "UTC",
  "America/New_York",
  "America/Chicago",
  "America/Denver",
  "America/Los_Angeles",
  "Asia/Tokyo",
  "Asia/Seoul",
  "Asia/Singapore",
  "Australia/Sydney",
];

const DEFAULT_TIMEZONE =
  Intl.DateTimeFormat().resolvedOptions().timeZone || "Europe/Warsaw";

function validateName(name: string): string | undefined {
  if (!name.trim()) {
    return "Name is required.";
  }

  if (name.trim().length < 2) {
    return "Name must be at least 2 characters long.";
  }

  return undefined;
}

function validateEmail(email: string): string | undefined {
  if (!email.trim()) {
    return "Email is required.";
  }

  const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

  if (!emailPattern.test(email)) {
    return "Enter a valid email address.";
  }

  return undefined;
}

function validatePassword(password: string): string | undefined {
  if (!password) {
    return "Password is required.";
  }

  if (password.length < 8) {
    return "Password must be at least 8 characters long.";
  }

  return undefined;
}

function validateTimezone(timezone: string): string | undefined {
  if (!timezone.trim()) {
    return "Timezone is required.";
  }

  if (!TIMEZONE_OPTIONS.includes(timezone)) {
    return "Choose a valid timezone.";
  }

  return undefined;
}

function validateUserType(userType: AccountType): string | undefined {
  if (userType !== "Creator" && userType !== "Member") {
    return "Choose an account type.";
  }

  return undefined;
}

function validateForm(form: RegisterForm): RegisterErrors {
  return {
    name: validateName(form.name),
    email: validateEmail(form.email),
    password: validatePassword(form.password),
    timezone: validateTimezone(form.timezone),
    userType: validateUserType(form.userType),
  };
}

function hasValidationErrors(errors: RegisterErrors): boolean {
  return Boolean(
    errors.name ||
      errors.email ||
      errors.password ||
      errors.timezone ||
      errors.userType,
  );
}

function mapUserTypeToEnum(userType: AccountType): number {
  return userType === "Creator" ? 0 : 1;
}

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

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
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
      const payload = {
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

      navigate("/login");
    } catch (error) {
      setServerError(
        error instanceof Error ? error.message : "Something went wrong.",
      );
    } finally {
      setLoading(false);
    }
  }

  return (
    <main className="page register-container">
      <section className="register-card">
        <div className="register-content">
          <div className="register-top">
            <Link to="/" className="register-home-link">
              Home
            </Link>
          </div>

          <h1 className="register-title">Create your account</h1>

          <p className="register-text">
            Join HabitHub and start building habits with your team.
          </p>

          {serverError && (
            <p className="register-form-error" role="alert">
              {serverError}
            </p>
          )}

          <form className="register-form" onSubmit={handleSubmit} noValidate>
            <div className="register-field">
              <label className="register-label" htmlFor="name">
                Name
              </label>
              <input
                id="name"
                className="register-input"
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
                <p id="name-error" className="register-field-error">
                  {errors.name}
                </p>
              )}
            </div>

            <div className="register-field">
              <label className="register-label" htmlFor="email">
                Email
              </label>
              <input
                id="email"
                className="register-input"
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
                <p id="email-error" className="register-field-error">
                  {errors.email}
                </p>
              )}
            </div>

            <div className="register-field">
              <label className="register-label" htmlFor="password">
                Password
              </label>
              <input
                id="password"
                className="register-input"
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
                <p id="password-error" className="register-field-error">
                  {errors.password}
                </p>
              )}
            </div>

            <div className="register-field">
              <label className="register-label" htmlFor="timezone">
                Timezone
              </label>
              <select
                id="timezone"
                className="register-input"
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
                <p id="timezone-error" className="register-field-error">
                  {errors.timezone}
                </p>
              )}
            </div>

            <div className="register-field">
              <span className="register-label">Account type</span>

              <div
                className="register-role-group"
                role="radiogroup"
                aria-label="Account type"
              >
                <button
                  type="button"
                  className={
                    form.userType === "Creator"
                      ? "register-role-button register-role-button-active"
                      : "register-role-button"
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
                      ? "register-role-button register-role-button-active"
                      : "register-role-button"
                  }
                  onClick={() => handleUserTypeChange("Member")}
                  aria-pressed={form.userType === "Member"}
                >
                  Member
                </button>
              </div>

              {touched.userType && errors.userType && (
                <p className="register-field-error">{errors.userType}</p>
              )}
            </div>

            <button
              className="register-submit-button"
              type="submit"
              disabled={loading || !formIsValid}
            >
              {loading ? "Creating account..." : "Create account"}
            </button>

            <p className="register-footer-text">
              Already have an account?{" "}
              <Link to="/login" className="register-footer-link">
                Log in
              </Link>
            </p>
          </form>
        </div>
      </section>
    </main>
  );
}