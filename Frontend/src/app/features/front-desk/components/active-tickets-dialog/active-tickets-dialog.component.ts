import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatTabsModule } from '@angular/material/tabs';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { TicketListComponent } from '../ticket-list/ticket-list.component';

@Component({
  selector: 'app-active-tickets-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatTabsModule,
    MatButtonModule,
    MatIconModule,
    TicketListComponent,
  ],
  templateUrl: './active-tickets-dialog.component.html',
  styleUrls: ['./active-tickets-dialog.component.scss'],
})
export class ActiveTicketsDialogComponent {
  data = inject<{
    housekeepingCount: number;
    maintenanceCount: number;
    foodOrdersCount: number;
  }>(MAT_DIALOG_DATA);
}
