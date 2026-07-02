import { useEffect, useState } from "react";
import { CheckCircle2, Download, Edit3, ListChecks, MoreHorizontal, Trash2, Users } from "lucide-react";
import { formatDate, formatEmployee } from "../utils/format";
import type { Project, ProjectTask } from "../types";

type ProjectsTableProps = {
  projects: Project[];
  tasksByProjectId: Record<number, ProjectTask[]>;
  onOpenEmployees: (project: Project) => void;
  onOpenTasks: (project: Project) => void;
  onEditProject: (project: Project) => void;
  onDeleteProject: (project: Project) => void;
  onAssignManager: (project: Project) => void;
  onDownloadProjectDocument: (project: Project) => void;
  onMenuOpenChange: (isOpen: boolean) => void;
  canEditProjects: boolean;
  canAssignManager: boolean;
};

export function ProjectsTable({
  projects,
  tasksByProjectId,
  onOpenEmployees,
  onOpenTasks,
  onEditProject,
  onDeleteProject,
  onAssignManager,
  onDownloadProjectDocument,
  onMenuOpenChange,
  canEditProjects,
  canAssignManager
}: ProjectsTableProps) {
  const [openMenuProjectId, setOpenMenuProjectId] = useState<number | null>(null);

  useEffect(() => {
    onMenuOpenChange(openMenuProjectId !== null);
  }, [onMenuOpenChange, openMenuProjectId]);

  useEffect(() => {
    if (openMenuProjectId === null) {
      return;
    }

    function handlePointerDown(event: PointerEvent) {
      if (event.target instanceof Element && !event.target.closest(".action-menu-wrapper")) {
        setOpenMenuProjectId(null);
      }
    }

    window.addEventListener("pointerdown", handlePointerDown);

    return () => window.removeEventListener("pointerdown", handlePointerDown);
  }, [openMenuProjectId]);

  if (projects.length === 0) {
    return (
      <div className="empty-state">
        <CheckCircle2 aria-hidden="true" size={24} />
        <span>Нет проектов</span>
      </div>
    );
  }

  return (
    <div className="table-scroll">
      <table>
        <thead>
          <tr>
            <th>Название проекта</th>
            <th>Название компании заказчика</th>
            <th>Название компании исполнителя</th>
            <th>Менеджер</th>
            <th>Работники</th>
            <th>Список задач</th>
            <th>Дата начала</th>
            <th>Дата окончания</th>
            <th>Приоритет</th>
            <th>Действия</th>
          </tr>
        </thead>
        <tbody>
          {projects.map((project) => {
            const projectTasks = tasksByProjectId[project.id] ?? [];
            const latestDocument = project.documents?.[0] ?? null;
            const hasMenuActions = canAssignManager || latestDocument !== null;

            return (
              <tr key={project.id}>
                <td>
                  <span className="strong-cell">{project.name}</span>
                </td>
                <td>{project.customerCompanyName}</td>
                <td>{project.contractorCompanyName}</td>
                <td>{formatEmployee(project.projectManager)}</td>
                <td>
                  <button className="text-button" type="button" onClick={() => onOpenEmployees(project)}>
                    <Users size={16} />
                    <span>{project.employees.length}</span>
                  </button>
                </td>
                <td>
                  <button className="text-button" type="button" onClick={() => onOpenTasks(project)}>
                    <ListChecks size={16} />
                    <span>{projectTasks.length}</span>
                  </button>
                </td>
                <td>{formatDate(project.projectStartDate)}</td>
                <td>{formatDate(project.projectEndDate)}</td>
                <td>
                  <span className="priority-pill">{project.projectPriority}</span>
                </td>
                <td>
                  <div className="row-actions">
                    {canEditProjects ? (
                      <>
                        <button className="icon-button compact" type="button" title="Редактировать" onClick={() => onEditProject(project)}>
                          <Edit3 size={16} />
                        </button>
                        <button className="icon-button compact danger" type="button" title="Удалить" onClick={() => onDeleteProject(project)}>
                          <Trash2 size={16} />
                        </button>
                      </>
                    ) : null}
                    {hasMenuActions ? (
                      <div className="action-menu-wrapper">
                        <button
                          className="icon-button compact"
                          type="button"
                          title="Действия"
                          onClick={() =>
                            setOpenMenuProjectId((currentProjectId) => (currentProjectId === project.id ? null : project.id))
                          }
                        >
                          <MoreHorizontal size={16} />
                        </button>
                        {openMenuProjectId === project.id ? (
                          <div className="action-menu">
                            {canAssignManager ? (
                              <button
                                type="button"
                                onClick={() => {
                                  setOpenMenuProjectId(null);
                                  onAssignManager(project);
                                }}
                              >
                                Назначить менеджера
                              </button>
                            ) : null}
                            {latestDocument ? (
                              <button
                                type="button"
                                onClick={() => {
                                  setOpenMenuProjectId(null);
                                  onDownloadProjectDocument(project);
                                }}
                              >
                                <Download size={15} />
                                Скачать файл
                              </button>
                            ) : (
                              <span>Файл не прикреплен</span>
                            )}
                          </div>
                        ) : null}
                      </div>
                    ) : null}
                  </div>
                </td>
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
}
