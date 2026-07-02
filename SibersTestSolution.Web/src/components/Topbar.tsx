import { LogOut, PanelLeftClose, PanelLeftOpen, RefreshCw, Search } from "lucide-react";

type TopbarProps = {
  title: string;
  isSidebarHidden: boolean;
  onToggleSidebar: () => void;
  search: string;
  onSearchChange: (value: string) => void;
  onRefresh: () => void;
  userName: string;
  onLogout: () => void;
};

export function Topbar({
  title,
  isSidebarHidden,
  onToggleSidebar,
  search,
  onSearchChange,
  onRefresh,
  userName,
  onLogout
}: TopbarProps) {
  const SidebarIcon = isSidebarHidden ? PanelLeftOpen : PanelLeftClose;

  return (
    <header className="topbar">
      <div className="topbar-title">
        <button
          className="icon-button"
          type="button"
          title={isSidebarHidden ? "Показать меню" : "Скрыть меню"}
          onClick={onToggleSidebar}
        >
          <SidebarIcon size={18} />
        </button>
        <div>
          <p className="eyebrow">Sibers Test</p>
          <h1>{title}</h1>
        </div>
      </div>

      <div className="topbar-actions">
        <label className="search-field">
          <Search aria-hidden="true" size={18} />
          <input
            value={search}
            onChange={(event) => onSearchChange(event.target.value)}
            placeholder="Поиск"
          />
        </label>
        <button className="icon-button" type="button" title="Обновить" onClick={onRefresh}>
          <RefreshCw size={18} />
        </button>
        <span className="user-badge">{userName}</span>
        <button className="icon-button" type="button" title="Выйти" onClick={onLogout}>
          <LogOut size={18} />
        </button>
      </div>
    </header>
  );
}
