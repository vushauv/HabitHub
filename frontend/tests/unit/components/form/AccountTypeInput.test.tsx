import { fireEvent, render, renderHook, screen, waitFor } from "@testing-library/react";
import { expect, it, vi } from "vitest";
import AccountTypeInput from "../../../../src/components/form/AccountTypeInput";
import { useForm } from "react-hook-form";
import { useLens } from "@hookform/lenses";
import type { AccountType } from "../../../../src/services/User";

it("renders label, value, placeholder and error correctly", () => {
  const { result } = renderHook(() => useForm<{ accountType: AccountType }>({
    defaultValues: { accountType: "Creator" },
    errors: {
      accountType: { type: "yes", message: "This is an error" }
    }
  }));
  const { result: resultLens } = renderHook(() => useLens(result.current));

  render(
    <AccountTypeInput
      label="AccountTypeTest"
      lens={resultLens.current.focus("accountType")}
    />
  );

  const creatorButton = screen.getByText("Creator");
  const memberButton = screen.getByText("Member");

  expect(creatorButton).toBeInTheDocument();
  expect(memberButton).toBeInTheDocument();
  
  expect(creatorButton).toBePressed();
  expect(memberButton).not.toBePressed();

  const errorElement = screen.getByText("This is an error");
  expect(errorElement).toBeInTheDocument();
});

it("handles value changes correctly", async () => {
  const { result } = renderHook(() => useForm<{ accountType: AccountType }>({
    defaultValues: { accountType: "Creator" }
  }));
  const { result: resultLens } = renderHook(() => useLens(result.current));
  
  const onSubmit = vi.fn();

  render(
    <form onSubmit={result.current.handleSubmit(onSubmit)}>
      <AccountTypeInput
        label="AccountTypeTest"
        lens={resultLens.current.focus("accountType")}
      />
    </form>
  );

  const creatorButton = screen.getByText("Creator");
  const memberButton = screen.getByText("Member");

  expect(creatorButton).toBeInTheDocument();
  expect(memberButton).toBeInTheDocument();
  
  expect(creatorButton).toBePressed();
  expect(memberButton).not.toBePressed();

  fireEvent.click(memberButton)

  expect(creatorButton).not.toBePressed();
  expect(memberButton).toBePressed();

  fireEvent.submit(memberButton);

  await waitFor(() => {
    expect(onSubmit).toHaveBeenCalledOnce();
    expect(onSubmit.mock.lastCall![0]).toEqual({ accountType: "Member" });
  });
});

it("is disabled when form is disabled", () => {
  const { result } = renderHook(() => useForm<{ accountType: AccountType }>({
    defaultValues: { accountType: "Creator" },
    disabled: true
  }));
  const { result: resultLens } = renderHook(() => useLens(result.current));

  render(
    <AccountTypeInput
      label="AccountTypeTest"
      lens={resultLens.current.focus("accountType")}
    />
  );

  const buttons = screen.getAllByRole("button");
  expect(buttons).toHaveLength(2);
  expect(buttons[0]).toBeDisabled();
  expect(buttons[1]).toBeDisabled();
});

it("submits form correctly when submitted", () => {
  const { result } = renderHook(() => useForm<{ accountType: AccountType }>({
    defaultValues: { accountType: "Creator" }
  }));
  const { result: resultLens } = renderHook(() => useLens(result.current));
  
  const onSubmit = vi.fn();

  render(
    <form onSubmit={onSubmit}>
      <AccountTypeInput
        label="AccountTypeTest"
        lens={resultLens.current.focus("accountType")}
      />
    </form>
  );

  fireEvent.submit(screen.getByLabelText("AccountTypeTest"));
  expect(onSubmit).toHaveBeenCalledOnce();
});