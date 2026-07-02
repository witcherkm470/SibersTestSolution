import { useState } from "react";
import { Upload, X } from "lucide-react";
import { toDateInputValue } from "../utils/format";
import type { Project, ProjectPayload } from "../types";

const maxProjectDocumentSizeBytes = 5 * 1024 * 1024;

type ProjectFormProps = {
  project?: Project;
  onSubmit: (payload: ProjectPayload, files: File[]) => Promise<void>;
  onCancel: () => void;
};

export function ProjectForm({ project, onSubmit, onCancel }: ProjectFormProps) {
  const [form, setForm] = useState<ProjectPayload>({
    name: project?.name ?? "",
    customerCompanyName: project?.customerCompanyName ?? "",
    contractorCompanyName: project?.contractorCompanyName ?? "",
    projectStartDate: project ? toDateInputValue(project.projectStartDate) : "",
    projectEndDate: project ? toDateInputValue(project.projectEndDate) : "",
    projectPriority: project?.projectPriority ?? 1
  });
  const [replacementFile, setReplacementFile] = useState<File | null>(null);
  const [fileError, setFileError] = useState<string | null>(null);
  const [isSaving, setIsSaving] = useState(false);

  const currentDocument = project?.documents?.[0] ?? null;

  function selectReplacementFile(fileList: FileList | null) {
    const file = fileList?.[0] ?? null;

    if (!file) {
      return;
    }

    if (file.size > maxProjectDocumentSizeBytes) {
      setFileError(`Файл "${file.name}" больше 5 МБ.`);
      return;
    }

    setFileError(null);
    setReplacementFile(file);
  }

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setIsSaving(true);

    try {
      await onSubmit(form, replacementFile ? [replacementFile] : []);
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <form className="entity-form" onSubmit={handleSubmit}>
      <label>
        <span>Название проекта</span>
        <input value={form.name} onChange={(event) => setForm({ ...form, name: event.target.value })} />
      </label>
      <label>
        <span>Компания заказчика</span>
        <input
          value={form.customerCompanyName}
          onChange={(event) => setForm({ ...form, customerCompanyName: event.target.value })}
        />
      </label>
      <label>
        <span>Компания исполнителя</span>
        <input
          value={form.contractorCompanyName}
          onChange={(event) => setForm({ ...form, contractorCompanyName: event.target.value })}
        />
      </label>
      <div className="form-grid">
        <label>
          <span>Дата начала</span>
          <input
            type="date"
            value={form.projectStartDate}
            onChange={(event) => setForm({ ...form, projectStartDate: event.target.value })}
          />
        </label>
        <label>
          <span>Дата окончания</span>
          <input
            type="date"
            value={form.projectEndDate}
            onChange={(event) => setForm({ ...form, projectEndDate: event.target.value })}
          />
        </label>
      </div>
      <label>
        <span>Приоритет</span>
        <input
          type="number"
          min={1}
          value={form.projectPriority}
          onChange={(event) => setForm({ ...form, projectPriority: Number(event.target.value) })}
        />
      </label>

      <div className="field-group">
        <span className="field-label">Файл проекта</span>
        {currentDocument ? (
          <div className="uploaded-file-row">
            <span>{currentDocument.originalFileName}</span>
            <small>Текущий файл</small>
          </div>
        ) : null}
        <label
          className="file-dropzone"
          onDragOver={(event) => event.preventDefault()}
          onDrop={(event) => {
            event.preventDefault();
            selectReplacementFile(event.dataTransfer.files);
          }}
        >
          <Upload size={24} />
          <span>{currentDocument ? "Выберите новый файл, чтобы заменить текущий" : "Выберите файл проекта"}</span>
          <input type="file" onChange={(event) => selectReplacementFile(event.target.files)} />
        </label>
        {fileError ? <small className="field-error">{fileError}</small> : null}
        {replacementFile ? (
          <div className="uploaded-file-row">
            <span>{replacementFile.name}</span>
            <button type="button" title="Убрать" onClick={() => setReplacementFile(null)}>
              <X size={16} />
            </button>
          </div>
        ) : null}
      </div>

      <div className="form-actions">
        <button className="secondary-button" type="button" onClick={onCancel}>
          Отмена
        </button>
        <button className="primary-button" type="submit" disabled={isSaving}>
          Сохранить
        </button>
      </div>
    </form>
  );
}
