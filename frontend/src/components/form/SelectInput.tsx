import type { Lens } from '@hookform/lenses';
import { useController } from 'react-hook-form';

export type SelectInputProps<T extends string = string> = {
  label: string,
  lens: Lens<T>,
} & Pick<React.SelectHTMLAttributes<HTMLSelectElement>, "required" | "children" | "className">

export default function SelectInput<T extends string>({ label, lens, children, className, ...inputProps }: SelectInputProps<T>) {
  const { field, fieldState } = useController((lens as unknown as Lens<string>).interop())
  const name = field.name;
  return (
    <div className={`form-field ${className ?? ""}`}>
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
