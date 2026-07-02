import { formatEmployee, taskStatuses } from "../utils/format";
import type { ProjectTask } from "../types";

type TaskListProps = {
  tasks: ProjectTask[];
};

export function TaskList({ tasks }: TaskListProps) {
  if (tasks.length === 0) {
    return <div className="panel-empty">Нет задач</div>;
  }

  return (
    <div className="list-stack">
      {tasks.map((task) => (
        <div className="list-row task-row" key={task.id}>
          <div>
            <span>{task.name}</span>
            <small>
              {taskStatuses[task.taskStatus] ?? "Unknown"} · {formatEmployee(task.taskOwner)}
            </small>
          </div>
          <span className="priority-pill">{task.taskPriority}</span>
        </div>
      ))}
    </div>
  );
}
