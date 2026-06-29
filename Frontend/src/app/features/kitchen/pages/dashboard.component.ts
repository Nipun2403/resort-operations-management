import { Component, inject } from '@angular/core';
import { map } from 'rxjs/operators';
import { TaskDashboardComponent } from '../../../shared/components/task-dashboard/task-dashboard.component';
import { TaskDashboardConfig, Task, DetailSection } from '../../../shared/models/task.model';
import { OrderApiService } from '../../user/services/order-api.service';

@Component({
  selector: 'app-kitchen-dashboard',
  standalone: true,
  imports: [TaskDashboardComponent],
  template: `<app-task-dashboard [config]="config" />`,
})
export class KitchenDashboardComponent {
  private orderApi = inject(OrderApiService);

  config: TaskDashboardConfig = {
    entityName: 'Food Order',
    fetchTasks: (params: any) =>
      this.orderApi.getAll(params).pipe(
        map((res: any) => ({
          totalCount: res.totalCount,
          data: res.data.map(
            (order: any) =>
              ({
                id: order.id,
                status: order.orderStatus ?? 'Pending',
                location: order.roomNumber ?? (order.roomId ? `Room ${order.roomId}` : 'N/A'),
                description: `Order #${order.id}`,
                createdAt: order.generatedAt ?? '',
                raw: order,
              } as Task)
          ),
        }))
      ),
    updateTaskStatus: (id: number, newStatus: string) =>
      this.orderApi.updateStatus(id, { status: newStatus }),
    statusOptions: [
      { value: 'All', label: 'All' },
      { value: 'Pending', label: 'Pending' },
      { value: 'Preparing', label: 'Preparing' },
      { value: 'Delivered', label: 'Delivered' },
    ],
    getLocation: (t: Task) => t.location,
    getDescription: (t: Task) => t.description,
    getDetailSections: (t: Task) => {
      const order = t.raw as any;
      const itemsArray = order.orderItems || [];
      const items = itemsArray.length > 0
        ? itemsArray.map((i: any) => `${i.quantity}x ${i.menuItemName ?? 'Item #' + i.menuItemId}`).join(', ')
        : 'None';
      return [
        {
          title: 'Order Information',
          fields: [
            { label: 'Order ID', value: String(order.id) },
            { label: 'Status', value: t.status },
            { label: 'Room', value: t.location },
            { label: 'Items', value: items },
            {
              label: 'Created At',
              value: t.createdAt ? new Date(t.createdAt).toLocaleString() : 'N/A',
            },
          ],
        },
      ] as DetailSection[];
    },
  };
}
