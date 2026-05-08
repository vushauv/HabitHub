import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import "./Login.css";
import "../App.css";
import type { LoginRequestDto } from "../services/dtos";
import { loginFormSchema, type LoginForm } from "../services/Login";
import {
  API_BASE_URL,
  mapUserTypeToEnum,
} from "../services/User";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import TextInput from "../components/form/TextInput";
import { useLens } from "@hookform/lenses";
import AccountTypeInput from "../components/form/AccountTypeInput";
import SubmitButton from "../components/form/SubmitButton";
import {
  getDashboardPathForUser,
  storeSessionAndLoadCurrentUser,
} from "../services/Auth";

export default function Login() {
  const navigate = useNavigate();
  
  const [serverError, setServerError] = useState("");
  const [loading, setLoading] = useState(false);

  const { handleSubmit, control, formState, subscribe } = useForm<LoginForm>({
    defaultValues: {
      email: "",
      password: "",
      userType: "Member",
    },
    disabled: loading,
    resolver: zodResolver(loginFormSchema),
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
  }, [subscribe])

  async function onSubmit(form: LoginForm) {
    setServerError("");

    setLoading(true);

    try {
      const payload: LoginRequestDto = {
        email: form.email,
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

      const data = (await response.json()) as { sessionId?: string | null };
      const sessionId = data.sessionId;

      if (!sessionId) {
        throw new Error(
          "Login succeeded but no session id was returned by the server.",
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

          <form className="login-form" onSubmit={handleSubmit(onSubmit)} noValidate>
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
              autoComplete="current-password"
            />

            <AccountTypeInput
              label="Account type"
              lens={lens.focus("userType")}
            />

            <SubmitButton
              formState={formState}
              disabled={loading}
            >
              {loading ? "Logging in..." : "Log in"}
            </SubmitButton>

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
