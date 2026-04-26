import type { Lens } from '@hookform/lenses';
import { useController } from 'react-hook-form';

export type SelectInputProps = {
  label: string,
  lens: Lens<string>,
} & Pick<React.SelectHTMLAttributes<HTMLSelectElement>, "required" | "children">

export default function SelectInput({ label, lens, children, ...inputProps }: SelectInputProps) {
  const { field, fieldState } = useController(lens.interop())
  const name = field.name;
  return (
    <div className="form-field">
      <label className="form-label" htmlFor={name}>
        {label}
      </label>
      <select
        {...field}
        {...inputProps}
        id={name}
        className="form-input"
        aria-invalid={fieldState.invalid}
        aria-describedby={(fieldState.invalid && fieldState.error != null) ? name+"-error" : undefined}
      >
        {children}
      </select>
      {(fieldState.invalid && fieldState.error != null) && (
        <p id={name+"-error"} className="field-error">
          {fieldState.error.message ?? "Error"}
        </p>
      )}
    </div>
  )
}