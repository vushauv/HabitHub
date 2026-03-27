import { Navigate, Outlet } from "react-router-dom";

type AccountType = "Creator" | "Member";

type StoredAuth = {
  isLoggedIn?: boolean;
  userType?: AccountType;
  sessionId?: string | null;
  userId?: string | null;
};

type ProtectedRouteProps = {
  allowedUserType: AccountType;
};

function getStoredAuth(): StoredAuth | null {
  const rawAuth = localStorage.getItem("habithubAuth");

  if (!rawAuth) {
    return null;
  }

  try {
    return JSON.parse(rawAuth) as StoredAuth;
  } catch {
    localStorage.removeItem("habithubAuth");
    return null;
  }
}

export default function ProtectedRoute({
  allowedUserType,
}: ProtectedRouteProps) {
  const auth = getStoredAuth();

  if (!auth?.isLoggedIn) {
    return <Navigate to="/login" replace />;
  }

  if (auth.userType !== allowedUserType) {
    return (
      <Navigate
        to={auth.userType === "Creator" ? "/main-creator" : "/main-member"}
        replace
      />
    );
  }

  return <Outlet />;
}