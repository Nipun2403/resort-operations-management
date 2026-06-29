import { Component, inject } from '@angular/core';
import { map } from 'rxjs/operators';
import { TaskDashboardComponent } from '../../../shared/components/task-dashboard/task-dashboard.component';
import { TaskDashboardConfig, Task, DetailSection } from '../../../shared/models/task.model';
import { HousekeepingApiService } from '../../user/services/housekeeping-api.service';
import { HousekeepingTask } from '../../admin/models/housekeeping-task.model';

@Component({
  selector: 'app-housekeeping-dashboard',
  standalone: true,
  imports: [TaskDashboardComponent],
  template: `<app-task-dashboard [config]="config" />`,
})
export class HousekeepingDashboardComponent {
  private housekeepingApi = inject(HousekeepingApiService);

  config: TaskDashboardConfig = {
    entityName: 'Housekeeping Task',
    fetchTasks: (params: any) =>
      this.housekeepingApi.getAll(params).pipe(
        map((res: any) => ({
          totalCount: res.totalCount,
          data: res.data.map(
            (task: HousekeepingTask) =>
              ({
                id: task.id,
                status: task.status, // Pending, InProgress, Completed
                location: task.location || `Room ${task.roomId}`,
                description: task.description || 'No description provided.',
                createdAt: task.createdAt,
                raw: task,
              } as Task)
          ),
        }))
      ),
    updateTaskStatus: (id: number, newStatus: string) =>
      this.housekeepingApi.updateStatus(id, { status: newStatus }),
    statusOptions: [
      { value: 'All', label: 'All' },
      { value: 'Pending', label: 'Pending' },
      { value: 'InProgress', label: 'In Progress' },
      { value: 'Completed', label: 'Completed' },
    ],
    getLocation: (t: Task) => t.location,
    getDescription: (t: Task) => t.description,
    getDetailSections: (t: Task) => {
      const task = t.raw as HousekeepingTask;
      return [
        {
          title: 'Task Details',
          fields: [
            { label: 'Task ID', value: String(task.id) },
            { label: 'Room ID', value: task.roomId ? String(task.roomId) : 'N/A' },
            { label: 'Location', value: task.location || 'N/A' },
            { label: 'Origin Type', value: task.originType },
            { label: 'Status', value: task.status },
            { label: 'Description', value: task.description || 'N/A' },
            {
              label: 'Created At',
              value: task.createdAt ? new Date(task.createdAt).toLocaleString() : 'N/A',
            },
            {
              label: 'Started At',
              value: task.startedAt ? new Date(task.startedAt).toLocaleString() : 'N/A',
            },
            {
              label: 'Finished At',
              value: task.finishedAt ? new Date(task.finishedAt).toLocaleString() : 'N/A',
            },
          ],
        },
      ] as DetailSection[];
    },
  };
}
