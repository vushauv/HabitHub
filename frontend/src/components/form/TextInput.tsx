import type { Lens } from '@hookform/lenses';
import { useController } from 'react-hook-form';

export type TextInputProps = {
  label: string,
  lens: Lens<string>,
} & Pick<React.InputHTMLAttributes<HTMLInputElement>, "className" | "required" | "type" | "placeholder" | "autoComplete">

export default function TextInput({ label, lens, className, ...inputProps }: TextInputProps) {
  const { field, fieldState } = useController(lens.interop())
  const name = field.name;
  return (
    <div className={"form-field "+className}>
      <label className="form-label" htmlFor={name}>
        {label}
      </label>
      <input
        {...field}
        {...inputProps}
        id={name}
        className="form-input"
        aria-invalid={fieldState.invalid}
        aria-describedby={fieldState.error != null ? name+"-error" : undefined}
      />
      {fieldState.error != null && (
        <p id={name+"-error"} className="field-error">
          {fieldState.error.message ?? "Error"}
        </p>
      )}
    </div>
  )
}