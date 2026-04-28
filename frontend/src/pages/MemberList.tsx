import { Link, useNavigate, useSearchParams } from "react-router-dom";
import { useEffect, useMemo, useState } from "react";
import "./MemberList.css";
import "../App.css";
import {
  clearStoredAuth,
  getStoredAuth,
  getTeam,
  getTeamErrorMessage,
  getTeamMembers,
  kickUser,
  TeamRequestError,
  type TeamDetailsDto,
  type TeamMemberDto,
} from "../services/Team";

function resolveErrorMessage(error: unknown): string {
  if (error instanceof TeamRequestError) {
    return error.message || getTeamErrorMessage(error.code);
  }

  return "Something went wrong while loading team members. Please try again.";
}

export default function MemberList() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const teamId = searchParams.get("teamId") ?? "";
  const auth = useMemo(() => getStoredAuth(), []);
  const [team, setTeam] = useState<TeamDetailsDto | null>(null);
  const [members, setMembers] = useState<TeamMemberDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [pageError, setPageError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [pendingMemberId, setPendingMemberId] = useState<string | null>(null);

  useEffect(() => {
    let isMounted = true;

    const loadMembers = async () => {
      setLoading(true);
      setPageError(null);

      if (!teamId) {
        if (isMounted) {
          setPageError("Choose a team first.");
          setLoading(false);
        }

        return;
      }

      if (!auth?.isLoggedIn || !auth.sessionId) {
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
        const [loadedTeam, loadedMembers] = await Promise.all([
          getTeam(auth, teamId),
          getTeamMembers(auth, teamId),
        ]);

        if (!isMounted) {
          return;
        }

        setTeam(loadedTeam);
        setMembers(
          loadedMembers.filter((member) => member.status === "Active"),
        );
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

    void loadMembers();

    return () => {
      isMounted = false;
    };
  }, [auth, navigate, teamId]);

  const handleKickMember = async (member: TeamMemberDto) => {
    const confirmed = window.confirm(`Kick ${member.name} from this team?`);

    if (!confirmed || !teamId) {
      return;
    }

    setPendingMemberId(member.memberId);
    setPageError(null);
    setSuccessMessage(null);

    try {
      await kickUser(auth, teamId, member.memberId);

      setMembers((currentMembers) =>
        currentMembers.filter(
          (currentMember) => currentMember.memberId !== member.memberId,
        ),
      );

      setSuccessMessage("Member kicked.");
    } catch (error) {
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
      setPendingMemberId(null);
    }
  };

  return (
    <main className="page">
      <section className="container">
        <div className="background-glow background-glow-left" />
        <div className="background-glow background-glow-right" />

        <div className="card page-card-shell">
          <div className="content member-list-content">
            <div className="page-topbar">
              <Link
                to="/"
                className="button button-secondary page-nav-button"
              >
                Home
              </Link>

              <Link
                to="/teams-creator"
                className="button button-secondary page-nav-button"
              >
                Teams
              </Link>
            </div>

            <div className="content-centered member-list-header">
              <h1 className="title member-list-title pill-title">Team members</h1>

              {team ? <p className="text member-list-text">{team.name}</p> : null}
            </div>

            {pageError ? (
              <p className="form-error member-list-message" role="alert">
                {pageError}
              </p>
            ) : null}

            {successMessage ? (
              <p className="alert-success">{successMessage}</p>
            ) : null}

            {loading ? (
              <div className="state-card">
                <p className="state-title">Loading members...</p>
                <p className="state-text">
                  We are retrieving the active team members.
                </p>
              </div>
            ) : members.length === 0 ? (
              <div className="state-card">
                <p className="state-title">No members found</p>
                <p className="state-text">
                  This team has no active members right now.
                </p>
              </div>
            ) : (
              <section className="member-list-table" aria-label="Team members">
                <div className="data-table-row member-list-table-row data-table-head member-list-table-head">
                  <span>Name</span>
                  <span>Email</span>
                  <span></span>
                </div>

                {members.map((member) => {
                  const isKicking = pendingMemberId === member.memberId;

                  return (
                    <article
                      className="data-table-row member-list-table-row"
                      key={member.memberId}
                    >
                      <span className="member-list-name">{member.name}</span>
                      <span className="member-list-email">{member.email}</span>

                      <button
                        type="button"
                        className="button button-secondary table-row-button member-list-kick-button"
                        onClick={() => void handleKickMember(member)}
                        disabled={isKicking}
                      >
                        {isKicking ? "Kicking..." : "Kick"}
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
