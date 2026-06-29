import { Component, inject, signal, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { map } from 'rxjs/operators';
import { TaskDashboardComponent } from '../../../shared/components/task-dashboard/task-dashboard.component';
import { TaskDashboardConfig, Task, DetailSection } from '../../../shared/models/task.model';
import { HousekeepingApiService } from '../../user/services/housekeeping-api.service';
import { HousekeepingTask } from '../../admin/models/housekeeping-task.model';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-housekeeping-dashboard',
  standalone: true,
  imports: [TaskDashboardComponent],
  template: `<app-task-dashboard [config]="config" [refresh]="refreshTrigger()" />`,
})
export class HousekeepingDashboardComponent {
  private housekeepingApi = inject(HousekeepingApiService);
  private notificationService = inject(NotificationService);
  private destroyRef = inject(DestroyRef);

  refreshTrigger = signal(0);

  constructor() {
    this.notificationService.startConnection();

    this.notificationService.onNewHousekeepingTask
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(task => {
        this.refreshTrigger.update(n => n + 1);
        this.notificationService.showNotification(
          'New Housekeeping Task',
          `${task.description}${task.roomNumber ? ' – Room ' + task.roomNumber : ''}`
        );
      });
  }

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
