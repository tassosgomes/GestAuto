export interface PagedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

// --- Payment Method Types ---

export interface PaymentMethod {
  id: number;
  code: string;
  name: string;
  isActive: boolean;
  displayOrder: number;
}

// --- Lead Types ---

export interface Lead {
  id: string;
  name: string;
  email?: string;
  phone?: string;
  source?: string;
  status: string; // 'New', 'Contacted', 'Qualified', 'Converted', 'Lost'
  score?: string; // 'Diamond', 'Gold', 'Silver', 'Bronze'
  salesPersonId: string;
  interestedModel?: string;
  interestedTrim?: string;
  interestedColor?: string;
  qualification?: Qualification;
  interactions?: Interaction[];
  createdAt: string;
  lastInteractionAt?: string;
  updatedAt?: string;
}

export interface CreateLeadRequest {
  name: string;
  email: string;
  phone: string;
  source: string;
  interestedModel?: string;
  interestedTrim?: string;
  interestedColor?: string;
}

export interface UpdateLeadRequest {
  name?: string;
  email?: string;
  phone?: string;
  status?: string;
  interestedModel?: string;
  interestedTrim?: string;
  interestedColor?: string;
}

export interface QualifyLeadRequest {
  hasTradeInVehicle: boolean;
  tradeInVehicle?: TradeInVehicle;
  paymentMethod: string;
  estimatedMonthlyIncome?: number | null;
  expectedPurchaseDate?: string | null;
  interestedInTestDrive: boolean;
}

export interface Qualification {
  hasTradeInVehicle: boolean;
  tradeInVehicle?: TradeInVehicle;
  paymentMethod?: string;
  estimatedMonthlyIncome?: number;
  expectedPurchaseDate?: string;
  interestedInTestDrive: boolean;
}

export interface TradeInVehicle {
  brand?: string;
  model?: string;
  year: number;
  mileage: number;
  licensePlate?: string;
  color?: string;
  generalCondition?: string;
  hasDealershipServiceHistory: boolean;
}

export interface Interaction {
  id: string;
  type: string; // 'Call', 'Email', 'WhatsApp', 'Visit'
  description: string;
  occurredAt: string;
  createdAt: string;
}

export interface RegisterInteractionRequest {
  type: string;
  description: string;
}

// --- Proposal Types ---

export interface Proposal {
  id: string;
  leadId: string;
  status: string; // 'Draft', 'Sent', 'Negotiation', 'Approved', 'Rejected'
  vehicleModel?: string;
  vehicleTrim?: string;
  vehicleColor?: string;
  vehicleYear?: number;
  isReadyDelivery: boolean;
  vehiclePrice: number;
  discountAmount: number;
  discountReason?: string;
  discountApproved: boolean;
  discountApproverId?: string;
  tradeInValue: number;
  paymentMethod?: string;
  downPayment?: number;
  installments?: number;
  items?: ProposalItem[];
  usedVehicleEvaluationId?: string;
  totalValue: number;
  createdAt: string;
  updatedAt?: string;
}

export interface ProposalListItem {
  id: string;
  leadId: string;
  status: string;
  vehicleModel: string;
  totalValue: number;
  createdAt: string;
}

export interface ProposalItem {
  id: string;
  description: string;
  value: number;
}

export interface CreateProposalRequest {
  leadId: string;
  vehicleModel?: string;
  vehicleTrim?: string;
  vehicleColor?: string;
  vehicleYear: number;
  isReadyDelivery: boolean;
  vehiclePrice: number;
  paymentMethod?: string;
  downPayment?: number;
  installments?: number;
  items?: { description: string; value: number }[];
  tradeIn?: {
    model: string;
    year: number;
    mileage: number;
    value: number;
  };
  discount?: number;
}

export interface UpdateProposalRequest {
  vehicleModel?: string;
  vehicleTrim?: string;
  vehicleColor?: string;
  vehicleYear?: number;
  isReadyDelivery?: boolean;
  vehiclePrice?: number;
  paymentMethod?: string;
  downPayment?: number;
  installments?: number;
}

export interface ApplyDiscountRequest {
  amount: number;
  reason: string;
}

export interface AddProposalItemRequest {
  description: string;
  value: number;
}

// --- Used Vehicle Evaluation Types ---

export interface UsedVehicleEvaluation {
  id: string;
  proposalId: string;
  status: string;
  vehicle: {
    brand: string;
    model: string;
    year: number;
    mileage: number;
    licensePlate: string;
    color: string;
    generalCondition: string;
    hasDealershipServiceHistory: boolean;
  };
  evaluatedValue?: number | null;
  evaluationNotes?: string | null;
  requestedAt: string;
  respondedAt?: string | null;
  customerAccepted?: boolean | null;
  customerRejectionReason?: string | null;
}

export interface RequestUsedVehicleEvaluationRequest {
  proposalId: string;
  brand: string;
  model: string;
  year: number;
  mileage: number;
  licensePlate: string;
  color: string;
  generalCondition: string;
  hasDealershipServiceHistory: boolean;
}

export interface UsedVehicleEvaluationCustomerResponseRequest {
  accepted: boolean;
  rejectionReason?: string;
}

// --- Test Drive Types ---

export interface TestDrive {
  id: string;
  leadId: string;
  vehicleId: string;
  status: string; // 'Scheduled', 'Completed', 'Cancelled', 'NoShow'
  scheduledAt: string;
  completedAt?: string;
  salesPersonId: string;
  notes?: string;
  checklist?: TestDriveChecklist;
  customerFeedback?: string;
  cancellationReason?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface TestDriveChecklist {
  initialMileage: number;
  finalMileage: number;
  fuelLevel?: string;
  visualObservations?: string;
}

export interface ScheduleTestDriveRequest {
  leadId: string;
  vehicleId: string;
  scheduledAt: string;
  notes?: string;
}

export interface CompleteTestDriveRequest {
  checklist: TestDriveChecklist;
  customerFeedback?: string;
}

export interface CancelTestDriveRequest {
  reason: string;
}
