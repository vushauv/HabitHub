import { render, screen } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { describe, it, expect, beforeEach } from "vitest";
import ProtectedRoute from "../../src/ProtectedRoute";

function renderWithRoutes(initialPath: string, allowedUserType: "Creator" | "Member") {
  render(
    <MemoryRouter initialEntries={[initialPath]}>
      <Routes>
        <Route path="/login" element={<div>Login Page</div>} />
        <Route path="/main-creator" element={<div>Creator Dashboard</div>} />
        <Route path="/main-member" element={<div>Member Dashboard</div>} />
        <Route element={<ProtectedRoute allowedUserType={allowedUserType} />}>
          <Route
            path={allowedUserType === "Creator" ? "/main-creator" : "/main-member"}
            element={
              <div>
                {allowedUserType === "Creator" ? "Creator Dashboard" : "Member Dashboard"}
              </div>
            }
          />
        </Route>
      </Routes>
    </MemoryRouter>,
  );
}

describe("ProtectedRoute", () => {
  beforeEach(() => {
    localStorage.clear();
  });

  it("redirects to /login when not authenticated", () => {
    renderWithRoutes("/main-creator", "Creator");

    expect(screen.getByText("Login Page")).toBeInTheDocument();
  });

  it("redirects to /login when isLoggedIn is false", () => {
    localStorage.setItem(
      "habithubAuth",
      JSON.stringify({ isLoggedIn: false, userType: "Creator" }),
    );

    renderWithRoutes("/main-creator", "Creator");

    expect(screen.getByText("Login Page")).toBeInTheDocument();
  });

  it("renders protected content when authenticated with correct userType", () => {
    localStorage.setItem(
      "habithubAuth",
      JSON.stringify({ isLoggedIn: true, userType: "Creator" }),
    );

    renderWithRoutes("/main-creator", "Creator");

    expect(screen.getByText("Creator Dashboard")).toBeInTheDocument();
  });

  it("redirects Creator to /main-creator when accessing Member route", () => {
    localStorage.setItem(
      "habithubAuth",
      JSON.stringify({ isLoggedIn: true, userType: "Creator" }),
    );

    renderWithRoutes("/main-member", "Member");

    expect(screen.getByText("Creator Dashboard")).toBeInTheDocument();
  });

  it("redirects to /login when localStorage contains corrupted data", () => {
    localStorage.setItem("habithubAuth", "not-valid-json{{{");

    renderWithRoutes("/main-creator", "Creator");

    expect(screen.getByText("Login Page")).toBeInTheDocument();
    expect(localStorage.getItem("habithubAuth")).toBeNull();
  });
});
