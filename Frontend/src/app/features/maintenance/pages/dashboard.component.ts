import { Component, inject, signal, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { map } from 'rxjs/operators';
import { TaskDashboardComponent } from '../../../shared/components/task-dashboard/task-dashboard.component';
import { TaskDashboardConfig, Task, DetailSection } from '../../../shared/models/task.model';
import { MaintenanceApiService } from '../../user/services/maintenance-api.service';
import { MaintenanceTask } from '../../admin/models/maintenance-task.model';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-maintenance-dashboard',
  standalone: true,
  imports: [TaskDashboardComponent],
  template: `<app-task-dashboard [config]="config" [refresh]="refreshTrigger()" />`,
})
export class MaintenanceDashboardComponent {
  private maintenanceApi = inject(MaintenanceApiService);
  private notificationService = inject(NotificationService);
  private destroyRef = inject(DestroyRef);

  refreshTrigger = signal(0);

  constructor() {
    this.notificationService.startConnection();

    this.notificationService.onAlert
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(notification => {
        this.refreshTrigger.update(n => n + 1);
        this.notificationService.showNotification('New Task', notification.description);
      });
  }

  config: TaskDashboardConfig = {
    entityName: 'Maintenance Task',
    fetchTasks: (params: any) =>
      this.maintenanceApi.getAll(params).pipe(
        map((res: any) => ({
          totalCount: res.totalCount,
          data: res.data.map(
            (task: MaintenanceTask) =>
              ({
                id: task.id,
                status: task.status, // Pending, InProgress, Completed
                location: task.location || `Room ${task.roomId}`,
                description: task.description || 'No description provided.',
                createdAt: task.createdAt,
                isEmergency: task.isEmergency,
                raw: task,
              } as Task)
          ),
        }))
      ),
    updateTaskStatus: (id: number, newStatus: string) =>
      this.maintenanceApi.updateStatus(id, { status: newStatus }),
    statusOptions: [
      { value: 'All', label: 'All' },
      { value: 'Pending', label: 'Pending' },
      { value: 'InProgress', label: 'In Progress' },
      { value: 'Completed', label: 'Completed' },
    ],
    getLocation: (t: Task) => t.location,
    getDescription: (t: Task) => t.description,
    getDetailSections: (t: Task) => {
      const task = t.raw as MaintenanceTask;
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
