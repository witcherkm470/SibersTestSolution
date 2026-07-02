import { Edit3, Plus, Search, Trash2 } from "lucide-react";
import { formatEmployee } from "../utils/format";
import type { Employee, EmployeeQuery } from "../types";

type EmployeesPageProps = {
  employees: Employee[];
  filters: EmployeeQuery;
  onFiltersChange: (filters: EmployeeQuery) => void;
  onCreateEmployee: () => void;
  onEditEmployee: (employee: Employee) => void;
  onDeleteEmployee: (employee: Employee) => void;
};

export function EmployeesPage({
  employees,
  filters,
  onFiltersChange,
  onCreateEmployee,
  onEditEmployee,
  onDeleteEmployee
}: EmployeesPageProps) {
  return (
    <section className="table-frame">
      <div className="section-toolbar filter-toolbar">
        <div className="filter-controls">
          <label className="search-field employee-filter-search">
            <Search aria-hidden="true" size={18} />
            <input
              value={filters.search ?? ""}
              onChange={(event) => onFiltersChange({ ...filters, search: event.target.value })}
              placeholder="Найти работника"
            />
          </label>
          <label>
            <span>Сортировка</span>
            <select
              value={filters.sortBy ?? ""}
              onChange={(event) => onFiltersChange({ ...filters, sortBy: event.target.value })}
            >
              <option value="">По умолчанию</option>
              <option value="lastName">Фамилия</option>
              <option value="name">Имя</option>
              <option value="middleName">Отчество</option>
              <option value="email">Email</option>
            </select>
          </label>
          <label>
            <span>Порядок</span>
            <select
              value={filters.sortDirection ?? "asc"}
              onChange={(event) => onFiltersChange({ ...filters, sortDirection: event.target.value })}
            >
              <option value="asc">По возрастанию</option>
              <option value="desc">По убыванию</option>
            </select>
          </label>
        </div>
        <button className="primary-button" type="button" onClick={onCreateEmployee}>
          <Plus size={16} />
          <span>Добавить</span>
        </button>
      </div>
      <div className="table-scroll">
        <table>
          <thead>
            <tr>
              <th>ФИО</th>
              <th>Email</th>
              <th>Действия</th>
            </tr>
          </thead>
          <tbody>
            {employees.map((employee) => (
              <tr key={employee.id}>
                <td>
                  <span className="strong-cell">{formatEmployee(employee)}</span>
                </td>
                <td>{employee.email}</td>
                <td>
                  <div className="row-actions">
                    <button className="icon-button compact" type="button" title="Редактировать" onClick={() => onEditEmployee(employee)}>
                      <Edit3 size={16} />
                    </button>
                    <button className="icon-button compact danger" type="button" title="Удалить" onClick={() => onDeleteEmployee(employee)}>
                      <Trash2 size={16} />
                    </button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </section>
  );
}
