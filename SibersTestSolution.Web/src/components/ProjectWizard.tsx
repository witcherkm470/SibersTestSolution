import { useEffect, useMemo, useState } from "react";
import { Check, Search, Upload, X } from "lucide-react";
import { getEmployees } from "../api";
import type { Employee, ProjectPayload, UserRole } from "../types";
import { formatEmployee } from "../utils/format";

const maxProjectDocumentSizeBytes = 5 * 1024 * 1024;

type ProjectWizardProps = {
  onSubmit: (payload: ProjectPayload, files: File[]) => Promise<void>;
  onCancel: () => void;
};

type RemoteEmployeeSearchProps = {
  selectedEmployeeIds: number[];
  mode: "single" | "multiple";
  placeholder: string;
  roleFilter?: UserRole;
  onChange: (employees: Employee[]) => void;
};

function RemoteEmployeeSearch({
  selectedEmployeeIds,
  mode,
  placeholder,
  roleFilter,
  onChange
}: RemoteEmployeeSearchProps) {
  const [query, setQuery] = useState("");
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [selectedEmployees, setSelectedEmployees] = useState<Employee[]>([]);
  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    const controller = new AbortController();
    const timeoutId = window.setTimeout(() => {
      setIsLoading(true);
      getEmployees(query, controller.signal)
        .then(setEmployees)
        .catch((error) => {
          if (!(error instanceof DOMException && error.name === "AbortError")) {
            setEmployees([]);
          }
        })
        .finally(() => setIsLoading(false));
    }, 250);

    return () => {
      window.clearTimeout(timeoutId);
      controller.abort();
    };
  }, [query]);

  useEffect(() => {
    setSelectedEmployees((currentEmployees) =>
      currentEmployees.filter((employee) => selectedEmployeeIds.includes(employee.id))
    );
  }, [selectedEmployeeIds]);

  function selectEmployee(employee: Employee) {
    if (mode === "single") {
      setSelectedEmployees([employee]);
      onChange([employee]);
      return;
    }

    if (selectedEmployeeIds.includes(employee.id)) {
      return;
    }

    const nextEmployees = [...selectedEmployees, employee];
    setSelectedEmployees(nextEmployees);
    onChange(nextEmployees);
  }

  function removeEmployee(employeeId: number) {
    const nextEmployees = selectedEmployees.filter((employee) => employee.id !== employeeId);
    setSelectedEmployees(nextEmployees);
    onChange(nextEmployees);
  }

  const visibleEmployees = useMemo(() => {
    if (!roleFilter) {
      return employees;
    }

    return employees.filter((employee) => employee.roles?.includes(roleFilter));
  }, [employees, roleFilter]);

  return (
    <div className="remote-employee-search">
      <label className="search-field full-width">
        <Search aria-hidden="true" size={18} />
        <input value={query} onChange={(event) => setQuery(event.target.value)} placeholder={placeholder} />
      </label>

      {selectedEmployees.length > 0 ? (
        <div className="selected-chip-list">
          {selectedEmployees.map((employee) => (
            <span className="selected-chip" key={employee.id}>
              {formatEmployee(employee)}
              <button type="button" title="Убрать" onClick={() => removeEmployee(employee.id)}>
                <X size={14} />
              </button>
            </span>
          ))}
        </div>
      ) : null}

      <div className="picker-list compact">
        {visibleEmployees.map((employee) => {
          const isSelected = selectedEmployeeIds.includes(employee.id);

          return (
            <button
              className={isSelected ? "picker-button active" : "picker-button"}
              key={employee.id}
              type="button"
              onClick={() => selectEmployee(employee)}
            >
              <span>
                <strong>{formatEmployee(employee)}</strong>
                <small>{employee.email}</small>
              </span>
              {isSelected ? <Check size={16} /> : null}
            </button>
          );
        })}
        {visibleEmployees.length === 0 ? (
          <div className="employee-combobox-empty">{isLoading ? "Ищем..." : "Ничего не найдено"}</div>
        ) : null}
      </div>
    </div>
  );
}

