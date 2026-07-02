import { useMemo, useState } from "react";
import { ChevronRight, Search } from "lucide-react";
import { EmployeeList } from "./EmployeeList";
import { TaskList } from "./TaskList";
import { formatEmployee } from "../utils/format";
import type { Employee, PanelState, ProjectTask } from "../types";

type DetailsPanelProps = {
  panel: PanelState | null;
  employees: Employee[];
  tasks: ProjectTask[];
  onClose: () => void;
  onAddEmployee: (projectId: number, employeeId: number) => Promise<void>;
  onRemoveEmployee: (projectId: number, employeeId: number) => Promise<void>;
  canManageProjectEmployees: boolean;
};

export function DetailsPanel({
  panel,
  employees,
  tasks,
  onClose,
  onAddEmployee,
  onRemoveEmployee,
  canManageProjectEmployees
}: DetailsPanelProps) {
  const [query, setQuery] = useState("");

  const currentEmployeeIds = useMemo(() => {
    return new Set(panel?.kind === "employees" ? panel.project.employees.map((employee) => employee.id) : []);
  }, [panel]);

  const availableEmployees = useMemo(() => {
    const normalizedQuery = query.trim().toLowerCase();

    return employees.filter((employee) => {
      if (!normalizedQuery) {
        return true;
      }

      return `${formatEmployee(employee)} ${employee.email}`.toLowerCase().includes(normalizedQuery);
    });
  }, [employees, query]);

  const visibleTasks = useMemo(() => {
    const normalizedQuery = query.trim().toLowerCase();

    if (!normalizedQuery) {
      return tasks;
    }

    return tasks.filter((task) =>
      `${task.name} ${formatEmployee(task.taskOwner)} ${formatEmployee(task.taskPerformer)}`
        .toLowerCase()
        .includes(normalizedQuery)
    );
  }, [query, tasks]);

  if (!panel) {
    return null;
  }

  return (
    <aside className="details-panel">
      <header>
        <button className="icon-button compact" type="button" title="Закрыть" onClick={onClose}>
          <ChevronRight size={18} />
        </button>
        <div>
          <p className="eyebrow">{panel.project.name}</p>
          <h2>{panel.kind === "employees" ? "Работники" : "Задачи"}</h2>
        </div>
      </header>

      <div className="panel-tools">
        <label className="search-field full-width">
          <Search aria-hidden="true" size={18} />
          <input
            value={query}
            onChange={(event) => setQuery(event.target.value)}
            placeholder={panel.kind === "employees" ? "Найти работника для проекта" : "Найти задачу"}
          />
        </label>
      </div>

      {panel.kind === "employees" ? (
        <>
          <EmployeeList
            employees={panel.project.employees}
            projectId={panel.project.id}
            onRemoveEmployee={canManageProjectEmployees ? onRemoveEmployee : undefined}
          />
          {canManageProjectEmployees ? (
            <>
              <div className="panel-section-title">Добавить в проект</div>
              <EmployeeList
                employees={availableEmployees}
                projectId={panel.project.id}
                addedEmployeeIds={currentEmployeeIds}
                onAddEmployee={onAddEmployee}
              />
            </>
          ) : null}
        </>
      ) : (
        <TaskList tasks={visibleTasks} />
      )}
    </aside>
  );
}
