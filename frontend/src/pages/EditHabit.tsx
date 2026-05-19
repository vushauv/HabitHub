import { Link, useNavigate, useParams } from "react-router-dom";
import { useEffect, useMemo, useState } from "react";
import { useForm } from "react-hook-form";
import { useLens } from "@hookform/lenses";
import { zodResolver } from "@hookform/resolvers/zod";
import "./CreateHabit.css";
import "../App.css";
import TextInput from "../components/form/TextInput";
import SubmitButton from "../components/form/SubmitButton";
import {
  clearStoredAuth,
  createEditHabitDefaultValues,
  editHabit,
  editHabitFormSchema,
  formatHabitType,
  formatHabitUnit,
  getHabit,
  getHabitErrorMessage,
  getStoredAuth,
  HabitRequestError,
  type EditHabitForm,
  type HabitSummaryDto,
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

  return "Something went wrong while editing the habit. Please try again.";
}

export default function EditHabit() {
  const navigate = useNavigate();
  const { teamId = "", habitId = "" } = useParams();
  const auth = useMemo(() => getStoredAuth(), []);
  const [team, setTeam] = useState<TeamDetailsDto | null>(null);
  const [habit, setHabit] = useState<HabitSummaryDto | null>(null);
  const [serverError, setServerError] = useState("");
  const [loading, setLoading] = useState(false);
  const [initialLoading, setInitialLoading] = useState(true);

  const {
    handleSubmit,
    control,
    formState,
    register,
    subscribe,
    reset,
    watch,
    setValue,
  } = useForm<EditHabitForm>({
    defaultValues: {
      name: "",
      goal: "",
      expiryDate: "",
      clearGoal: false,
      clearExpiryDate: false,
    },
    disabled: loading || initialLoading,
    resolver: zodResolver(editHabitFormSchema),
    mode: "all",
  });

  const lens = useLens({ control });
  const clearGoal = watch("clearGoal");
  const clearExpiryDate = watch("clearExpiryDate");

  useEffect(() => {
    if (clearGoal) {
      setValue("goal", "", {
        shouldDirty: true,
        shouldValidate: true,
      });
    }
  }, [clearGoal, setValue]);

  useEffect(() => {
    if (clearExpiryDate) {
      setValue("expiryDate", "", {
        shouldDirty: true,
        shouldValidate: true,
      });
    }
  }, [clearExpiryDate, setValue]);

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

    const loadHabit = async () => {
      setInitialLoading(true);
      setServerError("");

      if (!teamId || !habitId) {
        if (isMounted) {
          setServerError("Choose a habit first.");
          setInitialLoading(false);
        }

        return;
      }

      if (!auth) {
        clearStoredAuth();

        if (isMounted) {
          setServerError("Your session is no longer valid. Please log in again.");
          setInitialLoading(false);
        }

        setTimeout(() => {
          navigate("/login", { replace: true });
        }, 1200);

        return;
      }

      try {
        const [loadedTeam, loadedHabit] = await Promise.all([
          getTeam(auth, teamId),
          getHabit(auth, habitId),
        ]);

        if (!isMounted) {
          return;
        }

        setTeam(loadedTeam);
        setHabit(loadedHabit);
        reset(createEditHabitDefaultValues(loadedHabit));
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
      } finally {
        if (isMounted) {
          setInitialLoading(false);
        }
      }
    };

    void loadHabit();

    return () => {
      isMounted = false;
    };
  }, [auth, habitId, navigate, reset, teamId]);

  async function onSubmit(form: EditHabitForm) {
    setServerError("");

    if (!habit || !habitId || !teamId) {
      setServerError("Choose a habit first.");
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
      await editHabit(auth, habitId, form, habit);
      navigate(
        `/creator/teams/${encodeURIComponent(teamId)}/habits/${encodeURIComponent(
          habitId,
        )}/details`,
        { replace: true },
      );
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
                to={`/creator/teams/${encodeURIComponent(teamId)}/habits`}
                className="button button-secondary page-nav-button"
              >
                Habits
              </Link>
            </div>

            <div className="content-centered">
              <h1 className="title page-title-md habit-form-title pill-title">
                Edit Habit
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

            {initialLoading ? (
              <div className="state-card">
                <p className="state-title">Loading habit...</p>
                <p className="state-text">
                  We are retrieving the habit details.
                </p>
              </div>
            ) : habit ? (
              <form className="habit-form" onSubmit={handleSubmit(onSubmit)} noValidate>
                <TextInput
                  label="Name"
                  lens={lens.focus("name")}
                  type="text"
                  autoComplete="off"
                  required
                  className="habit-form-row"
                />

                <div className="habit-form-row">
                  <span className="form-label">Type</span>
                  <p className="habit-form-readonly">
                    {formatHabitType(habit.habitType)}
                  </p>
                </div>

                <div className="habit-form-row">
                  <span className="form-label">Unit</span>
                  <p className="habit-form-readonly">
                    {formatHabitUnit(habit.unit)}
                  </p>
                </div>

                <TextInput
                  label="Goal"
                  lens={lens.focus("goal")}
                  type="text"
                  autoComplete="off"
                  className="habit-form-row"
                />

                <label className="habit-form-checkbox-row">
                  <input
                    className="habit-form-checkbox"
                    type="checkbox"
                    {...register("clearGoal")}
                  />
                  Clear goal
                </label>

                <TextInput
                  label="Expires"
                  lens={lens.focus("expiryDate")}
                  type="datetime-local"
                  autoComplete="off"
                  className="habit-form-row"
                />

                <label className="habit-form-checkbox-row">
                  <input
                    className="habit-form-checkbox"
                    type="checkbox"
                    {...register("clearExpiryDate")}
                  />
                  Clear expiry date
                </label>

                <SubmitButton
                  formState={formState}
                  disabled={loading}
                >
                  {loading ? "Saving..." : "Save changes"}
                </SubmitButton>
              </form>
            ) : null}
          </div>
        </div>
      </section>
    </main>
  );
}
