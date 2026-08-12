type AppCheckboxProps = {
  checked: boolean;
  label: string;
  onChange: (checked: boolean) => void;
};

export const AppCheckbox = ({ checked, label, onChange }: AppCheckboxProps) => (
  <label className="flex items-center gap-2 text-sm font-medium">
    <input checked={checked} className="h-4 w-4 accent-primary" onChange={(event) => onChange(event.target.checked)} type="checkbox" />
    {label}
  </label>
);
