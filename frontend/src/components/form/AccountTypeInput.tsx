import type { Lens } from '@hookform/lenses';
import { useController } from 'react-hook-form';
import type { AccountType } from '../../services/User';

export type AccountTypeInputProps = {
  label: string,
  lens: Lens<AccountType>,
}

export default function AccountTypeInput({ label, lens }: AccountTypeInputProps) {
  const { field, fieldState } = useController(lens.interop());
  return (
    <div className="form-field">
      <span className="form-label">{label}</span>

      <div
        className="role-group"
        role="radiogroup"
        aria-label={label}
      >
        <button
          type="button"
          className={
            field.value === "Creator"
              ? "role-button role-button-active"
              : "role-button"
          }
          onClick={() => field.onChange("Creator")}
          disabled={field.disabled}
          aria-pressed={field.value === "Creator"}
        >
          Creator
        </button>

        <button
          type="button"
          className={
            field.value === "Member"
              ? "role-button role-button-active"
              : "role-button"
          }
          onClick={() => field.onChange("Member")}
          disabled={field.disabled}
          aria-pressed={field.value === "Member"}
        >
          Member
        </button>
      </div>

      {fieldState.error != null && (
        <p className="field-error">{fieldState.error.message}</p>
      )}
    </div>
  )
}