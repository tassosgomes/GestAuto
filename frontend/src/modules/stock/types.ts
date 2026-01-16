export const VehicleStatus = {
  InTransit: 1,
  InStock: 2,
  Reserved: 3,
  InTestDrive: 4,
  InPreparation: 5,
  Sold: 6,
  WrittenOff: 7
} as const;

export type VehicleStatus = (typeof VehicleStatus)[keyof typeof VehicleStatus];

export const VehicleCategory = {
  New: 1,
  Used: 2,
  Demonstration: 3
} as const;

export type VehicleCategory = (typeof VehicleCategory)[keyof typeof VehicleCategory];

export const ReservationType = {
  Standard: 1,
  PaidDeposit: 2,
  WaitingBank: 3
} as const;

export type ReservationType = (typeof ReservationType)[keyof typeof ReservationType];

export const ReservationStatus = {
  Active: 1,
  Cancelled: 2,
  Completed: 3,
  Expired: 4
} as const;

export type ReservationStatus = (typeof ReservationStatus)[keyof typeof ReservationStatus];

export const CheckInSource = {
  Manufacturer: 1,
  CustomerUsedPurchase: 2,
  StoreTransfer: 3,
  InternalFleet: 4
} as const;

export type CheckInSource = (typeof CheckInSource)[keyof typeof CheckInSource];

export const CheckOutReason = {
  Sale: 1,
  TestDrive: 2,
  Transfer: 3,
  TotalLoss: 4
} as const;

export type CheckOutReason = (typeof CheckOutReason)[keyof typeof CheckOutReason];

export const DemoPurpose = {
  TestDrive: 1,
  InternalFleet: 2
} as const;

export type DemoPurpose = (typeof DemoPurpose)[keyof typeof DemoPurpose];

export const TestDriveOutcome = {
  ReturnedToStock: 1,
  ConvertedToReservation: 2
} as const;

export type TestDriveOutcome = (typeof TestDriveOutcome)[keyof typeof TestDriveOutcome];

