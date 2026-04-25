import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { it, expect } from "vitest";
import Home from "../../../src/pages/Home";

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
    screen.getByRole("link", { name: "Log in" }),
  ).toHaveAttribute("href", "/login");
});