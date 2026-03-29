import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { describe, it, expect } from "vitest";
import Home from "./Home";

describe("Home", () => {
  it("renders heading and navigation links", () => {
    render(
      <MemoryRouter>
        <Home />
      </MemoryRouter>,
    );

    expect(
      screen.getByRole("heading", { name: "HabitHub" }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole("link", { name: "Get started" }),
    ).toHaveAttribute("href", "/register");

    expect(
      screen.getByRole("link", { name: "Sign in" }),
    ).toHaveAttribute("href", "/login");
  });
});