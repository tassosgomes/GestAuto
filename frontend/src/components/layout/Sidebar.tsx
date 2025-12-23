import { Link, useLocation } from "react-router-dom";
import { cn } from "@/lib/utils";
import { navItems } from "@/config/navigation";
import { buttonVariants } from "@/components/ui/button";
import { useAuth } from "@/auth/useAuth";
import { Role } from "@/auth/types";

export function Sidebar({ className, ...props }: React.HTMLAttributes<HTMLElement>) {
  const location = useLocation();
  const authState = useAuth();

  const userRoles = authState.status === 'ready' ? authState.session.roles : [];

  const filteredNavItems = navItems.filter(item => {
    if (!item.permission) return true;
    return userRoles.includes(item.permission as Role);
  });

  return (
    <nav
      className={cn(
        "grid items-start gap-2 text-sm font-medium",
        className
      )}
      {...props}
    >
      {filteredNavItems.map((item) => (
        <Link
          key={item.href}
          to={item.href}
          className={cn(
            buttonVariants({ variant: "ghost" }),
            location.pathname === item.href
              ? "bg-muted text-primary"
              : "text-muted-foreground hover:text-primary",
            "justify-start gap-3 px-3 py-2 transition-all"
          )}
        >
          <item.icon className="h-4 w-4" />
          {item.title}
        </Link>
      ))}
    </nav>
  );
}
