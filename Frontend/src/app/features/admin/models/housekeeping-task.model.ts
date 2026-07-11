export interface HousekeepingTask {
  id: number;
  roomId: number;
  location: string | null;
  isEmergency: boolean;
  description: string | null;
  originType: string;
  status: 'Pending' | 'InProgress' | 'Completed';
  createdAt: string;
  startedAt: string | null;
  finishedAt: string | null;
}
