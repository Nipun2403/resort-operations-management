export interface Claim {
  type: string;
  value: string;
}

export interface AuthMeResponse {
  claims?: Claim[];
  id?: number;
  email?: string;
  firstName?: string;
  lastName?: string;
  role?: string;
  isActive?: boolean;
}

