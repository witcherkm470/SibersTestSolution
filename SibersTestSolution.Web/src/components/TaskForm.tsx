import {useMemo, useState} from "react";
import {Search} from "lucide-react";
import {formatEmployee} from "../utils/format";
import type {Employee, Project, ProjectTask, TaskPayload, TaskUpdatePayload} from "../types";
import {useCurrentUser} from "../context/CurrentUserContext";

type TaskFormProps = {
    task?: ProjectTask;
    projects: Project[];
    employees: Employee[];
    onSubmit: (payload: TaskPayload | TaskUpdatePayload) => Promise<void>;
    onCancel: () => void;
};

type EmployeeSearchSelectProps = {
    label: string;
    placeholder: string;
    employees: Employee[];
    selectedEmployeeId: number | null;
    allowEmpty?: boolean;
    emptyLabel?: string;
    onChange: (employeeId: number | null) => void;
    disabled: boolean;
};

function EmployeeSearchSelect({
                                  label,
                                  placeholder,
                                  employees,
                                  selectedEmployeeId,
                                  allowEmpty = false,
                                  emptyLabel = "Не назначен",
                                  onChange,
                                  disabled = false
                              }: EmployeeSearchSelectProps) {
    const [query, setQuery] = useState("");
    const [isOpen, setIsOpen] = useState(false);
    const selectedEmployee = employees.find((employee) => employee.id === selectedEmployeeId) ?? null;
    const normalizedQuery = query.trim().toLowerCase();
    const selectedLabel = selectedEmployee ? formatEmployee(selectedEmployee) : emptyLabel;
    const shouldShowEmptyOption = allowEmpty && (!normalizedQuery || emptyLabel.toLowerCase().includes(normalizedQuery));

    const filteredEmployees = useMemo(() => {
        if (!normalizedQuery) {
            return employees;
        }

        return employees.filter((employee) =>
            `${formatEmployee(employee)} ${employee.email}`.toLowerCase().includes(normalizedQuery)
        );
    }, [employees, normalizedQuery]);

    function selectEmployee(employeeId: number | null) {
        onChange(employeeId);
        setQuery("");
        setIsOpen(false);
    }

    return (
        <div className="field-group employee-combobox">
            <span className="field-label">{label}</span>
            <div className="employee-combobox-control">
                <Search aria-hidden="true" size={18}/>
                <input
                    disabled={disabled}
                    value={isOpen ? query : selectedLabel}
                    onBlur={() => window.setTimeout(() => setIsOpen(false), 100)}
                    onChange={(event) => {
                        if (disabled) return;
                        setQuery(event.target.value);
                        setIsOpen(true);
                    }}
                    onFocus={() => {
                        if (disabled) return;
                        setQuery("");
                        setIsOpen(true);
                    }}
                    placeholder={placeholder}
                />
            </div>
            {isOpen && !disabled ? (
                <div className="employee-combobox-menu">
                    {shouldShowEmptyOption ? (
                        <button
                            className={selectedEmployeeId === null ? "employee-combobox-option active" : "employee-combobox-option"}
                            type="button"
                            onMouseDown={(event) => event.preventDefault()}
                            onClick={() => selectEmployee(null)}
                        >
                            <span>{emptyLabel}</span>
                        </button>
                    ) : null}
                    {filteredEmployees.map((employee) => (
                        <button
                            className={
                                selectedEmployeeId === employee.id ? "employee-combobox-option active" : "employee-combobox-option"
                            }
                            key={employee.id}
                            type="button"
                            onMouseDown={(event) => event.preventDefault()}
                            onClick={() => selectEmployee(employee.id)}
                        >
                            <span>{formatEmployee(employee)}</span>
                            <small>{employee.email}</small>
                        </button>
                    ))}
                    {filteredEmployees.length === 0 && !shouldShowEmptyOption ? (
                        <div className="employee-combobox-empty">Ничего не найдено</div>
                    ) : null}
                </div>
            ) : null}
        </div>
    );
}

export function TaskForm({task, projects, employees, onSubmit, onCancel}: TaskFormProps) {
    const currentUser = useCurrentUser();
    const firstProjectId = projects[0]?.id ?? 0;
    const currentEmployee = employees.find(
        (employee) => employee.id === currentUser.employeeId
    );

    const isCurrentUserEmployee = currentEmployee !== undefined;

    const defaultOwnerId =
        currentEmployee?.id ??
        employees[0]?.id ??
        0;

    const [form, setForm] = useState<TaskUpdatePayload>({
        name: task?.name ?? "",
        projectId: task?.projectId ?? firstProjectId,
        taskOwnerId: task?.taskOwnerId ?? defaultOwnerId,
        taskPerformerId: task?.taskPerformerId ?? null,
        taskStatus: task?.taskStatus ?? 1,
        comment: task?.comment ?? "",
        taskPriority: task?.taskPriority ?? 1
    });
    const [isSaving, setIsSaving] = useState(false);

    async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
        event.preventDefault();
        setIsSaving(true);

        try {
            if (task) {
                await onSubmit(form);
            } else {
                const {taskStatus: _, ...createPayload} = form;

                await onSubmit(createPayload);
            }
        } finally {
            setIsSaving(false);
        }
    }

    return (
        <form className="entity-form" onSubmit={handleSubmit}>
            <label>
                <span>Название задачи</span>
                <input value={form.name} onChange={(event) => setForm({...form, name: event.target.value})}/>
            </label>
            <label>
                <span>Проект</span>
                <select
                    value={form.projectId}
                    onChange={(event) => setForm({...form, projectId: Number(event.target.value)})}
                >
                    {projects.map((project) => (
                        <option key={project.id} value={project.id}>
                            {project.name}
                        </option>
                    ))}
                </select>
            </label>
            <EmployeeSearchSelect
                label="Автор"
                placeholder="Найти автора"
                employees={employees}
                selectedEmployeeId={form.taskOwnerId}
                onChange={(employeeId) => setForm({...form, taskOwnerId: employeeId ?? 0})}
                disabled={isCurrentUserEmployee}
            />
            <EmployeeSearchSelect
                label="Исполнитель"
                placeholder="Найти исполнителя"
                employees={employees}
                selectedEmployeeId={form.taskPerformerId}
                allowEmpty
                onChange={(employeeId) => setForm({...form, taskPerformerId: employeeId})}
                disabled={false}
            />
            <label>
                <span>Комментарий</span>
                <textarea
                    value={form.comment ?? ""}
                    onChange={(event) => setForm({...form, comment: event.target.value || null})}
                />
            </label>
            <label>
                <span>Приоритет</span>
                <input
                    type="number"
                    min={1}
                    value={form.taskPriority}
                    onChange={(event) => setForm({...form, taskPriority: Number(event.target.value)})}
                />
            </label>
            <div className="form-actions">
                <button className="secondary-button" type="button" onClick={onCancel}>
                    Отмена
                </button>
                <button className="primary-button" type="submit"
                        disabled={isSaving || projects.length === 0 || employees.length === 0}>
                    Сохранить
                </button>
            </div>
        </form>
    );
}
