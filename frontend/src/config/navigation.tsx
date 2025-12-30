import { Home, Settings, Users, FileText, Briefcase, LayoutDashboard, UserPlus, FileSignature, Car, CheckSquare } from "lucide-react";
import type { Role } from "@/auth/types";

export interface NavItem {
  title: string;
  href: string;
  icon: React.ComponentType<{ className?: string }>;
  permission?: Role | Role[];
  items?: NavItem[];
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
    items: [
      {
        title: "Dashboard",
        href: "/commercial",
        icon: LayoutDashboard,
      },
      {
        title: "Leads",
        href: "/commercial/leads",
        icon: UserPlus,
      },
      {
        title: "Propostas",
        href: "/commercial/proposals",
        icon: FileSignature,
      },
      {
        title: "Test-Drives",
        href: "/commercial/test-drives",
        icon: Car,
      },
      {
        title: "Aprovações",
        href: "/commercial/approvals",
        icon: CheckSquare,
      },
      {
        title: "Pipeline",
        href: "/commercial/pipeline",
        icon: LayoutDashboard,
        permission: ["ADMIN", "MANAGER", "SALES_MANAGER"],
      },
    ]
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
