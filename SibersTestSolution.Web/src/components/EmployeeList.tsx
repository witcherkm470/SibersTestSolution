import { Plus, X } from "lucide-react";
import { formatEmployee } from "../utils/format";
import type { Employee } from "../types";

type EmployeeListProps = {
  employees: Employee[];
  projectId?: number;
  addedEmployeeIds?: Set<number>;
  onAddEmployee?: (projectId: number, employeeId: number) => Promise<void>;
  onRemoveEmployee?: (projectId: number, employeeId: number) => Promise<void>;
};

export function EmployeeList({
  employees,
  projectId,
  addedEmployeeIds,
  onAddEmployee,
  onRemoveEmployee
}: EmployeeListProps) {
  if (employees.length === 0) {
    return <div className="panel-empty">Нет работников</div>;
  }

  return (
    <div className="list-stack">
      {employees.map((employee) => (
        <div className="list-row" key={employee.id}>
          <div>
            <span>{formatEmployee(employee)}</span>
            <small>{employee.email}</small>
          </div>
          {projectId && onAddEmployee && !addedEmployeeIds?.has(employee.id) ? (
            <button className="icon-button compact" type="button" title="Добавить" onClick={() => void onAddEmployee(projectId, employee.id)}>
              <Plus size={16} />
            </button>
          ) : null}
          {projectId && onAddEmployee && addedEmployeeIds?.has(employee.id) ? (
            <span className="list-row-status">В проекте</span>
          ) : null}
          {projectId && onRemoveEmployee ? (
            <button className="icon-button compact danger" type="button" title="Убрать" onClick={() => void onRemoveEmployee(projectId, employee.id)}>
              <X size={16} />
            </button>
          ) : null}
        </div>
      ))}
    </div>
  );
}
