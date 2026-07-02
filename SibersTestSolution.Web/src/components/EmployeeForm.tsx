import { useState } from "react";
import { KeyRound } from "lucide-react";
import type { Employee, EmployeePayload, UserRole } from "../types";

const emailPattern = /^[^@\s]+@[^@\s]+\.[^@\s]+$/;
const userNamePattern = /^[A-Za-z0-9._@+-]+$/;

const roleLabels: Record<UserRole, string> = {
  Employee: "Сотрудник",
  ProjectManager: "Менеджер проекта",
  Head: "Руководитель"
};

type EmployeeFormProps = {
  employee?: Employee;
  onSubmit: (payload: EmployeePayload) => Promise<void>;
  onCancel: () => void;
};

export function EmployeeForm({ employee, onSubmit, onCancel }: EmployeeFormProps) {
  const existingRole = employee?.roles?.[0] ?? null;
  const hasExistingAccess = Boolean(employee?.userName || existingRole);
  const [form, setForm] = useState<EmployeePayload>({
    name: employee?.name ?? "",
    lastName: employee?.lastName ?? "",
    middleName: employee?.middleName ?? "",
    email: employee?.email ?? "",
    userName: employee?.userName ?? null,
    password: null,
    role: existingRole
  });
  const [shouldCreateUser, setShouldCreateUser] = useState(hasExistingAccess);
  const [isSaving, setIsSaving] = useState(false);
  const [emailError, setEmailError] = useState<string | null>(null);
  const [loginError, setLoginError] = useState<string | null>(null);
  const [passwordError, setPasswordError] = useState<string | null>(null);

  function showLoginFields() {
    const emailAsLogin = form.email.trim();

    setShouldCreateUser(true);
    setForm({
      ...form,
      userName: userNamePattern.test(emailAsLogin) ? emailAsLogin : "",
      password: form.password ?? "admin",
      role: form.role ?? "Employee"
    });
  }

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();

    const normalizedEmail = form.email.trim();
    const normalizedUserName = form.userName?.trim() ?? "";
    const normalizedPassword = form.password?.trim() ?? "";

    if (!emailPattern.test(normalizedEmail)) {
      setEmailError("Введите корректный email");
      return;
    }

    if (shouldCreateUser && !userNamePattern.test(normalizedUserName)) {
      setLoginError("Логин может содержать только латиницу, цифры и . _ @ + -");
      return;
    }

    if (shouldCreateUser && !hasExistingAccess && normalizedPassword.length === 0) {
      setPasswordError("Введите пароль");
      return;
    }

    setEmailError(null);
    setLoginError(null);
    setPasswordError(null);
    setIsSaving(true);

    try {
      await onSubmit({
        ...form,
        email: normalizedEmail,
        userName: shouldCreateUser ? normalizedUserName : null,
        password: shouldCreateUser && normalizedPassword.length > 0 ? normalizedPassword : null,
        role: shouldCreateUser ? form.role : null
      });
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <form className="entity-form" onSubmit={handleSubmit}>
      <label>
        <span>Фамилия</span>
        <input value={form.lastName} onChange={(event) => setForm({ ...form, lastName: event.target.value })} />
      </label>
      <label>
        <span>Имя</span>
        <input value={form.name} onChange={(event) => setForm({ ...form, name: event.target.value })} />
      </label>
      <label>
        <span>Отчество</span>
        <input value={form.middleName} onChange={(event) => setForm({ ...form, middleName: event.target.value })} />
      </label>
      <label>
        <span>Email</span>
        <input
          type="email"
          inputMode="email"
          autoComplete="email"
          required
          pattern="^[^@\s]+@[^@\s]+\.[^@\s]+$"
          value={form.email}
          onChange={(event) => {
            setForm({ ...form, email: event.target.value });
            setEmailError(null);
          }}
        />
        {emailError ? <small className="field-error">{emailError}</small> : null}
      </label>
      <div className="login-action-row">
        <span className="field-label">Доступ</span>
        {hasExistingAccess ? (
          <span className="access-status">
            <KeyRound size={15} />
            Выдан
          </span>
        ) : (
          <button className="login-toggle-button" type="button" onClick={showLoginFields}>
            <KeyRound size={15} />
            {employee ? "Выдать логин" : "Создать логин"}
          </button>
        )}
      </div>
      {shouldCreateUser ? (
        <div className="login-fields">
          <label>
            <span>Логин</span>
            <input
              required
              inputMode="email"
              pattern="^[A-Za-z0-9._@+\-]+$"
              value={form.userName ?? ""}
              onChange={(event) => {
                setForm({ ...form, userName: event.target.value });
                setLoginError(null);
              }}
            />
            {loginError ? <small className="field-error">{loginError}</small> : null}
          </label>
          <label>
            <span>{hasExistingAccess ? "Новый пароль" : "Пароль"}</span>
            <input
              required={!hasExistingAccess}
              type="password"
              placeholder={hasExistingAccess ? "Оставьте пустым, чтобы не менять" : undefined}
              value={form.password ?? ""}
              onChange={(event) => {
                setForm({ ...form, password: event.target.value });
                setPasswordError(null);
              }}
            />
            {passwordError ? <small className="field-error">{passwordError}</small> : null}
          </label>
          <label>
            <span>Роль</span>
            <select
              value={form.role ?? "Employee"}
              onChange={(event) => setForm({ ...form, role: event.target.value as EmployeePayload["role"] })}
            >
              {Object.entries(roleLabels).map(([role, label]) => (
                <option key={role} value={role}>
                  {label}
                </option>
              ))}
            </select>
          </label>
        </div>
      ) : null}
      <div className="form-actions">
        <button className="secondary-button" type="button" onClick={onCancel}>
          Отмена
        </button>
        <button className="primary-button" type="submit" disabled={isSaving}>
          Сохранить
        </button>
      </div>
    </form>
  );
}
