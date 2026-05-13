import { fireEvent, render, renderHook, screen, waitFor } from "@testing-library/react";
import { expect, it, vi } from "vitest";
import TextInput from "../../../../src/components/form/TextInput";
import { useForm } from "react-hook-form";
import { useLens } from "@hookform/lenses";

it("renders label, value, placeholder and error correctly", () => {
  const { result } = renderHook(() => useForm({
    defaultValues: { textInputTest: "John" },
    errors: {
      textInputTest: { type: "yes", message: "This is an error" }
    }
  }));
  const { result: resultLens } = renderHook(() => useLens(result.current));

  render(
    <TextInput
      label="TextInputTest"
      lens={resultLens.current.focus("textInputTest")}
      placeholder="hi :3"
    />
  );

  const element = screen.getByLabelText("TextInputTest");
  expect(element).toBeInTheDocument();
  expect(element).toHaveValue("John");
  expect(element).toHaveAttribute("placeholder", "hi :3");
  const errorElement = screen.getByText("This is an error");
  expect(errorElement).toBeInTheDocument();
  expect(element).toBeInvalid()
});

it("handles value changes correctly", async () => {
  const { result } = renderHook(() => useForm({
    defaultValues: { textInputTest: "John" }
  }));
  const { result: resultLens } = renderHook(() => useLens(result.current));

  const onSubmit = vi.fn();
  render(
    <form onSubmit={result.current.handleSubmit(onSubmit)}>
      <TextInput
        label="TextInputTest"
        lens={resultLens.current.focus("textInputTest")}
      />
    </form>
  );

  const element = screen.getByLabelText("TextInputTest");
  expect(element).toBeInTheDocument();

  fireEvent.change(element, { target: { value: "John ;)" } });

  expect(element).toHaveValue("John ;)");

  fireEvent.submit(element);

  await waitFor(() => {
    expect(onSubmit).toHaveBeenCalledOnce();
    expect(onSubmit.mock.lastCall![0]).toEqual({ textInputTest: "John ;)" });
  });
});

it("is disabled when form is disabled", () => {
  const { result } = renderHook(() => useForm({
    defaultValues: { textInputTest: "John" },
    disabled: true
  }));
  const { result: resultLens } = renderHook(() => useLens(result.current));

  render(
    <TextInput
      label="TextInputTest"
      lens={resultLens.current.focus("textInputTest")}
    />
  );

  expect(screen.getByLabelText("TextInputTest")).toBeDisabled();
});

it("submits form correctly when submitted", () => {
  const { result } = renderHook(() => useForm({
    defaultValues: { textInputTest: "John" }
  }));
  const { result: resultLens } = renderHook(() => useLens(result.current));
  
  const onSubmit = vi.fn();

  render(
    <form onSubmit={onSubmit}>
      <TextInput
        label="TextInputTest"
        lens={resultLens.current.focus("textInputTest")}
      />
    </form>);

  fireEvent.submit(screen.getByLabelText("TextInputTest"));
  expect(onSubmit).toHaveBeenCalledOnce();
});