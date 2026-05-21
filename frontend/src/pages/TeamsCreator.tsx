import { Link, useNavigate } from "react-router-dom";
import { useEffect, useMemo, useState } from "react";
import "./TeamsCreator.css";
import "../App.css";
import {
  clearStoredAuth,
  deleteTeam,
  formatInviteExpiryDate,
  generateInviteCode,
  getInviteCodes,
  getStoredAuth,
  getTeamErrorMessage,
  getTeams,
  invalidateInviteCode,
  TeamRequestError,
  type InviteCodeDto,
  type TeamSummaryDto,
} from "../services/Team";

type PendingAction = {
  teamId: string;
  action: "generate" | "invalidate" | "delete";
} | null;

function resolveErrorMessage(error: unknown): string {
  if (error instanceof TeamRequestError) {
    return error.message || getTeamErrorMessage(error.code);
  }

  return "Something went wrong while loading your teams. Please try again.";
}

export default function TeamsCreator() {
  const navigate = useNavigate();
  const auth = useMemo(() => getStoredAuth(), []);
  const [teams, setTeams] = useState<TeamSummaryDto[]>([]);
  const [inviteCodes, setInviteCodes] = useState<Record<string, InviteCodeDto[]>>({});
  const [loading, setLoading] = useState(true);
  const [pageError, setPageError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [pendingAction, setPendingAction] = useState<PendingAction>(null);

  useEffect(() => {
    let isMounted = true;

    const loadTeams = async () => {
      setLoading(true);
      setPageError(null);

      if (!auth) {
        clearStoredAuth();

        if (isMounted) {
          setPageError("Your session is no longer valid. Please log in again.");
          setLoading(false);
        }

        setTimeout(() => {
          navigate("/login", { replace: true });
        }, 1200);

        return;
      }

      try {
        const loadedTeams = await getTeams(auth);

        if (!isMounted) {
          return;
        }

        setTeams(loadedTeams);

        const inviteEntries = await Promise.all(
          loadedTeams.map(async (team) => {
            const codes = await getInviteCodes(auth, team.teamId);
            return [team.teamId, codes] as const;
          }),
        );

        if (!isMounted) {
          return;
        }

        setInviteCodes(Object.fromEntries(inviteEntries));
      } catch (error) {
        if (!isMounted) {
          return;
        }

        if (error instanceof TeamRequestError && error.code === "auth-required") {
          clearStoredAuth();
          setPageError(getTeamErrorMessage(error.code));

          setTimeout(() => {
            navigate("/login", { replace: true });
          }, 1200);
        } else {
          setPageError(resolveErrorMessage(error));
        }
      } finally {
        if (isMounted) {
          setLoading(false);
        }
      }
    };

    void loadTeams();

    return () => {
      isMounted = false;
    };
  }, [auth, navigate]);

  const handleGenerateInviteCode = async (teamId: string) => {
    setPendingAction({ teamId, action: "generate" });
    setPageError(null);
    setSuccessMessage(null);

    try {
      const inviteCode = await generateInviteCode(auth, teamId);

      setInviteCodes((currentInviteCodes) => ({
        ...currentInviteCodes,
        [teamId]: [inviteCode],
      }));

      setSuccessMessage("New invite code created.");
    } catch (error) {
      handleActionError(error);
    } finally {
      setPendingAction(null);
    }
  };

  const handleInvalidateInviteCode = async (teamId: string, codeId: string) => {
    setPendingAction({ teamId, action: "invalidate" });
    setPageError(null);
    setSuccessMessage(null);

    try {
      await invalidateInviteCode(auth, teamId, codeId);

      setInviteCodes((currentInviteCodes) => ({
        ...currentInviteCodes,
        [teamId]:
          currentInviteCodes[teamId]?.filter(
            (inviteCode) => inviteCode.codeId !== codeId,
          ) ?? [],
      }));

      setSuccessMessage("Invite code invalidated.");
    } catch (error) {
      handleActionError(error);
    } finally {
      setPendingAction(null);
    }
  };

  const handleDeleteTeam = async (team: TeamSummaryDto) => {
    const confirmed = window.confirm(`Delete team "${team.name}"?`);

    if (!confirmed) {
      return;
    }

    setPendingAction({ teamId: team.teamId, action: "delete" });
    setPageError(null);
    setSuccessMessage(null);

    try {
      await deleteTeam(auth, team.teamId);

      setTeams((currentTeams) =>
        currentTeams.filter((currentTeam) => currentTeam.teamId !== team.teamId),
      );

      setInviteCodes((currentInviteCodes) => {
        const nextInviteCodes = { ...currentInviteCodes };
        delete nextInviteCodes[team.teamId];
        return nextInviteCodes;
      });

      setSuccessMessage("Team deleted.");
    } catch (error) {
      handleActionError(error);
    } finally {
      setPendingAction(null);
    }
  };

  const handleActionError = (error: unknown) => {
    if (error instanceof TeamRequestError && error.code === "auth-required") {
      clearStoredAuth();
      setPageError(getTeamErrorMessage(error.code));

      setTimeout(() => {
        navigate("/login", { replace: true });
      }, 1200);

      return;
    }

    setPageError(resolveErrorMessage(error));
  };

  return (
    <main className="page">
      <section className="container">
        <div className="background-glow background-glow-left" />
        <div className="background-glow background-glow-right" />

        <div className="card page-card-shell">
          <div className="content teams-creator-content">
            <div className="page-topbar">
              <Link
                to="/"
                className="button button-secondary page-nav-button"
              >
                Home
              </Link>
            </div>

            <div className="content-centered teams-creator-header">
              <h1 className="title page-title teams-creator-title pill-title">Your Teams</h1>

              <Link
                to="/create-team"
                className="button button-primary teams-creator-create-button"
              >
                Create a New Team
              </Link>
            </div>

            {pageError ? (
              <p className="form-error page-message" role="alert">
                {pageError}
              </p>
            ) : null}

            {successMessage ? (
              <p className="alert-success">{successMessage}</p>
            ) : null}

            {loading ? (
              <div className="state-card">
                <p className="state-title">Loading teams...</p>
                <p className="state-text">
                  We are retrieving the teams you created.
                </p>
              </div>
            ) : teams.length === 0 ? (
              <div className="state-card">
                <p className="state-title">No teams found</p>
                <p className="state-text">
                  Create a new team to start inviting members.
                </p>
              </div>
            ) : (
              <section className="table-list teams-creator-table" aria-label="Your teams">
                <div className="data-table-row teams-creator-table-row data-table-head teams-creator-table-head">
                  <span>Name</span>
                  <span>Habits</span>
                  <span>Chat</span>
                  <span>Invite code</span>
                  <span>Members</span>
                  <span></span>
                </div>

                {teams.map((team) => {
                  const currentInviteCode = inviteCodes[team.teamId]?.[0];
                  const isGenerating =
                    pendingAction?.teamId === team.teamId &&
                    pendingAction.action === "generate";
                  const isInvalidating =
                    pendingAction?.teamId === team.teamId &&
                    pendingAction.action === "invalidate";
                  const isDeleting =
                    pendingAction?.teamId === team.teamId &&
                    pendingAction.action === "delete";

                  return (
                    <article
                      className="data-table-row teams-creator-table-row"
                      key={team.teamId}
                    >
                      <span className="teams-creator-team-name">
                        {team.name}
                      </span>

                      <Link
                        to={`/habits-creator?teamId=${encodeURIComponent(
                          team.teamId,
                        )}`}
                        className="button button-secondary table-row-button"
                      >
                        Show Habits
                      </Link>

                      <button
                        type="button"
                        className="button button-secondary table-row-button"
                        disabled
                      >
                        Show Chat
                      </button>

                      <div className="teams-creator-code-cell">
                        {currentInviteCode ? (
                          <>
                            <span className="teams-creator-code-value">
                              {currentInviteCode.code}
                            </span>

                            <span className="teams-creator-code-expiry">
                              Expires{" "}
                              {formatInviteExpiryDate(
                                currentInviteCode.expiryDate,
                              )}
                            </span>

                            <button
                              type="button"
                              className="button button-secondary table-row-button"
                              onClick={() =>
                                void handleInvalidateInviteCode(
                                  team.teamId,
                                  currentInviteCode.codeId,
                                )
                              }
                              disabled={isInvalidating}
                            >
                              {isInvalidating ? "Invalidating..." : "Invalidate"}
                            </button>
                          </>
                        ) : (
                          <span className="teams-creator-code-empty">
                            No Active Code
                          </span>
                        )}

                        <button
                          type="button"
                          className="button button-secondary table-row-button"
                          onClick={() => void handleGenerateInviteCode(team.teamId)}
                          disabled={isGenerating}
                        >
                          {isGenerating ? "Creating..." : "New code"}
                        </button>
                      </div>

                      <Link
                        to={`/member-list?teamId=${encodeURIComponent(
                          team.teamId,
                        )}`}
                        className="button button-secondary table-row-button"
                      >
                        Show List
                      </Link>

                      <button
                        type="button"
                        className="button button-secondary table-row-button teams-creator-danger-button"
                        onClick={() => void handleDeleteTeam(team)}
                        disabled={isDeleting}
                      >
                        {isDeleting ? "Deleting..." : "Delete team"}
                      </button>
                    </article>
                  );
                })}
              </section>
            )}
          </div>
        </div>
      </section>
    </main>
  );
}
