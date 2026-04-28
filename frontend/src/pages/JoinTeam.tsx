import { Link, useNavigate } from "react-router-dom";
import { useMemo, useState, type ChangeEvent, type SubmitEvent } from "react";
import "./JoinTeam.css";
import "../App.css";
import {
  clearStoredAuth,
  getStoredAuth,
  getTeamErrorMessage,
  hasJoinTeamErrors,
  joinTeam,
  TeamRequestError,
  validateJoinTeamForm,
  type JoinTeamForm,
} from "../services/Team";

function resolveErrorMessage(error: unknown): string {
  if (error instanceof TeamRequestError) {
    return error.message || getTeamErrorMessage(error.code);
  }

  return "Something went wrong while joining the team. Please try again.";
}

export default function JoinTeam() {
  const navigate = useNavigate();
  const auth = useMemo(() => getStoredAuth(), []);
  const [form, setForm] = useState<JoinTeamForm>({
    code: "",
  });
  const [touched, setTouched] = useState<Record<keyof JoinTeamForm, boolean>>({
    code: false,
  });
  const [serverError, setServerError] = useState("");
  const [successMessage, setSuccessMessage] = useState("");
  const [loading, setLoading] = useState(false);

  const errors = useMemo(() => validateJoinTeamForm(form), [form]);
  const formIsValid = !hasJoinTeamErrors(errors);

  function handleChange(event: ChangeEvent<HTMLInputElement>) {
    setForm({
      code: event.target.value,
    });

    setTouched({
      code: true,
    });

    setServerError("");
    setSuccessMessage("");
  }

  function handleBlur() {
    setTouched({
      code: true,
    });
  }

  async function handleSubmit(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault();

    setTouched({
      code: true,
    });

    setServerError("");
    setSuccessMessage("");

    const currentErrors = validateJoinTeamForm(form);

    if (hasJoinTeamErrors(currentErrors)) {
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
      await joinTeam(auth, form);
      setSuccessMessage("You joined the team.");

      setTimeout(() => {
        navigate("/teams-member", { replace: true });
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
                to="/teams-member"
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

            <form className="join-team-form" onSubmit={handleSubmit} noValidate>
              <div className="form-field join-team-code-row">
                <label className="form-label join-team-label" htmlFor="code">
                  Invite Code
                </label>

                <input
                  id="code"
                  className="form-input join-team-input"
                  type="text"
                  name="code"
                  value={form.code}
                  onChange={handleChange}
                  onBlur={handleBlur}
                  placeholder="A1B2C3D4"
                  autoComplete="off"
                  minLength={8}
                  maxLength={8}
                  aria-invalid={Boolean(touched.code && errors.code)}
                  aria-describedby={
                    touched.code && errors.code
                      ? "join-team-code-error"
                      : undefined
                  }
                  required
                />

                {touched.code && errors.code ? (
                  <p id="join-team-code-error" className="field-error">
                    {errors.code}
                  </p>
                ) : null}
              </div>

              <button
                className="button button-primary join-team-submit"
                type="submit"
                disabled={loading || !formIsValid}
              >
                {loading ? "Submitting..." : "Submit"}
              </button>
            </form>
          </div>
        </div>
      </section>
    </main>
  );
}
