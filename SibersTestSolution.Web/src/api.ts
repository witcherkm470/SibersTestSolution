import type {
  CurrentUser,
  Employee,
  EmployeeQuery,
  EmployeePayload,
  LoginPayload,
  Project,
  ProjectDocument,
  ProjectPayload,
  ProjectQuery,
  ProjectTask,
  TaskPayload,
  TaskQuery,
  TaskUpdatePayload
} from "./types";

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? "";

type RequestOptions = {
  method?: string;
  body?: unknown;
  signal?: AbortSignal;
};

type ApiErrorResponse = {
  title?: string;
  detail?: string;
  message?: string;
  errors?: Record<string, string | string[]>;
};

function getFallbackErrorMessage(status: number): string {
  if (status >= 500) {
    return "На сервере произошла ошибка.";
  }

  if (status === 404) {
    return "Запись не найдена.";
  }

  if (status === 400) {
    return "Проверь данные в форме.";
  }

  return "Операция не выполнена.";
}

function stripHtmlTags(value: string): string {
  return value
    .replace(/<script[\s\S]*?<\/script>/gi, "")
    .replace(/<style[\s\S]*?<\/style>/gi, "")
    .replace(/<[^>]+>/g, " ")
    .replace(/\s+/g, " ")
    .trim();
}

function getHtmlTitle(value: string): string | null {
  const titleMatch = value.match(/<title>([\s\S]*?)<\/title>/i);

  return titleMatch ? stripHtmlTags(titleMatch[1]) : null;
}

function limitErrorMessage(value: string): string {
  return value.length > 500 ? `${value.slice(0, 500).trim()}...` : value;
}

function formatValidationErrors(errors: ApiErrorResponse["errors"]): string | null {
  if (!errors) {
    return null;
  }

  const messages = Object.entries(errors).flatMap(([field, fieldErrors]) => {
    const values = Array.isArray(fieldErrors) ? fieldErrors : [fieldErrors];

    return values.map((message) => (field ? `${field}: ${message}` : message));
  });

  return messages.length > 0 ? messages.join(" ") : null;
}

function getErrorMessageFromJson(error: ApiErrorResponse): string | null {
  return formatValidationErrors(error.errors) ?? error.detail ?? error.message ?? error.title ?? null;
}

async function getErrorMessage(response: Response): Promise<string> {
  const fallbackMessage = getFallbackErrorMessage(response.status);
  const responseText = await response.text();

  if (!responseText) {
    return fallbackMessage;
  }

  try {
    const parsedError = JSON.parse(responseText) as ApiErrorResponse;

    return getErrorMessageFromJson(parsedError) ?? fallbackMessage;
  } catch {
    const textError = responseText.trim().startsWith("<")
      ? getHtmlTitle(responseText) ?? stripHtmlTags(responseText)
      : responseText.trim();

    return textError ? limitErrorMessage(textError) : fallbackMessage;
  }
}

async function request<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const response = await fetch(`${apiBaseUrl}${path}`, {
    method: options.method ?? "GET",
    headers: {
      Accept: "application/json",
      ...(options.body === undefined || options.body instanceof FormData ? {} : { "Content-Type": "application/json" })
    },
    body: options.body === undefined
      ? undefined
      : options.body instanceof FormData
        ? options.body
        : JSON.stringify(options.body),
    credentials: "include",
    signal: options.signal
  });

  if (!response.ok) {
    throw new Error(await getErrorMessage(response));
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return response.json() as Promise<T>;
}

function toQueryString(query?: Record<string, string | number | undefined | null>): string {
  if (!query) {
    return "";
  }

  const searchParams = new URLSearchParams();

  Object.entries(query).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== "") {
      searchParams.set(key, String(value));
    }
  });

  const queryString = searchParams.toString();

  return queryString ? `?${queryString}` : "";
}

export function login(payload: LoginPayload): Promise<CurrentUser> {
  return request<CurrentUser>("/api/auth/login", {
    method: "POST",
    body: payload
  });
}

export function logout(): Promise<void> {
  return request<void>("/api/auth/logout", {
    method: "POST"
  });
}

export function getCurrentUser(signal?: AbortSignal): Promise<CurrentUser> {
  return request<CurrentUser>("/api/auth/me", { signal });
}

export function getProjects(query?: ProjectQuery, signal?: AbortSignal): Promise<Project[]> {
  return request<Project[]>(`/api/projects${toQueryString(query)}`, { signal });
}

