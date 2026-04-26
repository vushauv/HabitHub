import { Link, useNavigate } from "react-router-dom";
import { useEffect, useMemo, useState } from "react";
import "./CreateTeam.css";
import "../App.css";
import {
  clearStoredAuth,
  createTeam,
  createTeamFormSchema,
  getStoredAuth,
  getTeamErrorMessage,
  TeamRequestError,
  type CreateTeamForm,
} from "../services/Team";
import { useForm } from "react-hook-form";
import { useLens } from "@hookform/lenses";
import { zodResolver } from "@hookform/resolvers/zod";
import TextInput from "../components/form/TextInput";
import SubmitButton from "../components/form/SubmitButton";

function resolveErrorMessage(error: unknown): string {
  if (error instanceof TeamRequestError) {
    return error.message || getTeamErrorMessage(error.code);
  }

  return "Something went wrong while creating the team. Please try again.";
}

export default function CreateTeam() {
  const navigate = useNavigate();
  const auth = useMemo(() => getStoredAuth(), []);
  const [serverError, setServerError] = useState("");
  const [loading, setLoading] = useState(false);

  const { handleSubmit, control, formState, subscribe } = useForm<CreateTeamForm>({
    defaultValues: {
      name: ""
    },
    disabled: loading,
    resolver: zodResolver(createTeamFormSchema),
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

  async function onSubmit(form: CreateTeamForm) {
    setServerError("");

    if (!auth?.isLoggedIn || !auth.sessionId) {
      clearStoredAuth();
      setServerError("Your session is no longer valid. Please log in again.");

      setTimeout(() => {
        navigate("/login", { replace: true });
      }, 1200);

      return;
    }

    setLoading(true);

    try {
      await createTeam(auth, form);
      navigate("/teams-creator", { replace: true });
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

        <div className="card create-team-card-shell">
          <div className="content create-team-content">
            <div className="create-team-topbar">
              <Link
                to="/"
                className="button button-secondary create-team-nav-button"
              >
                Home
              </Link>

              <Link
                to="/teams-creator"
                className="button button-secondary create-team-nav-button"
              >
                Teams
              </Link>
            </div>

            <div className="content-centered create-team-header">
              <h1 className="title create-team-title">Create a Team</h1>
            </div>

            {serverError ? (
              <p className="form-error create-team-message" role="alert">
                {serverError}
              </p>
            ) : null}

            <form className="create-team-form" onSubmit={handleSubmit(onSubmit)} noValidate>
              <TextInput
                label="Name"
                lens={lens.focus("name")}
                type="text"
                placeholder="Book Enjoyers"
                autoComplete="off"
                required
                className="create-team-name-row"
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
