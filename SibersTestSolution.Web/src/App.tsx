import { useEffect, useMemo, useState } from "react";
import { Loader2 } from "lucide-react";
import {
  addProjectEmployee,
  assignProjectManager,
  changeTaskStatus,
  createEmployee,
  createProject,
  createTask,
  deleteEmployee,
  deleteProject,
  deleteTask,
  downloadProjectDocument,
  getEmployees,
  getCurrentUser,
  getProjects,
  getTasks,
  login,
  logout,
  removeProjectEmployee,
  uploadProjectDocuments,
  updateEmployee,
  updateProject,
  updateTask
} from "./api";
import { AssignManagerForm } from "./components/AssignManagerForm";
import { DetailsPanel } from "./components/DetailsPanel";
import { EmployeeForm } from "./components/EmployeeForm";
import { LoginPage } from "./components/LoginPage";
import { Modal } from "./components/Modal";
import { ProjectForm } from "./components/ProjectForm";
import { ProjectWizard } from "./components/ProjectWizard";
import { SidebarNavigation } from "./components/SidebarNavigation";
import { StateBanner } from "./components/StateBanner";
import { TaskForm } from "./components/TaskForm";
import { Topbar } from "./components/Topbar";
import { EmployeesPage } from "./pages/EmployeesPage";
import { ProjectsPage } from "./pages/ProjectsPage";
import { TasksPage } from "./pages/TasksPage";
import { formatEmployee } from "./utils/format";
import { CurrentUserProvider } from "./context/CurrentUserContext";
import type {
  Employee,
  EmployeeQuery,
  EmployeePayload,
  CurrentUser,
  LoginPayload,
  PanelState,
  Project,
  ProjectPayload,
  ProjectQuery,
  ProjectTask,
  TaskPayload,
  TaskQuery,
  TaskUpdatePayload,
  ViewName
} from "./types";

type ModalState =
  | { kind: "create-project" }
  | { kind: "edit-project"; project: Project }
  | { kind: "assign-manager"; project: Project }
  | { kind: "create-employee" }
  | { kind: "edit-employee"; employee: Employee }
  | { kind: "create-task" }
  | { kind: "edit-task"; task: ProjectTask };

const viewTitles: Record<ViewName, string> = {
  projects: "Проекты",
  employees: "Работники",
  tasks: "Задачи"
};

