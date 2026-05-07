import { useEffect, useState } from "react";
import type { UserDto } from "../services/dtos";
import {
  AuthRequestError,
  getCurrentUser,
  type StoredAuth,
} from "../services/Auth";

type UseCurrentUserResult = {
  currentUser: UserDto | null;
  isLoading: boolean;
  error: AuthRequestError | null;
};

export function useCurrentUser(auth: StoredAuth | null): UseCurrentUserResult {
  const [currentUser, setCurrentUser] = useState<UserDto | null>(null);
  const [isLoading, setIsLoading] = useState(auth !== null);
  const [error, setError] = useState<AuthRequestError | null>(null);

  useEffect(() => {
    let isMounted = true;

    if (!auth) {
      setCurrentUser(null);
      setError(null);
      setIsLoading(false);
      return () => {
        isMounted = false;
      };
    }

    setIsLoading(true);
    setCurrentUser(null);
    setError(null);

    void getCurrentUser(auth)
      .then((user) => {
        if (!isMounted) {
          return;
        }

        setCurrentUser(user);
      })
      .catch((caughtError: unknown) => {
        if (!isMounted) {
          return;
        }

        setCurrentUser(null);
        setError(
          caughtError instanceof AuthRequestError
            ? caughtError
            : new AuthRequestError(
                500,
                "unknown",
                "We could not load your account right now. Please try again.",
              ),
        );
      })
      .finally(() => {
        if (isMounted) {
          setIsLoading(false);
        }
      });

    return () => {
      isMounted = false;
    };
  }, [auth?.sessionId]);

  return {
    currentUser,
    isLoading,
    error,
  };
}
