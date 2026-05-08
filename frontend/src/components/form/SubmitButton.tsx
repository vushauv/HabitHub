import type { FieldValues, FormState } from "react-hook-form"

export type SubmitButtonProps<T extends FieldValues> = {
  formState: FormState<T>
  disabled?: boolean,
  children: string
}

export default function SubmitButton<T extends FieldValues>({ formState, disabled, children }: SubmitButtonProps<T>) {
  return (
    <input
      className="button button-primary form-submit"
      type="submit"
      disabled={disabled || !formState.isValid}
      value={children}
    />
  )
}