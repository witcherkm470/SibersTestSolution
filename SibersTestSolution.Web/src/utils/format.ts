import type { Employee } from "../types";

export const taskStatuses: Record<number, string> = {
  1: "Todo",
  2: "In progress",
  3: "Done"
};

export function formatEmployee(employee: Employee | null): string {
  if (!employee) {
    return "Не назначен";
  }

  if (employee.isDeleted) {
    return "Пользователь удален";
  }

  return [employee.lastName, employee.name, employee.middleName].filter(Boolean).join(" ");
}

export function formatDate(value: string): string {
  return new Intl.DateTimeFormat("ru-RU", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric"
  }).format(new Date(value));
}

export function toDateInputValue(value: string): string {
  return value.slice(0, 10);
}
