export interface UserDto {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  isActive: boolean;
  roles: string[];
}

export interface CustomerDto {
  customerId: string;
  customerCode: string;
  customerName: string;
  email: string;
  phone: string;
  companyName?: string;
  address?: string;
  city?: string;
  state?: string;
  status: number; // 1: Active, 2: Inactive, 3: Lead, 4: Archived
  createdDate: string;
  createdBy: string;
  ownerId: string;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface DashboardSummaryDto {
  totalCustomers: number;
  totalLeads: number;
  openLeads: number;
  totalOpportunities: number;
  openOpportunities: number;
  wonOpportunities: number;
  lostOpportunities: number;
  totalPipelineValue: number;
  weightedPipelineValue: number;
}