export interface VehicleListItem {
  id: string;
  category: VehicleCategory;
  currentStatus: VehicleStatus;
  vin: string;
  plate?: string | null;
  make: string;
  model: string;
  trim?: string | null;
  yearModel: number;
  color: string;
  mileageKm?: number | null;
  price?: number | null;
  location?: string | null;
  imageUrl?: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface VehicleResponse {
  id: string;
  category: VehicleCategory;
  currentStatus: VehicleStatus;
  vin: string;
  plate?: string | null;
  make: string;
  model: string;
  trim?: string | null;
  yearModel: number;
  color: string;
  mileageKm?: number | null;
  price?: number | null;
  location?: string | null;
  imageUrl?: string | null;
  evaluationId?: string | null;
  demoPurpose?: DemoPurpose | null;
  isRegistered: boolean;
  currentOwnerUserId?: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface VehicleHistoryItemResponse {
  type: string;
  occurredAtUtc: string;
  userId: string;
  details: Record<string, unknown>;
}

export interface VehicleHistoryResponse {
  vehicleId: string;
  items: VehicleHistoryItemResponse[];
}

export interface ReservationResponse {
  id: string;
  vehicleId: string;
  type: ReservationType;
  status: ReservationStatus;
  salesPersonId: string;
  contextType: string;
  contextId?: string | null;
  createdAtUtc: string;
  expiresAtUtc?: string | null;
  bankDeadlineAtUtc?: string | null;
  cancelledAtUtc?: string | null;
  cancelledByUserId?: string | null;
  cancelReason?: string | null;
  extendedAtUtc?: string | null;
  extendedByUserId?: string | null;
  previousExpiresAtUtc?: string | null;
}

export interface CreateReservationRequest {
  type: ReservationType;
  contextType: string;
  contextId?: string | null;
  bankDeadlineAtUtc?: string | null;
}

export interface CancelReservationRequest {
  reason: string;
}

export interface ExtendReservationRequest {
  newExpiresAtUtc: string;
}

export interface CheckInCreateRequest {
  source: CheckInSource;
  occurredAt?: string | null;
  notes?: string | null;
}

export interface CheckOutCreateRequest {
  reason: CheckOutReason;
  occurredAt?: string | null;
  notes?: string | null;
}

export interface StartTestDriveRequest {
  customerRef?: string | null;
  startedAt?: string | null;
}

export interface StartTestDriveResponse {
  testDriveId: string;
  vehicleId: string;
  salesPersonId: string;
  customerRef?: string | null;
  startedAtUtc: string;
}

export interface CompleteTestDriveRequest {
  outcome: TestDriveOutcome;
  endedAt?: string | null;
  reservation?: CreateReservationRequest | null;
}

export interface CompleteTestDriveResponse {
  testDriveId: string;
  vehicleId: string;
  outcome: TestDriveOutcome;
  endedAtUtc: string;
  currentStatus: VehicleStatus;
  reservationId?: string | null;
}

const VEHICLE_STATUS_LABELS: Record<number, string> = {
  [VehicleStatus.InTransit]: 'Em trânsito',
  [VehicleStatus.InStock]: 'Em estoque',
  [VehicleStatus.Reserved]: 'Reservado',
  [VehicleStatus.InTestDrive]: 'Em test-drive',
  [VehicleStatus.InPreparation]: 'Em preparação',
  [VehicleStatus.Sold]: 'Vendido',
  [VehicleStatus.WrittenOff]: 'Baixado'
};

const VEHICLE_CATEGORY_LABELS: Record<number, string> = {
  [VehicleCategory.New]: 'Novo',
  [VehicleCategory.Used]: 'Seminovo',
  [VehicleCategory.Demonstration]: 'Demonstração'
};

const RESERVATION_TYPE_LABELS: Record<number, string> = {
  [ReservationType.Standard]: 'Padrão',
  [ReservationType.PaidDeposit]: 'Entrada paga',
  [ReservationType.WaitingBank]: 'Aguardando banco'
};

const RESERVATION_STATUS_LABELS: Record<number, string> = {
  [ReservationStatus.Active]: 'Ativa',
  [ReservationStatus.Cancelled]: 'Cancelada',
  [ReservationStatus.Completed]: 'Concluída',
  [ReservationStatus.Expired]: 'Expirada'
};

const CHECK_IN_SOURCE_LABELS: Record<number, string> = {
  [CheckInSource.Manufacturer]: 'Montadora',
  [CheckInSource.CustomerUsedPurchase]: 'Compra cliente/seminovo',
  [CheckInSource.StoreTransfer]: 'Transferência entre lojas',
  [CheckInSource.InternalFleet]: 'Frota interna'
};

const CHECK_OUT_REASON_LABELS: Record<number, string> = {
  [CheckOutReason.Sale]: 'Venda',
  [CheckOutReason.TestDrive]: 'Test-drive',
  [CheckOutReason.Transfer]: 'Transferência',
  [CheckOutReason.TotalLoss]: 'Baixa sinistro/perda total'
};

const DEMO_PURPOSE_LABELS: Record<number, string> = {
  [DemoPurpose.TestDrive]: 'Test-drive',
  [DemoPurpose.InternalFleet]: 'Frota interna'
};

const TEST_DRIVE_OUTCOME_LABELS: Record<number, string> = {
  [TestDriveOutcome.ReturnedToStock]: 'Retornou ao estoque',
  [TestDriveOutcome.ConvertedToReservation]: 'Convertido em reserva'
};

const UNKNOWN_LABEL = 'Desconhecido';

const getLabelFromMap = (value: number | null | undefined, labels: Record<number, string>): string => {
  if (value == null) {
    return UNKNOWN_LABEL;
  }

  return labels[value] ?? UNKNOWN_LABEL;
};

export const mapVehicleStatusLabel = (status?: VehicleStatus | null): string =>
  getLabelFromMap(status ?? null, VEHICLE_STATUS_LABELS);

export const mapVehicleCategoryLabel = (category?: VehicleCategory | null): string =>
  getLabelFromMap(category ?? null, VEHICLE_CATEGORY_LABELS);

export const mapReservationTypeLabel = (type?: ReservationType | null): string =>
  getLabelFromMap(type ?? null, RESERVATION_TYPE_LABELS);

export const mapReservationStatusLabel = (status?: ReservationStatus | null): string =>
  getLabelFromMap(status ?? null, RESERVATION_STATUS_LABELS);

export const mapCheckInSourceLabel = (source?: CheckInSource | null): string =>
  getLabelFromMap(source ?? null, CHECK_IN_SOURCE_LABELS);

export const mapCheckOutReasonLabel = (reason?: CheckOutReason | null): string =>
  getLabelFromMap(reason ?? null, CHECK_OUT_REASON_LABELS);

export const mapDemoPurposeLabel = (purpose?: DemoPurpose | null): string =>
  getLabelFromMap(purpose ?? null, DEMO_PURPOSE_LABELS);

export const mapTestDriveOutcomeLabel = (outcome?: TestDriveOutcome | null): string =>
  getLabelFromMap(outcome ?? null, TEST_DRIVE_OUTCOME_LABELS);

export const toBankDeadlineAtUtc = (dateOnly: string): string => {
  if (!dateOnly) {
    return '';
  }

  const [year, month, day] = dateOnly.split('-').map(Number);

  if (!year || !month || !day) {
    return '';
  }

  const localDeadline = new Date(year, month - 1, day, 18, 0, 0, 0);

  if (Number.isNaN(localDeadline.getTime())) {
    return '';
  }

  return localDeadline.toISOString();
};
