import { Link, useNavigate, useSearchParams } from "react-router-dom";
import { useEffect, useMemo, useState } from "react";
import { useForm } from "react-hook-form";
import { useLens } from "@hookform/lenses";
import { zodResolver } from "@hookform/resolvers/zod";
import "./CreateHabit.css";
import "../App.css";
import TextInput from "../components/form/TextInput";
import SelectInput from "../components/form/SelectInput";
import SubmitButton from "../components/form/SubmitButton";
import {
  clearStoredAuth,
  createHabit,
  createHabitFormSchema,
  getHabitErrorMessage,
  getStoredAuth,
  habitTypeOptions,
  HabitRequestError,
  unitOptions,
  type CreateHabitForm,
} from "../services/Habit";
import {
  getTeam,
  getTeamErrorMessage,
  TeamRequestError,
  type TeamDetailsDto,
} from "../services/Team";

function resolveErrorMessage(error: unknown): string {
  if (error instanceof HabitRequestError) {
    return error.message || getHabitErrorMessage(error.code);
  }

  if (error instanceof TeamRequestError) {
    return error.message || getTeamErrorMessage(error.code);
  }

  return "Something went wrong while creating the habit. Please try again.";
}

export default function CreateHabit() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const teamId = searchParams.get("teamId") ?? "";
  const auth = useMemo(() => getStoredAuth(), []);
  const [team, setTeam] = useState<TeamDetailsDto | null>(null);
  const [serverError, setServerError] = useState("");
  const [loading, setLoading] = useState(false);

  const { handleSubmit, control, formState, subscribe, watch, setValue } =
    useForm<CreateHabitForm>({
      defaultValues: {
        name: "",
        goal: "",
        habitType: "Binary",
        unit: "",
        expiryDate: "",
      },
      disabled: loading,
      resolver: zodResolver(createHabitFormSchema),
      mode: "all",
    });

  const lens = useLens({ control });
  const habitType = watch("habitType");

  useEffect(() => {
    if (habitType === "Binary") {
      setValue("unit", "", {
        shouldDirty: true,
        shouldValidate: true,
      });
    }
  }, [habitType, setValue]);

  useEffect(() => {
    const callback = subscribe({
      formState: {
        values: true,
      },
      callback: () => {
        setServerError("");
      },
    });

    return () => callback();
  }, [subscribe]);

  useEffect(() => {
    let isMounted = true;

    const loadTeam = async () => {
      if (!teamId) {
        setServerError("Choose a team first.");
        return;
      }

      if (!auth) {
        clearStoredAuth();
        setServerError("Your session is no longer valid. Please log in again.");

        setTimeout(() => {
          navigate("/login", { replace: true });
        }, 1200);

        return;
      }

      try {
        const loadedTeam = await getTeam(auth, teamId);

        if (isMounted) {
          setTeam(loadedTeam);
        }
      } catch (error) {
        if (!isMounted) {
          return;
        }

        if (
          (error instanceof HabitRequestError ||
            error instanceof TeamRequestError) &&
          error.code === "auth-required"
        ) {
          clearStoredAuth();
          setServerError(getHabitErrorMessage(error.code));

          setTimeout(() => {
            navigate("/login", { replace: true });
          }, 1200);
        } else {
          setServerError(resolveErrorMessage(error));
        }
      }
    };

    void loadTeam();

    return () => {
      isMounted = false;
    };
  }, [auth, navigate, teamId]);

  async function onSubmit(form: CreateHabitForm) {
    setServerError("");

    if (!teamId) {
      setServerError("Choose a team first.");
      return;
    }

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
      await createHabit(auth, teamId, form);
      navigate(`/habits-creator?teamId=${encodeURIComponent(teamId)}`, {
        replace: true,
      });
    } catch (error) {
      if (error instanceof HabitRequestError && error.code === "auth-required") {
        clearStoredAuth();
        setServerError(getHabitErrorMessage(error.code));

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
          <div className="content habit-form-content">
            <div className="page-topbar">
              <Link
                to="/"
                className="button button-secondary page-nav-button"
              >
                Home
              </Link>

              <Link
                to={`/habits-creator?teamId=${encodeURIComponent(teamId)}`}
                className="button button-secondary page-nav-button"
              >
                Habits
              </Link>
            </div>

            <div className="content-centered">
              <h1 className="title page-title-md habit-form-title pill-title">
                Create Habit
              </h1>

              {team ? (
                <p className="text habit-form-team-name">{team.name}</p>
              ) : null}
            </div>

            {serverError ? (
              <p className="form-error page-message" role="alert">
                {serverError}
              </p>
            ) : null}

            <form className="habit-form" onSubmit={handleSubmit(onSubmit)} noValidate>
              <TextInput
                label="Name"
                lens={lens.focus("name")}
                type="text"
                placeholder="Morning walk"
                autoComplete="off"
                required
                className="habit-form-row"
              />

              <TextInput
                label="Goal"
                lens={lens.focus("goal")}
                type="text"
                placeholder="Walk before work"
                autoComplete="off"
                className="habit-form-row"
              />

              <SelectInput
                label="Type"
                lens={lens.focus("habitType")}
                required
                className="habit-form-row"
              >
                {habitTypeOptions.map((option) => (
                  <option key={option} value={option}>
                    {option}
                  </option>
                ))}
              </SelectInput>

              <SelectInput
                label="Unit"
                lens={lens.focus("unit")}
                required={habitType === "Quantitative"}
                className="habit-form-row"
              >
                <option value="">No unit</option>
                {unitOptions.map((option) => (
                  <option
                    key={option}
                    value={option}
                    disabled={habitType === "Binary"}
                  >
                    {option}
                  </option>
                ))}
              </SelectInput>

              <TextInput
                label="Expires"
                lens={lens.focus("expiryDate")}
                type="datetime-local"
                autoComplete="off"
                className="habit-form-row"
              />

              <SubmitButton
                formState={formState}
                disabled={loading}
              >
                {loading ? "Creating..." : "Create habit"}
              </SubmitButton>
            </form>
          </div>
        </div>
      </section>
    </main>
  );
}
