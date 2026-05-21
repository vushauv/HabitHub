import { useEffect } from "react";
import { Navigate, Outlet } from "react-router-dom";
import { useCurrentUser } from "./hooks/useCurrentUser";
import {
  clearStoredAuth,
  getAccountTypeForUser,
  getDashboardPathForUser,
  getStoredAuth,
} from "./services/Auth";
import type { AccountType } from "./services/User";

type ProtectedRouteProps = {
  allowedUserType?: AccountType;
};

export default function ProtectedRoute({
  allowedUserType,
}: ProtectedRouteProps) {
  const auth = getStoredAuth();
  const { currentUser, isLoading, error } = useCurrentUser(auth);

  useEffect(() => {
    if (!error) {
      return;
    }

    if (error.code === "auth-required" || error.code === "not-found") {
      clearStoredAuth();
    }
  }, [error]);

  if (!auth) {
    return <Navigate to="/login" replace />;
  }

  if (isLoading) {
    return (
      <main className="page container">
        <section className="card">
          <div className="content content-centered">
            <p className="text">Loading your account...</p>
          </div>
        </section>
      </main>
    );
  }

  if (error) {
    if (error.code === "auth-required" || error.code === "not-found") {
      return <Navigate to="/login" replace />;
    }

    return (
      <main className="page container">
        <section className="card">
          <div className="content content-centered">
            <p className="form-error" role="alert">
              {error.message}
            </p>
          </div>
        </section>
      </main>
    );
  }

  if (!currentUser) {
    return <Navigate to="/login" replace />;
  }

  if (
    allowedUserType &&
    getAccountTypeForUser(currentUser) !== allowedUserType
  ) {
    return <Navigate to={getDashboardPathForUser(currentUser)} replace />;
  }

  return <Outlet context={currentUser} />;
}
