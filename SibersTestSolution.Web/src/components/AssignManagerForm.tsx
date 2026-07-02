import { useMemo, useState } from "react";
import { Search } from "lucide-react";
import { formatEmployee } from "../utils/format";
import type { Employee, Project } from "../types";

type AssignManagerFormProps = {
  project: Project;
  employees: Employee[];
  onSubmit: (employeeId: number) => Promise<void>;
  onCancel: () => void;
};

export function AssignManagerForm({ project, employees, onSubmit, onCancel }: AssignManagerFormProps) {
  const [query, setQuery] = useState("");
  const managerCandidates = useMemo(() => {
    return employees.filter((employee) => employee.roles?.includes("ProjectManager"));
  }, [employees]);
  const [selectedEmployeeId, setSelectedEmployeeId] = useState(
    managerCandidates.some((employee) => employee.id === project.projectManagerId)
      ? project.projectManagerId ?? 0
      : managerCandidates[0]?.id ?? 0
  );
  const [isSaving, setIsSaving] = useState(false);

  const filteredEmployees = useMemo(() => {
    const normalizedQuery = query.trim().toLowerCase();

    if (!normalizedQuery) {
      return managerCandidates;
    }

    return managerCandidates.filter((employee) =>
      `${formatEmployee(employee)} ${employee.email}`.toLowerCase().includes(normalizedQuery)
    );
  }, [managerCandidates, query]);

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setIsSaving(true);

    try {
      await onSubmit(selectedEmployeeId);
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <form className="entity-form" onSubmit={handleSubmit}>
      <label className="search-field full-width">
        <Search aria-hidden="true" size={18} />
        <input value={query} onChange={(event) => setQuery(event.target.value)} placeholder="Найти менеджера" />
      </label>
      <div className="picker-list">
        {filteredEmployees.map((employee) => (
          <label className="picker-row" key={employee.id}>
            <input
              type="radio"
              checked={selectedEmployeeId === employee.id}
              onChange={() => setSelectedEmployeeId(employee.id)}
            />
            <span>
              <strong>{formatEmployee(employee)}</strong>
              <small>{employee.email}</small>
            </span>
          </label>
        ))}
      </div>
      <div className="form-actions">
        <button className="secondary-button" type="button" onClick={onCancel}>
          Отмена
        </button>
        <button className="primary-button" type="submit" disabled={isSaving || selectedEmployeeId <= 0}>
          Назначить
        </button>
      </div>
    </form>
  );
}
