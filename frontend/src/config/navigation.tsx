import { Home, Settings, Users, FileText, Briefcase } from "lucide-react";

export interface NavItem {
  title: string;
  href: string;
  icon: React.ComponentType<{ className?: string }>;
  permission?: string;
}

export const navItems: NavItem[] = [
  {
    title: "Home",
    href: "/",
    icon: Home,
  },
  {
    title: "Comercial",
    href: "/commercial",
    icon: Briefcase,
  },
  {
    title: "Avaliações",
    href: "/evaluations",
    icon: FileText,
  },
  {
    title: "Admin",
    href: "/admin",
    icon: Users,
    permission: "ADMIN", // Example permission
  },
  {
    title: "Configurações",
    href: "/settings",
    icon: Settings,
  },
];
