import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import "./Register.css";
import "../App.css";
import type { RegisterRequestDto } from "../services/dtos";
import {registerFormSchema, type RegisterForm} from "../services/Register.ts";
import {
  API_BASE_URL,
  DEFAULT_TIMEZONE,
  TIMEZONE_OPTIONS,
  mapUserTypeToEnum,
} from "../services/User.ts"
import { useForm } from "react-hook-form"
import TextInput from "../components/form/TextInput.tsx";
import { useLens } from "@hookform/lenses";
import SelectInput from "../components/form/SelectInput.tsx";
import AccountTypeInput from "../components/form/AccountTypeInput.tsx";
import SubmitButton from "../components/form/SubmitButton.tsx";
import { zodResolver } from "@hookform/resolvers/zod";
import {
  getDashboardPathForUser,
  storeSessionAndLoadCurrentUser,
} from "../services/Auth";


export default function Register() {
  const navigate = useNavigate();

  const [serverError, setServerError] = useState("");
  const [loading, setLoading] = useState(false);

  const { handleSubmit, control, formState, subscribe } = useForm<RegisterForm>({
    defaultValues: {
      name: "",
      email: "",
      password: "",
      timezone: TIMEZONE_OPTIONS.includes(DEFAULT_TIMEZONE)
        ? DEFAULT_TIMEZONE
        : "Europe/Warsaw",
      userType: "Member",
    },
    disabled: loading,
    resolver: zodResolver(registerFormSchema),
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
        setServerError("");
      },
    })

    return () => callback()
  }, [subscribe]);

  async function onSubmit(form: RegisterForm) {
    setServerError("");

    setLoading(true);

    try {
      const payload: RegisterRequestDto = {
        name: form.name,
        email: form.email,
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

      const data = (await response.json()) as { sessionId?: string | null };
      const sessionId = data.sessionId;

      if (!sessionId) {
        throw new Error(
          "Registration succeeded but no session id was returned by the server.",
        );
      }

      const currentUser = await storeSessionAndLoadCurrentUser(sessionId);

      navigate(getDashboardPathForUser(currentUser), { replace: true });
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

        <form className="register-form" onSubmit={handleSubmit(onSubmit)} noValidate>
          <TextInput
            label="Name"
            lens={lens.focus("name")}
            required
            type="text"
            placeholder="John"
            autoComplete="name"
          />

          <TextInput
            label="Email"
            lens={lens.focus("email")}
            required
            type="email"
            placeholder="john@example.com"
            autoComplete="email"
          />

          <TextInput
            label="Password"
            lens={lens.focus("password")}
            required
            type="password"
            placeholder="Enter your password"
            autoComplete="new-password"
          />

          <SelectInput
            label="Timezone"
            lens={lens.focus("timezone")}
            required
          >
            {TIMEZONE_OPTIONS.map((timezone) => (
              <option key={timezone} value={timezone}>
                {timezone}
              </option>
            ))}
          </SelectInput>

          <AccountTypeInput
            label="Account type"
            lens={lens.focus("userType")}
          />

          <SubmitButton
            formState={formState}
            disabled={loading}
          >
            {loading ? "Creating account..." : "Create account"}
          </SubmitButton>

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
