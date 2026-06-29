import { Observable } from 'rxjs';

export interface Task {
  id: number;
  status: string; // raw status from API (e.g., 'Pending', 'InProgress', 'Completed')
  location: string; // e.g., 'Room 201', 'Lobby', 'N/A'
  description: string; // e.g., 'AC not working', 'Order #123'
  createdAt: string; // ISO date
  raw: any; // original DTO for detail modal
}

export interface DetailSection {
  title: string; // e.g., 'Basic Information'
  fields: { label: string; value: string }[];
}

export interface TaskDashboardConfig<T extends Task = Task> {
  entityName: string; // 'Food Order', 'Housekeeping Task', etc.
  fetchTasks: (params: {
    pageNumber: number;
    pageSize: number;
    status?: string;
    sortBy?: string;
    sortDescending?: boolean;
  }) => Observable<{ totalCount: number; data: T[] }>;

  updateTaskStatus: (id: number, newStatus: string) => Observable<void>;

  statusOptions: { value: string; label: string }[]; // includes 'All' option

  getLocation: (task: T) => string;
  getDescription: (task: T) => string;
  getDetailSections: (task: T) => DetailSection[];
}