export function ProjectWizard({ onSubmit, onCancel }: ProjectWizardProps) {
  const [step, setStep] = useState(1);
  const [form, setForm] = useState<ProjectPayload>({
    name: "",
    customerCompanyName: "",
    contractorCompanyName: "",
    projectStartDate: "",
    projectEndDate: "",
    projectPriority: 1,
    projectManagerId: null,
    employeeIds: []
  });
  const [files, setFiles] = useState<File[]>([]);
  const [fileError, setFileError] = useState<string | null>(null);
  const [isSaving, setIsSaving] = useState(false);

  const stepTitle = useMemo(() => {
    return ["Проект", "Компании", "Менеджер", "Исполнители", "Документы"][step - 1];
  }, [step]);

  function addFiles(nextFiles: FileList | File[]) {
    const acceptedFiles = Array.from(nextFiles);
    const tooLargeFile = acceptedFiles.find((file) => file.size > maxProjectDocumentSizeBytes);

    if (tooLargeFile) {
      setFileError(`Файл "${tooLargeFile.name}" больше 5 МБ.`);
      return;
    }

    setFileError(null);
    setFiles((currentFiles) => [...currentFiles, ...acceptedFiles]);
  }

  function removeFile(fileName: string) {
    setFiles((currentFiles) => currentFiles.filter((file) => file.name !== fileName));
  }

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (step < 5) {
      setStep((currentStep) => currentStep + 1);
      return;
    }

    setIsSaving(true);

    try {
      await onSubmit(form, files);
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <form className="entity-form project-wizard" onSubmit={handleSubmit}>
      <div className="wizard-steps" aria-label="Шаги создания проекта">
        {[1, 2, 3, 4, 5].map((item) => (
          <button
            className={item === step ? "wizard-step active" : "wizard-step"}
            key={item}
            type="button"
            onClick={() => setStep(item)}
          >
            {item}
          </button>
        ))}
      </div>

      <h3>{stepTitle}</h3>

      {step === 1 ? (
        <>
          <label>
            <span>Название проекта</span>
            <input required value={form.name} onChange={(event) => setForm({ ...form, name: event.target.value })} />
          </label>
          <div className="form-grid">
            <label>
              <span>Дата начала</span>
              <input
                required
                type="date"
                value={form.projectStartDate}
                onChange={(event) => setForm({ ...form, projectStartDate: event.target.value })}
              />
            </label>
            <label>
              <span>Дата окончания</span>
              <input
                required
                type="date"
                value={form.projectEndDate}
                onChange={(event) => setForm({ ...form, projectEndDate: event.target.value })}
              />
            </label>
          </div>
          <label>
            <span>Приоритет</span>
            <input
              required
              type="number"
              min={1}
              value={form.projectPriority}
              onChange={(event) => setForm({ ...form, projectPriority: Number(event.target.value) })}
            />
          </label>
        </>
      ) : null}

      {step === 2 ? (
        <>
          <label>
            <span>Компания заказчика</span>
            <input
              required
              value={form.customerCompanyName}
              onChange={(event) => setForm({ ...form, customerCompanyName: event.target.value })}
            />
          </label>
          <label>
            <span>Компания исполнителя</span>
            <input
              required
              value={form.contractorCompanyName}
              onChange={(event) => setForm({ ...form, contractorCompanyName: event.target.value })}
            />
          </label>
        </>
      ) : null}

      {step === 3 ? (
        <RemoteEmployeeSearch
          mode="single"
          placeholder="Найти менеджера проекта"
          roleFilter="ProjectManager"
          selectedEmployeeIds={form.projectManagerId ? [form.projectManagerId] : []}
          onChange={(employees) => setForm({ ...form, projectManagerId: employees[0]?.id ?? null })}
        />
      ) : null}

      {step === 4 ? (
        <RemoteEmployeeSearch
          mode="multiple"
          placeholder="Найти исполнителя проекта"
          selectedEmployeeIds={form.employeeIds ?? []}
          onChange={(employees) => setForm({ ...form, employeeIds: employees.map((employee) => employee.id) })}
        />
      ) : null}

      {step === 5 ? (
        <>
          <label
            className="file-dropzone"
            onDragOver={(event) => event.preventDefault()}
            onDrop={(event) => {
              event.preventDefault();
              addFiles(event.dataTransfer.files);
            }}
          >
            <Upload size={24} />
            <span>Перетащи документы сюда или выбери файлы</span>
            <input type="file" multiple onChange={(event) => event.target.files && addFiles(event.target.files)} />
          </label>
          {fileError ? <small className="field-error">{fileError}</small> : null}
          {files.length > 0 ? (
            <div className="uploaded-file-list">
              {files.map((file) => (
                <div className="uploaded-file-row" key={`${file.name}-${file.lastModified}`}>
                  <span>{file.name}</span>
                  <button type="button" title="Убрать" onClick={() => removeFile(file.name)}>
                    <X size={16} />
                  </button>
                </div>
              ))}
            </div>
          ) : null}
        </>
      ) : null}

      <div className="form-actions">
        <button className="secondary-button" type="button" onClick={step === 1 ? onCancel : () => setStep(step - 1)}>
          {step === 1 ? "Отмена" : "Назад"}
        </button>
        <button className="primary-button" type="submit" disabled={isSaving}>
          {step === 5 ? "Создать" : "Далее"}
        </button>
      </div>
    </form>
  );
}