export function App() {
  const [activeView, setActiveView] = useState<ViewName>("projects");
  const [isSidebarHidden, setIsSidebarHidden] = useState(false);
  const [currentUser, setCurrentUser] = useState<CurrentUser | null>(null);
  const [isAuthLoading, setIsAuthLoading] = useState(true);
  const [projects, setProjects] = useState<Project[]>([]);
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [tasks, setTasks] = useState<ProjectTask[]>([]);
  const [selectedPanel, setSelectedPanel] = useState<PanelState | null>(null);
  const [modal, setModal] = useState<ModalState | null>(null);
  const [search, setSearch] = useState("");
  const [projectFilters, setProjectFilters] = useState<ProjectQuery>({ sortDirection: "asc" });
  const [employeeFilters, setEmployeeFilters] = useState<EmployeeQuery>({ sortDirection: "asc" });
  const [taskFilters, setTaskFilters] = useState<TaskQuery>({ sortDirection: "asc" });
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const isHead = currentUser?.roles.includes("Head") ?? false;
  const isProjectManager = currentUser?.roles.includes("ProjectManager") ?? false;
  const canManageEmployees = isHead;
  const canManageProjects = isHead;
  const canManageTasks = isHead || isProjectManager;
  const availableViews: ViewName[] = isHead ? ["projects", "employees", "tasks"] : ["projects", "tasks"];

  const tasksByProjectId = useMemo(() => {
    return tasks.reduce<Record<number, ProjectTask[]>>((accumulator, task) => {
      accumulator[task.projectId] = accumulator[task.projectId] ?? [];
      accumulator[task.projectId].push(task);
      return accumulator;
    }, {});
  }, [tasks]);

  const visibleProjects = useMemo(() => {
    const query = search.trim().toLowerCase();
    let result = projects;

    if (query) {
      result = result.filter((project) => {
      const manager = formatEmployee(project.projectManager).toLowerCase();
      const searchable = [
        project.name,
        project.customerCompanyName,
        project.contractorCompanyName,
        manager
      ].join(" ");

      return searchable.toLowerCase().includes(query);
    });
    }

    if (projectFilters.startDateFrom) {
      result = result.filter((project) => project.projectStartDate.slice(0, 10) >= projectFilters.startDateFrom!);
    }

    if (projectFilters.startDateTo) {
      result = result.filter((project) => project.projectStartDate.slice(0, 10) <= projectFilters.startDateTo!);
    }

    if (projectFilters.priority) {
      result = result.filter((project) => project.projectPriority === Number(projectFilters.priority));
    }

    result = [...result].sort((left, right) => {
      const direction = projectFilters.sortDirection === "desc" ? -1 : 1;

      switch (projectFilters.sortBy) {
        case "name":
          return left.name.localeCompare(right.name) * direction;
        case "startDate":
          return left.projectStartDate.localeCompare(right.projectStartDate) * direction;
        case "endDate":
          return left.projectEndDate.localeCompare(right.projectEndDate) * direction;
        case "priority":
          return (left.projectPriority - right.projectPriority) * direction;
        default:
          return left.id - right.id;
      }
    });

    return result;
  }, [projectFilters, projects, search]);

  const visibleEmployees = useMemo(() => {
    const queries = [search, employeeFilters.search]
      .map((value) => value?.trim().toLowerCase())
      .filter((value): value is string => Boolean(value));
    let result = employees;

    if (queries.length > 0) {
      result = result.filter((employee) => {
        const searchable = `${formatEmployee(employee)} ${employee.email}`.toLowerCase();

        return queries.every((query) => searchable.includes(query));
      });
    }

    result = [...result].sort((left, right) => {
      const direction = employeeFilters.sortDirection === "desc" ? -1 : 1;

      switch (employeeFilters.sortBy) {
        case "name":
          return left.name.localeCompare(right.name) * direction;
        case "lastName":
          return left.lastName.localeCompare(right.lastName) * direction;
        case "middleName":
          return left.middleName.localeCompare(right.middleName) * direction;
        case "email":
          return left.email.localeCompare(right.email) * direction;
        default:
          return left.id - right.id;
      }
    });

    return result;
  }, [employeeFilters, employees, search]);

  const visibleTasks = useMemo(() => {
    const query = search.trim().toLowerCase();
    let result = tasks;

    if (query) {
      result = result.filter((task) =>
      [
        task.name,
        task.project?.name,
        formatEmployee(task.taskOwner),
        formatEmployee(task.taskPerformer)
      ]
        .join(" ")
        .toLowerCase()
        .includes(query)
    );
    }

    if (taskFilters.status) {
      result = result.filter((task) => task.taskStatus === Number(taskFilters.status));
    }

    if (taskFilters.projectId) {
      result = result.filter((task) => task.projectId === Number(taskFilters.projectId));
    }

    result = [...result].sort((left, right) => {
      const direction = taskFilters.sortDirection === "desc" ? -1 : 1;

      switch (taskFilters.sortBy) {
        case "name":
          return left.name.localeCompare(right.name) * direction;
        case "project":
          return (left.project?.name ?? "").localeCompare(right.project?.name ?? "") * direction;
        case "status":
          return (left.taskStatus - right.taskStatus) * direction;
        case "priority":
          return (left.taskPriority - right.taskPriority) * direction;
        default:
          return left.id - right.id;
      }
    });

    return result;
  }, [search, taskFilters, tasks]);

  async function loadData(signal?: AbortSignal) {
    if (!currentUser) {
      return;
    }

    setIsLoading(true);
    setError(null);

    try {
      const [projectsResponse, employeesResponse, tasksResponse] = await Promise.all([
        getProjects(undefined, signal),
        canManageEmployees || isProjectManager ? getEmployees(undefined, signal) : Promise.resolve([]),
        getTasks(undefined, signal)
      ]);

      setProjects(projectsResponse);
      setEmployees(employeesResponse);
      setTasks(tasksResponse);
    } catch (loadError) {
      if (loadError instanceof DOMException && loadError.name === "AbortError") {
        return;
      }

      setError(loadError instanceof Error ? loadError.message : "Не удалось загрузить данные");
    } finally {
      setIsLoading(false);
    }
  }

  async function runAction(action: () => Promise<void>) {
    setError(null);

    try {
      await action();
    } catch (actionError) {
      setError(actionError instanceof Error ? actionError.message : "Операция не выполнена");
    }
  }

  function replaceProject(project: Project) {
    setProjects((currentProjects) =>
      currentProjects.map((currentProject) => (currentProject.id === project.id ? project : currentProject))
    );
    setSelectedPanel((currentPanel) => {
      if (!currentPanel || currentPanel.project.id !== project.id) {
        return currentPanel;
      }

      return { ...currentPanel, project };
    });
  }

  function replaceTask(task: ProjectTask) {
    setTasks((currentTasks) =>
      currentTasks.map((currentTask) => (currentTask.id === task.id ? task : currentTask))
    );
  }

  async function handleLogin(payload: LoginPayload) {
    const user = await login(payload);
    setCurrentUser(user);
    setActiveView("projects");

    return user;
  }

  async function handleLogout() {
    await logout();
    setCurrentUser(null);
    setProjects([]);
    setEmployees([]);
    setTasks([]);
    setSelectedPanel(null);
    setModal(null);
  }

  function canManageSelectedProjectEmployees(panel: PanelState | null) {
    if (!panel || panel.kind !== "employees") {
      return false;
    }

    return isHead || (isProjectManager && panel.project.projectManagerId === currentUser?.employeeId);
  }

  useEffect(() => {
    const controller = new AbortController();

    getCurrentUser(controller.signal)
      .then(setCurrentUser)
      .catch(() => setCurrentUser(null))
      .finally(() => setIsAuthLoading(false));

    return () => controller.abort();
  }, []);

  useEffect(() => {
    if (!currentUser) {
      setIsLoading(false);
      return;
    }

    const controller = new AbortController();

    void loadData(controller.signal);

    return () => controller.abort();
  }, [currentUser]);

  useEffect(() => {
    if (currentUser && !availableViews.includes(activeView)) {
      setActiveView("projects");
    }
  }, [activeView, availableViews, currentUser]);

  if (isAuthLoading) {
    return (
      <main className="login-shell">
        <div className="loading-state">
          <Loader2 className="spin" size={28} />
        </div>
      </main>
    );
  }

  if (!currentUser) {
    return <LoginPage onLogin={handleLogin} />;
  }

  return (
      <CurrentUserProvider currentUser={currentUser}>
        <main className={isSidebarHidden ? "app-shell navigation-hidden" : "app-shell"}>
          {isSidebarHidden ? null : (
            <SidebarNavigation
              activeView={activeView}
              views={availableViews}
              onChange={(view) => {
                setActiveView(view);
                setSearch("");
                setSelectedPanel(null);
              }}
            />
          )}

          <section className="workspace">
            <Topbar
              title={viewTitles[activeView]}
              isSidebarHidden={isSidebarHidden}
              onToggleSidebar={() => setIsSidebarHidden((currentValue) => !currentValue)}
              search={search}
              onSearchChange={setSearch}
              onRefresh={() => void loadData()}
              userName={currentUser.userName}
              onLogout={() => void handleLogout()}
            />

            {error ? <StateBanner message={error} /> : null}

            {isLoading ? (
              <section className="table-frame">
                <div className="loading-state">
                  <Loader2 className="spin" size={28} />
                </div>
              </section>
            ) : null}

            {!isLoading && activeView === "projects" ? (
              <ProjectsPage
                projects={visibleProjects}
                tasksByProjectId={tasksByProjectId}
                onOpenEmployees={(project) => setSelectedPanel({ kind: "employees", project })}
                onOpenTasks={(project) => setSelectedPanel({ kind: "tasks", project })}
                onCreateProject={() => setModal({ kind: "create-project" })}
                onEditProject={(project) => setModal({ kind: "edit-project", project })}
                onAssignManager={(project) => setModal({ kind: "assign-manager", project })}
                onDownloadProjectDocument={(project) =>
                  void runAction(async () => {
                    const document = project.documents[0];

                    if (!document) {
                      throw new Error("Файл не прикреплен.");
                    }

                    await downloadProjectDocument(project.id, document.id, document.originalFileName);
                  })
                }
                canCreateProject={canManageProjects}
                canEditProjects={canManageProjects}
                canAssignManager={canManageProjects}
                filters={projectFilters}
                onFiltersChange={setProjectFilters}
                onDeleteProject={(project) =>
                  void runAction(async () => {
                    if (!window.confirm(`Удалить проект "${project.name}"?`)) {
                      return;
                    }

                    await deleteProject(project.id);
                    setProjects((currentProjects) => currentProjects.filter((item) => item.id !== project.id));
                    setSelectedPanel((currentPanel) =>
                      currentPanel?.project.id === project.id ? null : currentPanel
                    );
                  })
                }
              />
            ) : null}

            {!isLoading && activeView === "employees" ? (
              <EmployeesPage
                employees={visibleEmployees}
                filters={employeeFilters}
                onFiltersChange={setEmployeeFilters}
                onCreateEmployee={() => setModal({ kind: "create-employee" })}
                onEditEmployee={(employee) => setModal({ kind: "edit-employee", employee })}
                onDeleteEmployee={(employee) =>
                  void runAction(async () => {
                    if (!window.confirm(`Удалить работника "${formatEmployee(employee)}"?`)) {
                      return;
                    }

                    await deleteEmployee(employee.id);
                    await loadData();
                  })
                }
              />
            ) : null}

            {!isLoading && activeView === "tasks" ? (
              <TasksPage
                tasks={visibleTasks}
                projects={projects}
                onCreateTask={() => setModal({ kind: "create-task" })}
                onEditTask={(task) => setModal({ kind: "edit-task", task })}
                canCreateTask={canManageTasks}
                canEditTasks={canManageTasks}
                canDeleteTasks={canManageTasks}
                filters={taskFilters}
                onFiltersChange={setTaskFilters}
                onDeleteTask={(task) =>
                  void runAction(async () => {
                    if (!window.confirm(`Удалить задачу "${task.name}"?`)) {
                      return;
                    }

                    await deleteTask(task.id);
                    await loadData();
                  })
                }
                onChangeTaskStatus={(task, taskStatus) =>
                  void runAction(async () => {
                    const updatedTask = await changeTaskStatus(task.id, taskStatus);
                    replaceTask(updatedTask);
                  })
                }
              />
            ) : null}
          </section>

          <DetailsPanel
            panel={selectedPanel}
            employees={employees}
            tasks={selectedPanel?.kind === "tasks" ? tasksByProjectId[selectedPanel.project.id] ?? [] : []}
            onClose={() => setSelectedPanel(null)}
            canManageProjectEmployees={canManageSelectedProjectEmployees(selectedPanel)}
            onAddEmployee={(projectId, employeeId) =>
              runAction(async () => {
                const project = await addProjectEmployee(projectId, employeeId);
                replaceProject(project);
              })
            }
            onRemoveEmployee={(projectId, employeeId) =>
              runAction(async () => {
                const project = await removeProjectEmployee(projectId, employeeId);
                replaceProject(project);
              })
            }
          />

          {modal ? renderModal(modal) : null}
        </main>
      </CurrentUserProvider>
  );

  function renderModal(currentModal: ModalState) {
    switch (currentModal.kind) {
      case "create-project":
        return (
          <Modal title="Добавить проект" onClose={() => setModal(null)}>
            <ProjectWizard
              onCancel={() => setModal(null)}
              onSubmit={(payload: ProjectPayload, files: File[]) =>
                runAction(async () => {
                  const project = await createProject(payload);
                  if (files.length > 0) {
                    await uploadProjectDocuments(project.id, files);
                    await loadData();
                  } else {
                    setProjects((currentProjects) => [...currentProjects, project]);
                  }
                  setModal(null);
                })
              }
            />
          </Modal>
        );
      case "edit-project":
        return (
          <Modal title="Редактировать проект" onClose={() => setModal(null)}>
            <ProjectForm
              project={currentModal.project}
              onCancel={() => setModal(null)}
              onSubmit={(payload: ProjectPayload, files: File[]) =>
                runAction(async () => {
                  const project = await updateProject(currentModal.project.id, payload);
                  if (files.length > 0) {
                    await uploadProjectDocuments(project.id, files);
                    await loadData();
                  } else {
                    replaceProject(project);
                  }
                  setModal(null);
                })
              }
            />
          </Modal>
        );
      case "assign-manager":
        return (
          <Modal title="Назначить менеджера" onClose={() => setModal(null)}>
            <AssignManagerForm
              project={currentModal.project}
              employees={employees}
              onCancel={() => setModal(null)}
              onSubmit={(employeeId) =>
                runAction(async () => {
                  const project = await assignProjectManager(currentModal.project.id, employeeId);
                  replaceProject(project);
                  setModal(null);
                })
              }
            />
          </Modal>
        );
      case "create-employee":
        return (
          <Modal title="Добавить работника" onClose={() => setModal(null)}>
            <EmployeeForm
              onCancel={() => setModal(null)}
              onSubmit={(payload: EmployeePayload) =>
                runAction(async () => {
                  await createEmployee(payload);
                  await loadData();
                  setModal(null);
                })
              }
            />
          </Modal>
        );
      case "edit-employee":
        return (
          <Modal title="Редактировать работника" onClose={() => setModal(null)}>
            <EmployeeForm
              employee={currentModal.employee}
              onCancel={() => setModal(null)}
              onSubmit={(payload: EmployeePayload) =>
                runAction(async () => {
                  await updateEmployee(currentModal.employee.id, payload);
                  await loadData();
                  setModal(null);
                })
              }
            />
          </Modal>
        );
      case "create-task":
        return (
          <Modal title="Добавить задачу" onClose={() => setModal(null)}>
            <TaskForm
              projects={projects}
              employees={employees}
              onCancel={() => setModal(null)}
              onSubmit={(payload) =>
                runAction(async () => {
                  await createTask(payload as TaskPayload);
                  await loadData();
                  setModal(null);
                })
              }
            />
          </Modal>
        );
      case "edit-task":
        return (
          <Modal title="Редактировать задачу" onClose={() => setModal(null)}>
            <TaskForm
              task={currentModal.task}
              projects={projects}
              employees={employees}
              onCancel={() => setModal(null)}
              onSubmit={(payload) =>
                runAction(async () => {
                  await updateTask(currentModal.task.id, payload as TaskUpdatePayload);
                  await loadData();
                  setModal(null);
                })
              }
            />
          </Modal>
        );
    }
  }
}
