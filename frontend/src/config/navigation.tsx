import {
  Home,
  Settings,
  Users,
  FileText,
  Briefcase,
  LayoutDashboard,
  UserPlus,
  FileSignature,
  Car,
  CheckSquare,
  Boxes,
  ArrowLeftRight,
  BadgeDollarSign,
  Wrench,
  ClipboardList,
} from "lucide-react";
import type { Role } from "@/auth/types";
import type { AppMenu } from "@/rbac/rbac";

export interface NavItem {
  title: string;
  href: string;
  icon: React.ComponentType<{ className?: string }>;
  menu?: AppMenu;
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
    menu: "COMMERCIAL",
    permission: ["SALES_PERSON", "SALES_MANAGER", "MANAGER", "ADMIN"],
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
        permission: ["ADMIN", "MANAGER", "SALES_MANAGER"],
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
    title: "Estoque",
    href: "/stock",
    icon: Boxes,
    menu: "STOCK",
    permission: ["STOCK_PERSON", "STOCK_MANAGER", "SALES_PERSON", "SALES_MANAGER", "MANAGER", "ADMIN"],
    items: [
      {
        title: "Dashboard",
        href: "/stock",
        icon: LayoutDashboard,
      },
      {
        title: "Veículos",
        href: "/stock/vehicles",
        icon: Car,
      },
      {
        title: "Reservas",
        href: "/stock/reservations",
        icon: ClipboardList,
      },
      {
        title: "Movimentações",
        href: "/stock/movements",
        icon: ArrowLeftRight,
      },
      {
        title: "Test-drives",
        href: "/stock/test-drives",
        icon: Car,
      },
      {
        title: "Preparação",
        href: "/stock/preparation",
        icon: Wrench,
        permission: ["STOCK_MANAGER", "MANAGER", "ADMIN"],
      },
      {
        title: "Financeiro",
        href: "/stock/finance",
        icon: BadgeDollarSign,
        permission: ["STOCK_MANAGER", "MANAGER", "ADMIN"],
      },
      {
        title: "Baixas / Exceções",
        href: "/stock/write-offs",
        icon: CheckSquare,
        permission: ["STOCK_MANAGER", "MANAGER", "ADMIN"],
      },
    ],
  },
  {
    title: "Avaliações",
    href: "/evaluations",
    icon: FileText,
    menu: "EVALUATIONS",
    permission: ["VEHICLE_EVALUATOR", "EVALUATION_MANAGER", "MANAGER", "VIEWER", "ADMIN"],
  },
  {
    title: "Admin",
    href: "/admin",
    icon: Users,
    menu: "ADMIN",
    permission: "ADMIN", // Example permission
  },
  {
    title: "Configurações",
    href: "/settings",
    icon: Settings,
    menu: "ADMIN",
    permission: "ADMIN",
  },
];
