import { fireEvent, render, renderHook, screen } from "@testing-library/react";
import { expect, it, vi } from "vitest";
import SubmitButton from "../../../../src/components/form/SubmitButton";
import { useForm } from "react-hook-form";

it("renders text correctly", () => {
  const { result } = renderHook(() => useForm());

  render(<SubmitButton formState={result.current.formState}>ThisIsSubmit</SubmitButton>);

  expect(screen.getByRole("button", { name: "ThisIsSubmit" })).toBeInTheDocument()
});

it("is disabled when form is invalid", () => {
  const { result } = renderHook(() => useForm());
  
  render(<SubmitButton formState={{...result.current.formState, isValid: false}}>Submit</SubmitButton>);

  expect(screen.getByRole("button", { name: "Submit" })).toBeDisabled()
});

it("is enabled when form is valid", () => {
  const { result } = renderHook(() => useForm());
  
  render(<SubmitButton formState={{...result.current.formState, isValid: true}}>Submit</SubmitButton>);

  expect(screen.getByRole("button", { name: "Submit" })).toBeEnabled()
});

it("is disabled when form is valid but it's manually disabled", () => {
  const { result } = renderHook(() => useForm());
  
  render(
    <SubmitButton
      formState={{...result.current.formState, isValid: false}}
      disabled
    >
      Submit
    </SubmitButton>);

  expect(screen.getByRole("button", { name: "Submit" })).toBeDisabled()
});

it("submits form correctly when clicked", () => {
  const { result } = renderHook(() => useForm());
  
  const onSubmit = vi.fn();

  render(
    <form onSubmit={onSubmit}>
      <SubmitButton
        formState={{...result.current.formState, isValid: true}}
      >
        Submit
      </SubmitButton>
    </form>);

  fireEvent.click(screen.getByRole("button", { name: "Submit" }));
  expect(onSubmit).toHaveBeenCalledOnce();
});