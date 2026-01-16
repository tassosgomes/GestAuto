import { format, formatDistanceToNow, parseISO } from 'date-fns';
import { ptBR } from 'date-fns/locale';
import type { ReservationListItem, ReservationStatus } from '../types';

/**
 * Format reservation deadline with countdown
 * Example: "15/01/2026 às 18:00 (expira em 2 dias)"
 */
export function formatReservationDeadline(
  expiresAtUtc: string | null | undefined,
  status: ReservationStatus
): string {
  if (!expiresAtUtc) return '-';
  if (status !== 1) return '-'; // ReservationStatus.Active

  const expiresDate = parseISO(expiresAtUtc);
  const formatted = format(expiresDate, "dd/MM/yyyy 'às' HH:mm", { locale: ptBR });
  const distance = formatDistanceToNow(expiresDate, { locale: ptBR, addSuffix: true });

  return `${formatted} (${distance})`;
}

/**
 * Format bank deadline (date-only display)
 */
export function formatBankDeadline(
  bankDeadlineAtUtc: string | null | undefined
): string {
  if (!bankDeadlineAtUtc) return '-';

  const date = parseISO(bankDeadlineAtUtc);
  return format(date, 'dd/MM/yyyy', { locale: ptBR });
}

/**
 * Check if reservation can be extended by current user
 */
export function canUserExtendReservation(
  userRoles: string[]
): boolean {
  return userRoles.includes('MANAGER') ||
         userRoles.includes('SALES_MANAGER') ||
         userRoles.includes('ADMIN');
}

/**
 * Check if reservation can be cancelled by current user
 */
export function canUserCancelReservation(
  salesPersonId: string,
  userRoles: string[],
  userId: string
): boolean {
  const isManager = userRoles.includes('MANAGER') ||
                    userRoles.includes('SALES_MANAGER') ||
                    userRoles.includes('ADMIN');

  if (isManager) return true;
  return salesPersonId === userId;
}

/**
 * Check if reservation is active and not expired
 */
export function isReservationActive(
  reservation: Pick<ReservationListItem, 'status' | 'expiresAtUtc'>
): boolean {
  if (reservation.status !== 1) return false; // ReservationStatus.Active
  if (!reservation.expiresAtUtc) return true;

  const now = new Date();
  const expiresAt = parseISO(reservation.expiresAtUtc);
  return expiresAt > now;
}
