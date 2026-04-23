import { Link, useNavigate } from "react-router-dom";
import { useMemo, useState, type ChangeEvent, type SubmitEvent } from "react";
import "./CreateTeam.css";
import "../App.css";
import {
  clearStoredAuth,
  createTeam,
  getStoredAuth,
  getTeamErrorMessage,
  hasCreateTeamErrors,
  TeamRequestError,
  validateCreateTeamForm,
  type CreateTeamForm,
} from "../services/Team";

function resolveErrorMessage(error: unknown): string {
  if (error instanceof TeamRequestError) {
    return error.message || getTeamErrorMessage(error.code);
  }

  return "Something went wrong while creating the team. Please try again.";
}

export default function CreateTeam() {
  const navigate = useNavigate();
  const auth = useMemo(() => getStoredAuth(), []);
  const [form, setForm] = useState<CreateTeamForm>({
    name: "",
  });
  const [touched, setTouched] = useState<Record<keyof CreateTeamForm, boolean>>({
    name: false,
  });
  const [serverError, setServerError] = useState("");
  const [loading, setLoading] = useState(false);

  const errors = useMemo(() => validateCreateTeamForm(form), [form]);
  const formIsValid = !hasCreateTeamErrors(errors);

  function handleChange(event: ChangeEvent<HTMLInputElement>) {
    setForm({
      name: event.target.value,
    });

    setTouched({
      name: true,
    });

    setServerError("");
  }

  function handleBlur() {
    setTouched({
      name: true,
    });
  }

  async function handleSubmit(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault();

    setTouched({
      name: true,
    });

    setServerError("");

    const currentErrors = validateCreateTeamForm(form);

    if (hasCreateTeamErrors(currentErrors)) {
      return;
    }

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
                home
              </Link>

              <Link
                to="/teams-creator"
                className="button button-secondary create-team-nav-button"
              >
                teams
              </Link>
            </div>

            <div className="content-centered create-team-header">
              <h1 className="title create-team-title">create a team</h1>
            </div>

            {serverError ? (
              <p className="form-error create-team-message" role="alert">
                {serverError}
              </p>
            ) : null}

            <form className="create-team-form" onSubmit={handleSubmit} noValidate>
              <div className="form-field create-team-name-row">
                <label className="form-label create-team-label" htmlFor="name">
                  name
                </label>

                <input
                  id="name"
                  className="form-input create-team-input"
                  type="text"
                  name="name"
                  value={form.name}
                  onChange={handleChange}
                  onBlur={handleBlur}
                  placeholder="book enjoyers"
                  autoComplete="off"
                  minLength={3}
                  maxLength={100}
                  aria-invalid={Boolean(touched.name && errors.name)}
                  aria-describedby={
                    touched.name && errors.name
                      ? "create-team-name-error"
                      : undefined
                  }
                  required
                />

                {touched.name && errors.name ? (
                  <p id="create-team-name-error" className="field-error">
                    {errors.name}
                  </p>
                ) : null}
              </div>

              <button
                className="button button-primary create-team-submit"
                type="submit"
                disabled={loading || !formIsValid}
              >
                {loading ? "submitting..." : "submit"}
              </button>
            </form>
          </div>
        </div>
      </section>
    </main>
  );
}
