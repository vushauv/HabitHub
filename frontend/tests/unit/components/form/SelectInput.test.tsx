import { fireEvent, render, renderHook, screen, waitFor } from "@testing-library/react";
import { expect, it, vi } from "vitest";
import SelectInput from "../../../../src/components/form/SelectInput";
import { useForm } from "react-hook-form";
import { useLens } from "@hookform/lenses";

it("renders label, value, placeholder and error correctly", () => {
  const { result } = renderHook(() => useForm({
    defaultValues: { selectInputTest: "Haskell Language Server" },
    errors: {
      selectInputTest: { type: "yes", message: "This is an error" }
    }
  }));
  const { result: resultLens } = renderHook(() => useLens(result.current));

  render(
    <SelectInput
      label="SelectInputTest"
      lens={resultLens.current.focus("selectInputTest")}
    >
      <option>John</option>
      <option>Haskell Language Server</option>
    </SelectInput>
  );

  const element = screen.getByLabelText("SelectInputTest");
  expect(element).toBeInTheDocument();
  expect(element).toHaveValue("Haskell Language Server");
  const errorElement = screen.getByText("This is an error");
  expect(errorElement).toBeInTheDocument();
  expect(element).toBeInvalid()
});

it("handles value changes correctly", async () => {
  const { result } = renderHook(() => useForm({
    defaultValues: { selectInputTest: "Haskell Language Server" }
  }));
  const { result: resultLens } = renderHook(() => useLens(result.current));
  
  const onSubmit = vi.fn();

  render(
    <form onSubmit={result.current.handleSubmit(onSubmit)}>
      <SelectInput
        label="SelectInputTest"
        lens={resultLens.current.focus("selectInputTest")}
      >
        <option>John</option>
        <option>Haskell Language Server</option>
      </SelectInput>
    </form>
  );

  const element = screen.getByLabelText("SelectInputTest");
  expect(element).toBeInTheDocument();

  fireEvent.change(element, { target: { value: "John" } });

  expect(element).toHaveValue("John");

  fireEvent.submit(element);

  await waitFor(() => {
    expect(onSubmit).toHaveBeenCalledOnce();
    expect(onSubmit.mock.lastCall![0]).toEqual({ selectInputTest: "John" });
  });
});

it("is disabled when form is disabled", () => {
  const { result } = renderHook(() => useForm({
    defaultValues: { selectInputTest: "Haskell Language Server" },
    disabled: true
  }));
  const { result: resultLens } = renderHook(() => useLens(result.current));

  render(
    <SelectInput
      label="SelectInputTest"
      lens={resultLens.current.focus("selectInputTest")}
    >
      <option>John</option>
      <option>Haskell Language Server</option>
    </SelectInput>
  );

  expect(screen.getByLabelText("SelectInputTest")).toBeDisabled();
});

it("submits form correctly when submitted", () => {
  const { result } = renderHook(() => useForm({
    defaultValues: { selectInputTest: "Haskell Language Server" }
  }));
  const { result: resultLens } = renderHook(() => useLens(result.current));
  
  const onSubmit = vi.fn();

  render(
    <form onSubmit={onSubmit}>
      <SelectInput
        label="SelectInputTest"
        lens={resultLens.current.focus("selectInputTest")}
      >
        <option>John</option>
        <option>Haskell Language Server</option>
      </SelectInput>
    </form>
  );

  fireEvent.submit(screen.getByLabelText("SelectInputTest"));
  expect(onSubmit).toHaveBeenCalledOnce();
});