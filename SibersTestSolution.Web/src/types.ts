export type ViewName = "projects" | "employees" | "tasks";

export type UserRole = "Head" | "ProjectManager" | "Employee";

export type CurrentUser = {
  id: string;
  userName: string;
  email: string | null;
  employeeId: number | null;
  roles: UserRole[];
};

export type LoginPayload = {
  userName: string;
  password: string;
};

export type Employee = {
  id: number;
  name: string;
  lastName: string;
  middleName: string;
  email: string;
  isDeleted?: boolean;
  userName?: string | null;
  roles?: UserRole[];
};

export type EmployeePayload = {
  name: string;
  lastName: string;
  middleName: string;
  email: string;
  userName?: string | null;
  password?: string | null;
  role?: UserRole | null;
};

export type Project = {
  id: number;
  name: string;
  customerCompanyName: string;
  contractorCompanyName: string;
  employees: Employee[];
  projectManagerId: number | null;
  projectManager: Employee | null;
  projectStartDate: string;
  projectEndDate: string;
  projectPriority: number;
  documents: ProjectDocument[];
};

export type ProjectDocument = {
  id: number;
  originalFileName: string;
  contentType: string;
  sizeInBytes: number;
  uploadedAtUtc: string;
};

export type ProjectPayload = {
  name: string;
  customerCompanyName: string;
  contractorCompanyName: string;
  projectStartDate: string;
  projectEndDate: string;
  projectPriority: number;
  projectManagerId?: number | null;
  employeeIds?: number[];
};

export type TaskProject = {
  id: number;
  name: string;
  projectManagerId: number | null;
};

export type ProjectTask = {
  id: number;
  name: string;
  projectId: number;
  project: TaskProject | null;
  taskOwnerId: number;
  taskOwner: Employee | null;
  taskPerformerId: number | null;
  taskPerformer: Employee | null;
  taskStatus: number;
  comment: string | null;
  taskPriority: number;
};

export type TaskPayload = {
  name: string;
  projectId: number;
  taskOwnerId: number;
  taskPerformerId: number | null;
  comment: string | null;
  taskPriority: number;
};

export type TaskUpdatePayload = TaskPayload & {
  taskStatus: number;
};

export type PanelState =
  | { kind: "employees"; project: Project }
  | { kind: "tasks"; project: Project };

export type ProjectQuery = {
  search?: string;
  startDateFrom?: string;
  startDateTo?: string;
  priority?: string;
  sortBy?: string;
  sortDirection?: string;
};

export type EmployeeQuery = {
  search?: string;
  sortBy?: string;
  sortDirection?: string;
};

export type TaskQuery = {
  search?: string;
  status?: string;
  projectId?: string;
  performerId?: string;
  ownerId?: string;
  sortBy?: string;
  sortDirection?: string;
};
