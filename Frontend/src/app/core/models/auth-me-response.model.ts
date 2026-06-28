export interface Claim {
  type: string;
  value: string;
}

export interface AuthMeResponse {
  claims: Claim[];
}
