import { Badge } from '@/components/ui/badge';
import type { ReservationStatus } from '../../types';
import { mapReservationStatusLabel } from '../../types';

const STATUS_VARIANTS: Record<number, "default" | "secondary" | "destructive" | "outline"> = {
  1: "default",      // Active
  2: "destructive",  // Cancelled
  3: "secondary",    // Completed
  4: "outline",      // Expired
};

interface ReservationStatusBadgeProps {
  status: ReservationStatus;
}

export function ReservationStatusBadge({ status }: ReservationStatusBadgeProps) {
  const label = mapReservationStatusLabel(status);
  const variant = STATUS_VARIANTS[status] ?? "outline";

  return <Badge variant={variant}>{label}</Badge>;
}
