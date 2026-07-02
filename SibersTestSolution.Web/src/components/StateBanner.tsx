import { ClipboardList } from "lucide-react";

type StateBannerProps = {
  message: string;
};

export function StateBanner({ message }: StateBannerProps) {
  return (
    <div className="state-banner error">
      <ClipboardList aria-hidden="true" size={20} />
      <span>{message}</span>
    </div>
  );
}
