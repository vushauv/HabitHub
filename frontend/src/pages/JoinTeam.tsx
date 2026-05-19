import { Link, useNavigate } from "react-router-dom";
import { useEffect, useMemo, useState } from "react";
import "./JoinTeam.css";
import "../App.css";
import {
  clearStoredAuth,
  getStoredAuth,
  getTeamErrorMessage,
  joinTeam,
  joinTeamFormSchema,
  TeamRequestError,
  type JoinTeamForm,
} from "../services/Team";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { useLens } from "@hookform/lenses";
import TextInput from "../components/form/TextInput";
import SubmitButton from "../components/form/SubmitButton";

function resolveErrorMessage(error: unknown): string {
  if (error instanceof TeamRequestError) {
    return error.message || getTeamErrorMessage(error.code);
  }

  return "Something went wrong while joining the team. Please try again.";
}

export default function JoinTeam() {
  const navigate = useNavigate();
  const auth = useMemo(() => getStoredAuth(), []);
  const [serverError, setServerError] = useState("");
  const [successMessage, setSuccessMessage] = useState("");
  const [loading, setLoading] = useState(false);

  const { handleSubmit, control, formState, subscribe } = useForm<JoinTeamForm>({
    defaultValues: {
      code: ""
    },
    disabled: loading,
    resolver: zodResolver(joinTeamFormSchema),
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
        setSuccessMessage("");
      },
    })

    return () => callback()
  }, [subscribe]);

  async function onSubmit(form: JoinTeamForm) {
    setServerError("");
    setSuccessMessage("");

    if (!auth) {
      clearStoredAuth();
      setServerError("Your session is no longer valid. Please log in again.");

      setTimeout(() => {
        navigate("/login", { replace: true });
      }, 1200);

      return;
    }

    setLoading(true);

    try {
      await joinTeam(auth, form);
      setSuccessMessage("You joined the team.");

      setTimeout(() => {
        navigate("/member/teams", { replace: true });
      }, 800);
    } catch (error) {
      if (error instanceof TeamRequestError && error.code === "auth-required") {
        clearStoredAuth();
        setServerError(getTeamErrorMessage(error.code));

        setTimeout(() => {
          navigate("/login", { replace: true });
        }, 1200);
      } else {
        setServerError(resolveErrorMessage(error));
      }
    } finally {
      setLoading(false);
    }
  }

  return (
    <main className="page">
      <section className="container">
        <div className="background-glow background-glow-left" />
        <div className="background-glow background-glow-right" />

        <div className="card page-card-shell">
          <div className="content join-team-content">
            <div className="page-topbar">
              <Link
                to="/"
                className="button button-secondary page-nav-button"
              >
                Home
              </Link>

              <Link
                to="/member/teams"
                className="button button-secondary page-nav-button"
              >
                Teams
              </Link>
            </div>

            <div className="content-centered">
              <h1 className="title page-title-md join-team-title pill-title">Join a New Team</h1>
            </div>

            {serverError ? (
              <p className="form-error page-message" role="alert">
                {serverError}
              </p>
            ) : null}

            {successMessage ? (
              <p className="alert-success">{successMessage}</p>
            ) : null}

            <form className="join-team-form" onSubmit={handleSubmit(onSubmit)} noValidate>
              <TextInput
                label="Invite Code"
                lens={lens.focus("code")}
                type="text"
                placeholder="A1B2C3D4"
                autoComplete="off"
                required
                className="join-team-code-row"
              />

              <SubmitButton
                formState={formState}
                disabled={loading}
              >
                {loading ? "Submitting..." : "Submit"}
              </SubmitButton>
            </form>
          </div>
        </div>
      </section>
    </main>
  );
}
