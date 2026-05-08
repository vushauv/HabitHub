import { render, screen } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { it, expect, beforeEach, afterAll } from "vitest";
import ProtectedRoute from "../../src/ProtectedRoute";

function renderApp(initialPath: string) {
  render(
    <MemoryRouter initialEntries={[initialPath]}>
      <Routes>
        <Route path="/login" element={<div>Login Page</div>} />
        <Route element={<ProtectedRoute allowedUserType="Creator" />}>
          <Route path="/main-creator" element={<div>Creator Dashboard</div>} />
        </Route>
        <Route element={<ProtectedRoute allowedUserType="Member" />}>
          <Route path="/main-member" element={<div>Member Dashboard</div>} />
        </Route>
      </Routes>
    </MemoryRouter>,
  );
}

beforeEach(() => {
  localStorage.clear();
});
afterAll(() => {
  localStorage.clear();
});

it("redirects to /login when not authenticated", () => {
  renderApp("/main-creator");

  expect(screen.getByText("Login Page")).toBeInTheDocument();
});

it("redirects to /login when isLoggedIn is false", () => {
  localStorage.setItem(
    "habithubAuth",
    JSON.stringify({ isLoggedIn: false, userType: "Creator" }),
  );

  renderApp("/main-creator");

  expect(screen.getByText("Login Page")).toBeInTheDocument();
});

// TODO: Fix
it.skip("renders Creator Dashboard when authenticated as Creator", () => {
  localStorage.setItem(
    "habithubAuth",
    JSON.stringify({ isLoggedIn: true, userType: "Creator" }),
  );

  renderApp("/main-creator");

  expect(screen.getByText("Creator Dashboard")).toBeInTheDocument();
});

// TODO: Fix
it.skip("renders Member Dashboard when authenticated as Member", () => {
  localStorage.setItem(
    "habithubAuth",
    JSON.stringify({ isLoggedIn: true, userType: "Member" }),
  );

  renderApp("/main-member");

  expect(screen.getByText("Member Dashboard")).toBeInTheDocument();
});

// TODO: Fix
it.skip("redirects Creator to /main-creator when accessing Member route", () => {
  localStorage.setItem(
    "habithubAuth",
    JSON.stringify({ isLoggedIn: true, userType: "Creator" }),
  );

  renderApp("/main-member");

  expect(screen.getByText("Creator Dashboard")).toBeInTheDocument();
});

it("redirects to /login and clears storage when localStorage contains corrupted data", () => {
  localStorage.setItem("habithubAuth", "not-valid-json{{{");

  renderApp("/main-creator");

  expect(screen.getByText("Login Page")).toBeInTheDocument();
  expect(localStorage.getItem("habithubAuth")).toBeNull();
});