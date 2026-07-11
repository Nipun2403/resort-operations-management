export interface CustomerRequest {
  id: number;
  type: 'Housekeeping' | 'Maintenance' | 'Food Order';
  roomId: number;
  roomNumber: string;
  description: string;
  status: string;
  createdAt: string;
  isEmergency: boolean;
}
