export type StaffRole =
  | 'Admin'
  | 'FrontDesk'
  | 'Kitchen'
  | 'Housekeeping'
  | 'Maintenance';

export interface Staff {
  id: number;
  email: string;
  firstName: string;
  lastName: string;
  role: StaffRole;
  isActive: boolean;
  createdAt: string;
}

export interface CreateStaffDTO {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  role: StaffRole;
}

export interface UpdateStaffDTO {
  firstName?: string;
  lastName?: string;
  role?: StaffRole;
  isActive?: boolean;
}
