import { useState } from "react";
import { Plus } from "lucide-react";
import { ProjectsTable } from "../components/ProjectsTable";
import type { Project, ProjectQuery, ProjectTask } from "../types";

type ProjectsPageProps = {
  projects: Project[];
  tasksByProjectId: Record<number, ProjectTask[]>;
  onOpenEmployees: (project: Project) => void;
  onOpenTasks: (project: Project) => void;
  onEditProject: (project: Project) => void;
  onDeleteProject: (project: Project) => void;
  onAssignManager: (project: Project) => void;
  onDownloadProjectDocument: (project: Project) => void;
  onCreateProject: () => void;
  canCreateProject: boolean;
  canEditProjects: boolean;
  canAssignManager: boolean;
  filters: ProjectQuery;
  onFiltersChange: (filters: ProjectQuery) => void;
};

export function ProjectsPage(props: ProjectsPageProps) {
  const [isMenuOpen, setIsMenuOpen] = useState(false);

  return (
    <section className={isMenuOpen ? "table-frame menu-open" : "table-frame"}>
      <div className="section-toolbar filter-toolbar">
        <div className="filter-controls">
          <label>
            <span>Начало от</span>
            <input
              type="date"
              value={props.filters.startDateFrom ?? ""}
              onChange={(event) => props.onFiltersChange({ ...props.filters, startDateFrom: event.target.value })}
            />
          </label>
          <label>
            <span>Начало до</span>
            <input
              type="date"
              value={props.filters.startDateTo ?? ""}
              onChange={(event) => props.onFiltersChange({ ...props.filters, startDateTo: event.target.value })}
            />
          </label>
          <label>
            <span>Приоритет</span>
            <input
              min={1}
              type="number"
              value={props.filters.priority ?? ""}
              onChange={(event) => props.onFiltersChange({ ...props.filters, priority: event.target.value })}
            />
          </label>
          <label>
            <span>Сортировка</span>
            <select
              value={props.filters.sortBy ?? ""}
              onChange={(event) => props.onFiltersChange({ ...props.filters, sortBy: event.target.value })}
            >
              <option value="">По умолчанию</option>
              <option value="name">Название</option>
              <option value="startDate">Дата начала</option>
              <option value="endDate">Дата окончания</option>
              <option value="priority">Приоритет</option>
            </select>
          </label>
          <label>
            <span>Порядок</span>
            <select
              value={props.filters.sortDirection ?? "asc"}
              onChange={(event) => props.onFiltersChange({ ...props.filters, sortDirection: event.target.value })}
            >
              <option value="asc">По возрастанию</option>
              <option value="desc">По убыванию</option>
            </select>
          </label>
        </div>
        {props.canCreateProject ? (
          <button className="primary-button" type="button" onClick={props.onCreateProject}>
            <Plus size={16} />
            <span>Добавить</span>
          </button>
        ) : null}
      </div>
      <ProjectsTable {...props} onMenuOpenChange={setIsMenuOpen} />
    </section>
  );
}
