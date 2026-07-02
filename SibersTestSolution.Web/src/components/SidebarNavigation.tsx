import { BriefcaseBusiness, ListTodo, Users } from "lucide-react";
import type { ViewName } from "../types";

type SidebarNavigationProps = {
  activeView: ViewName;
  views: ViewName[];
  onChange: (view: ViewName) => void;
};

const items: Array<{ view: ViewName; label: string; Icon: typeof BriefcaseBusiness }> = [
  { view: "projects", label: "Проекты", Icon: BriefcaseBusiness },
  { view: "employees", label: "Работники", Icon: Users },
  { view: "tasks", label: "Задачи", Icon: ListTodo }
];

export function SidebarNavigation({ activeView, views, onChange }: SidebarNavigationProps) {
  return (
    <nav className="side-nav" aria-label="Основная навигация">
      <div className="brand-mark">STS</div>
      <div className="side-nav-items">
        {items.filter((item) => views.includes(item.view)).map(({ view, label, Icon }) => (
          <button
            className={activeView === view ? "nav-button active" : "nav-button"}
            key={view}
            type="button"
            onClick={() => onChange(view)}
          >
            <Icon size={18} />
            <span>{label}</span>
          </button>
        ))}
      </div>
    </nav>
  );
}
