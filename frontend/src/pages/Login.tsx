import { useMemo, useState, type ChangeEvent, type FormEvent } from "react";
import { Link, useNavigate } from "react-router-dom";

type AccountType = "Creator" | "Member";

type LoginForm = {
  email: string;
  password: string;
  userType: AccountType;
};

type LoginErrors = {
  email?: string;
  password?: string;
  userType?: string;
};

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

  return undefined;
}

function validateUserType(userType: AccountType): string | undefined {
  if (userType !== "Creator" && userType !== "Member") {
    return "Choose an account type.";
  }

  return undefined;
}

function validateForm(form: LoginForm): LoginErrors {
  return {
    email: validateEmail(form.email),
    password: validatePassword(form.password),
    userType: validateUserType(form.userType),
  };
}

function hasValidationErrors(errors: LoginErrors): boolean {
  return Boolean(errors.email || errors.password || errors.userType);
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

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
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
        userType: form.userType,
      };

      const response = await fetch("/auth/login", {
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

      navigate("/");
    } catch (error) {
      setServerError(
        error instanceof Error ? error.message : "Something went wrong.",
      );
    } finally {
      setLoading(false);
    }
  }

  return (
    <main className="page login-container">
      <section className="login-card">
        <div className="login-content">
          <div className="login-top">
            <Link to="/" className="login-home-link">
              Home
            </Link>
          </div>

          <h1 className="login-title">Log in</h1>

          <p className="login-text">
            Welcome back. Log in to continue with HabitHub.
          </p>

          {serverError && (
            <p className="login-form-error" role="alert">
              {serverError}
            </p>
          )}

          <form className="login-form" onSubmit={handleSubmit} noValidate>
            <div className="login-field">
              <label className="login-label" htmlFor="email">
                Email
              </label>
              <input
                id="email"
                className="login-input"
                type="email"
                name="email"
                value={form.email}
                onChange={(event) => handleChange("email", event)}
                onBlur={() => handleBlur("email")}
                placeholder="example@gmail.com"
                autoComplete="email"
                aria-invalid={Boolean(touched.email && errors.email)}
                aria-describedby={touched.email && errors.email ? "login-email-error" : undefined}
                required
              />
              {touched.email && errors.email && (
                <p id="login-email-error" className="login-field-error">
                  {errors.email}
                </p>
              )}
            </div>

            <div className="login-field">
              <label className="login-label" htmlFor="password">
                Password
              </label>
              <input
                id="password"
                className="login-input"
                type="password"
                name="password"
                value={form.password}
                onChange={(event) => handleChange("password", event)}
                onBlur={() => handleBlur("password")}
                placeholder="Enter your password"
                autoComplete="current-password"
                aria-invalid={Boolean(touched.password && errors.password)}
                aria-describedby={
                  touched.password && errors.password ? "login-password-error" : undefined
                }
                required
              />
              {touched.password && errors.password && (
                <p id="login-password-error" className="login-field-error">
                  {errors.password}
                </p>
              )}
            </div>

            <div className="login-field">
              <span className="login-label">Account type</span>

              <div
                className="login-role-group"
                role="radiogroup"
                aria-label="Account type"
              >
                <button
                  type="button"
                  className={
                    form.userType === "Creator"
                      ? "login-role-button login-role-button-active"
                      : "login-role-button"
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
                      ? "login-role-button login-role-button-active"
                      : "login-role-button"
                  }
                  onClick={() => handleUserTypeChange("Member")}
                  aria-pressed={form.userType === "Member"}
                >
                  Member
                </button>
              </div>

              {touched.userType && errors.userType && (
                <p className="login-field-error">{errors.userType}</p>
              )}
            </div>

            <button
              className="login-submit-button"
              type="submit"
              disabled={loading || !formIsValid}
            >
              {loading ? "Logging in..." : "Log in"}
            </button>

            <p className="login-footer-text">
              Don&apos;t have an account?{" "}
              <Link to="/register" className="login-footer-link">
                Create one
              </Link>
            </p>
          </form>
        </div>
      </section>
    </main>
  );
}