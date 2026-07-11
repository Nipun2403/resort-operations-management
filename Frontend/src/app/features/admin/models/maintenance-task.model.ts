export interface MaintenanceTask {
  id: number;
  roomId: number;
  location: string;
  isEmergency: boolean;
  originType: string;
  status: 'Pending' | 'InProgress' | 'Completed';
  description: string;
  createdAt: string;
  startedAt: string | null;
  finishedAt: string | null;
  assignedToUserId?: number;
}
