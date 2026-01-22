import { useEffect, useState } from "react";
import { Link, useLocation, useNavigate } from "react-router-dom";
import { cn } from "@/lib/utils";
import { navItems } from "@/config/navigation";
import type { NavItem } from "@/config/navigation";
import { buttonVariants } from "@/components/ui/button";
import { useAuth } from "@/auth/useAuth";
import { ChevronDown, ChevronRight } from "lucide-react";
import { canAccessMenu } from "@/rbac/rbac";

export function Sidebar({ className, ...props }: React.HTMLAttributes<HTMLElement>) {
  const location = useLocation();
  const navigate = useNavigate();
  const authState = useAuth();
  
  // Initialize open menus based on current path
  const [openMenus, setOpenMenus] = useState<string[]>(() => {
    return navItems
      .filter(item => item.items && location.pathname.startsWith(item.href))
      .map(item => item.href);
  });

  useEffect(() => {
    const activeParents = navItems
      .filter(item => item.items && location.pathname.startsWith(item.href))
      .map(item => item.href);

    if (activeParents.length === 0) return;

    setOpenMenus((prev) => {
      const next = new Set(prev);
      activeParents.forEach((href) => next.add(href));
      return Array.from(next);
    });
  }, [location.pathname]);

  const userRoles = authState.status === 'ready' ? authState.session.roles : [];

  const hasPermission = (item: NavItem): boolean => {
    if (item.menu && !canAccessMenu(userRoles, item.menu)) return false;
    if (!item.permission) return true;
    const permissions = Array.isArray(item.permission) ? item.permission : [item.permission];
    return permissions.some(permission => userRoles.includes(permission));
  };

  const filterNavItems = (items: NavItem[]): NavItem[] => {
    return items.reduce<NavItem[]>((acc, item) => {
      if (!hasPermission(item)) {
        return acc;
      }

      if (item.items && item.items.length > 0) {
        const filteredChildren = filterNavItems(item.items);
        if (filteredChildren.length === 0) {
          return acc;
        }

        acc.push({
          ...item,
          items: filteredChildren,
        });
        return acc;
      }

      acc.push(item);
      return acc;
    }, []);
  };

  const filteredNavItems = filterNavItems(navItems);

  const toggleMenu = (href: string) => {
    setOpenMenus(prev => 
      prev.includes(href) 
        ? prev.filter(h => h !== href) 
        : [...prev, href]
    );
  };

  const renderNavItem = (item: NavItem, level = 0) => {
    const hasChildren = item.items && item.items.length > 0;
    const isOpen = openMenus.includes(item.href);
    const isActive = location.pathname === item.href;
    const isParentActive = hasChildren && location.pathname.startsWith(item.href);

    return (
      <div key={item.href} className="w-full">
        {hasChildren ? (
          <button
            type="button"
            onClick={() => {
              if (location.pathname !== item.href) {
                setOpenMenus((prev) => (prev.includes(item.href) ? prev : [...prev, item.href]));
                navigate(item.href);
                return;
              }

              toggleMenu(item.href);
            }}
            className={cn(
              buttonVariants({ variant: "ghost" }),
              "w-full justify-between px-3 py-2 hover:bg-muted/50",
              (isActive || isParentActive) && "text-primary font-medium"
            )}
          >
            <div className="flex items-center gap-3">
              <item.icon className="h-4 w-4" />
              {item.title}
            </div>
            {isOpen ? <ChevronDown className="h-4 w-4" /> : <ChevronRight className="h-4 w-4" />}
          </button>
        ) : (
          <Link
            to={item.href}
            className={cn(
              buttonVariants({ variant: "ghost" }),
              isActive
                ? "bg-muted text-primary"
                : "text-muted-foreground hover:text-primary",
              "w-full justify-start gap-3 px-3 py-2 transition-all",
              level > 0 && "pl-10"
            )}
          >
            <item.icon className="h-4 w-4" />
            {item.title}
          </Link>
        )}

        {hasChildren && isOpen && (
          <div className="flex flex-col gap-1 mt-1">
            {item.items!.map(subItem => renderNavItem(subItem, level + 1))}
          </div>
        )}
      </div>
    );
  };

  return (
    <nav
      className={cn(
        "grid items-start gap-2 text-sm font-medium",
        className
      )}
      {...props}
    >
      {filteredNavItems.map(item => renderNavItem(item))}
    </nav>
  );
}
