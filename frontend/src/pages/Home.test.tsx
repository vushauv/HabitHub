import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { describe, it, expect } from "vitest";
import Home from "./Home";

describe("Home", () => {
  it("renders heading and register link", () => {
    render(
      <MemoryRouter>
        <Home />
      </MemoryRouter>,
    );

    expect(screen.getByRole("heading", { name: "HabitHub" })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Get Started" })).toHaveAttribute("href", "/register");
  });
});
