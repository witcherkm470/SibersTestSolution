import { Edit3, MoreHorizontal, Plus, Trash2 } from "lucide-react";
import { useEffect, useState } from "react";
import { formatEmployee, taskStatuses } from "../utils/format";
import type { Project, ProjectTask, TaskQuery } from "../types";

type TasksPageProps = {
  tasks: ProjectTask[];
  projects: Project[];
  onCreateTask: () => void;
  onEditTask: (task: ProjectTask) => void;
  onDeleteTask: (task: ProjectTask) => void;
  onChangeTaskStatus: (task: ProjectTask, taskStatus: number) => void;
  canCreateTask: boolean;
  canEditTasks: boolean;
  canDeleteTasks: boolean;
  filters: TaskQuery;
  onFiltersChange: (filters: TaskQuery) => void;
};

type TaskStatusAction = {
  status: number;
  label: string;
};

function getStatusActions(taskStatus: number): TaskStatusAction[] {
  if (taskStatus === 1) {
    return [{ status: 2, label: "Взять в работу" }];
  }

  if (taskStatus === 2) {
    return [
      { status: 1, label: "Вернуть в Todo" },
      { status: 3, label: "Завершить" }
    ];
  }

  return [];
}

export function TasksPage({
  tasks,
  projects,
  onCreateTask,
  onEditTask,
  onDeleteTask,
  onChangeTaskStatus,
  canCreateTask,
  canEditTasks,
  canDeleteTasks,
  filters,
  onFiltersChange
}: TasksPageProps) {
  const [openMenuTaskId, setOpenMenuTaskId] = useState<number | null>(null);

  useEffect(() => {
    if (openMenuTaskId === null) {
      return;
    }

    function handlePointerDown(event: PointerEvent) {
      if (event.target instanceof Element && !event.target.closest(".action-menu-wrapper")) {
        setOpenMenuTaskId(null);
      }
    }

    window.addEventListener("pointerdown", handlePointerDown);

    return () => window.removeEventListener("pointerdown", handlePointerDown);
  }, [openMenuTaskId]);

  return (
    <section className={openMenuTaskId === null ? "table-frame" : "table-frame menu-open"}>
      <div className="section-toolbar filter-toolbar">
        <div className="filter-controls">
          <label>
            <span>Статус</span>
            <select
              value={filters.status ?? ""}
              onChange={(event) => onFiltersChange({ ...filters, status: event.target.value })}
            >
              <option value="">Все</option>
              <option value="1">Todo</option>
              <option value="2">In progress</option>
              <option value="3">Done</option>
            </select>
          </label>
          <label>
            <span>Проект</span>
            <select
              value={filters.projectId ?? ""}
              onChange={(event) => onFiltersChange({ ...filters, projectId: event.target.value })}
            >
              <option value="">Все</option>
              {projects.map((project) => (
                <option key={project.id} value={project.id}>
                  {project.name}
                </option>
              ))}
            </select>
          </label>
          <label>
            <span>Сортировка</span>
            <select
              value={filters.sortBy ?? ""}
              onChange={(event) => onFiltersChange({ ...filters, sortBy: event.target.value })}
            >
              <option value="">По умолчанию</option>
              <option value="name">Название</option>
              <option value="project">Проект</option>
              <option value="status">Статус</option>
              <option value="priority">Приоритет</option>
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
        {canCreateTask ? (
          <button className="primary-button" type="button" onClick={onCreateTask}>
            <Plus size={16} />
            <span>Добавить</span>
          </button>
        ) : null}
      </div>
      <div className="table-scroll">
        <table>
          <thead>
            <tr>
              <th>Название</th>
              <th>Проект</th>
              <th>Автор</th>
              <th>Исполнитель</th>
              <th>Статус</th>
              <th>Приоритет</th>
              <th>Действия</th>
            </tr>
          </thead>
          <tbody>
            {tasks.map((task) => (
              <tr key={task.id}>
                <td>
                  <span className="strong-cell">{task.name}</span>
                </td>
                <td>{task.project?.name ?? task.projectId}</td>
                <td>{formatEmployee(task.taskOwner)}</td>
                <td>{formatEmployee(task.taskPerformer)}</td>
                <td>{taskStatuses[task.taskStatus] ?? "Unknown"}</td>
                <td>
                  <span className="priority-pill">{task.taskPriority}</span>
                </td>
                <td>
                  <div className="row-actions">
                    {canEditTasks ? (
                      <button className="icon-button compact" type="button" title="Редактировать" onClick={() => onEditTask(task)}>
                        <Edit3 size={16} />
                      </button>
                    ) : null}
                    {canDeleteTasks ? (
                      <button className="icon-button compact danger" type="button" title="Удалить" onClick={() => onDeleteTask(task)}>
                        <Trash2 size={16} />
                      </button>
                    ) : null}
                    <div className="action-menu-wrapper">
                      <button
                        className="icon-button compact"
                        type="button"
                        title="Сменить статус"
                        onClick={() =>
                          setOpenMenuTaskId((currentTaskId) => (currentTaskId === task.id ? null : task.id))
                        }
                      >
                        <MoreHorizontal size={16} />
                      </button>
                      {openMenuTaskId === task.id ? (
                        <div className="action-menu">
                          {getStatusActions(task.taskStatus).map((action) => (
                            <button
                              key={action.status}
                              type="button"
                              onClick={() => {
                                setOpenMenuTaskId(null);
                                onChangeTaskStatus(task, action.status);
                              }}
                            >
                              {action.label}
                            </button>
                          ))}
                          {getStatusActions(task.taskStatus).length === 0 ? (
                            <span>Статус нельзя изменить</span>
                          ) : null}
                        </div>
                      ) : null}
                    </div>
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
