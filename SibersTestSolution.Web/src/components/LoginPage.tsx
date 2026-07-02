import { useState } from "react";
import type { CurrentUser, LoginPayload } from "../types";

type LoginPageProps = {
  onLogin: (payload: LoginPayload) => Promise<CurrentUser>;
};

export function LoginPage({ onLogin }: LoginPageProps) {
  const [form, setForm] = useState<LoginPayload>({
    userName: "admin",
    password: "admin"
  });
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);
    setIsLoading(true);

    try {
      await onLogin(form);
    } catch (loginError) {
      setError(loginError instanceof Error ? loginError.message : "Не удалось войти");
    } finally {
      setIsLoading(false);
    }
  }

  return (
    <main className="login-shell">
      <form className="login-form" onSubmit={handleSubmit}>
        <div>
          <p className="eyebrow">Sibers Test</p>
          <h1>Вход</h1>
        </div>
        {error ? <div className="state-banner error">{error}</div> : null}
        <label>
          <span>Логин</span>
          <input value={form.userName} onChange={(event) => setForm({ ...form, userName: event.target.value })} />
        </label>
        <label>
          <span>Пароль</span>
          <input
            type="password"
            value={form.password}
            onChange={(event) => setForm({ ...form, password: event.target.value })}
          />
        </label>
        <button className="primary-button" type="submit" disabled={isLoading}>
          Войти
        </button>
      </form>
    </main>
  );
}
