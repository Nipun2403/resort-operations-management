import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDividerModule } from '@angular/material/divider';

import { Booking } from '../../../../admin/models/booking.model';
import { FoodOrderPanelComponent } from '../food-order-panel/food-order-panel.component';
import { HousekeepingRequestPanelComponent } from '../housekeeping-request-panel/housekeeping-request-panel.component';
import { MaintenanceRequestPanelComponent } from '../maintenance-request-panel/maintenance-request-panel.component';
@Component({
  selector: 'app-room-service-tab',
  standalone: true,
  imports: [
    CommonModule,
    MatDividerModule,
    FoodOrderPanelComponent,
    HousekeepingRequestPanelComponent,
    MaintenanceRequestPanelComponent,
  ],
  templateUrl: './room-service-tab.component.html',
  styleUrls: ['./room-service-tab.component.scss'],
})
export class RoomServiceTabComponent {
  booking = input.required<Booking>();
}