export function createProject(payload: ProjectPayload): Promise<Project> {
  return request<Project>("/api/projects", {
    method: "POST",
    body: {
      ...payload,
      projectManagerId: payload.projectManagerId ?? null,
      employeeIds: payload.employeeIds ?? []
    }
  });
}

export function updateProject(id: number, payload: ProjectPayload): Promise<Project> {
  return request<Project>(`/api/projects/${id}`, {
    method: "PUT",
    body: payload
  });
}

export function deleteProject(id: number): Promise<void> {
  return request<void>(`/api/projects/${id}`, {
    method: "DELETE"
  });
}

export function assignProjectManager(projectId: number, employeeId: number): Promise<Project> {
  return request<Project>(`/api/projects/${projectId}/manager`, {
    method: "PATCH",
    body: { employeeId }
  });
}

export function addProjectEmployee(projectId: number, employeeId: number): Promise<Project> {
  return request<Project>(`/api/projects/${projectId}/employees`, {
    method: "POST",
    body: { employeeId }
  });
}

export function removeProjectEmployee(projectId: number, employeeId: number): Promise<Project> {
  return request<Project>(`/api/projects/${projectId}/employees/${employeeId}`, {
    method: "DELETE"
  });
}

function getFileNameFromContentDisposition(value: string | null): string | null {
  if (!value) {
    return null;
  }

  const encodedFileName = value.match(/filename\*=UTF-8''([^;]+)/i);

  if (encodedFileName?.[1]) {
    return decodeURIComponent(encodedFileName[1]);
  }

  const quotedFileName = value.match(/filename="?([^";]+)"?/i);

  return quotedFileName?.[1] ?? null;
}

export function uploadProjectDocuments(projectId: number, files: File[]): Promise<ProjectDocument[]> {
  const formData = new FormData();

  files.forEach((file) => formData.append("files", file));

  return request<ProjectDocument[]>(`/api/projects/${projectId}/documents`, {
    method: "POST",
    body: formData
  });
}

export async function downloadProjectDocument(
  projectId: number,
  documentId: number,
  fallbackFileName: string
): Promise<void> {
  const response = await fetch(`${apiBaseUrl}/api/projects/${projectId}/documents/${documentId}/download`, {
    credentials: "include"
  });

  if (!response.ok) {
    throw new Error(await getErrorMessage(response));
  }

  const blob = await response.blob();
  const url = window.URL.createObjectURL(blob);
  const link = document.createElement("a");

  link.href = url;
  link.download = getFileNameFromContentDisposition(response.headers.get("Content-Disposition")) ?? fallbackFileName;
  document.body.append(link);
  link.click();
  link.remove();
  window.URL.revokeObjectURL(url);
}

export function getEmployees(query?: string | EmployeeQuery, signal?: AbortSignal): Promise<Employee[]> {
  const normalizedQuery = typeof query === "string" ? { search: query } : query;

  return request<Employee[]>(`/api/employees${toQueryString(normalizedQuery)}`, { signal });
}

export function createEmployee(payload: EmployeePayload): Promise<Employee> {
  return request<Employee>("/api/employees", {
    method: "POST",
    body: payload
  });
}

export function updateEmployee(id: number, payload: EmployeePayload): Promise<Employee> {
  return request<Employee>(`/api/employees/${id}`, {
    method: "PUT",
    body: payload
  });
}

export function deleteEmployee(id: number): Promise<void> {
  return request<void>(`/api/employees/${id}`, {
    method: "DELETE"
  });
}

export function getTasks(query?: TaskQuery, signal?: AbortSignal): Promise<ProjectTask[]> {
  return request<ProjectTask[]>(`/api/tasks${toQueryString(query)}`, { signal });
}

export function createTask(payload: TaskPayload): Promise<ProjectTask> {
  return request<ProjectTask>("/api/tasks", {
    method: "POST",
    body: payload
  });
}

export function updateTask(id: number, payload: TaskUpdatePayload): Promise<ProjectTask> {
  return request<ProjectTask>(`/api/tasks/${id}`, {
    method: "PUT",
    body: payload
  });
}

export function changeTaskStatus(id: number, taskStatus: number): Promise<ProjectTask> {
  return request<ProjectTask>(`/api/tasks/${id}/status`, {
    method: "PATCH",
    body: { taskStatus }
  });
}

export function deleteTask(id: number): Promise<void> {
  return request<void>(`/api/tasks/${id}`, {
    method: "DELETE"
  });
}
