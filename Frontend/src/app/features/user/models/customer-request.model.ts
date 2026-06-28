export interface CustomerRequest {
  id: number;
  type: 'Housekeeping' | 'Maintenance';
  roomId: number;
  roomNumber: string;
  description: string;
  status: string;
  createdAt: string;
}
