import { Component, Inject, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { MatIconModule } from '@angular/material/icon';

import { DetailSection } from '../../models/task.model';

@Component({
  selector: 'app-task-detail-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatDividerModule,
    MatIconModule,
  ],
  templateUrl: './task-detail-dialog.component.html',
  styleUrls: ['./task-detail-dialog.component.scss'],
})
export class TaskDetailDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<TaskDetailDialogComponent>);

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: {
      task: any;
      detailSections: DetailSection[];
      canStart: boolean;
      canComplete: boolean;
      inProgressStatus: string;
      completedStatus: string;
    }
  ) {}

  start(): void {
    this.dialogRef.close({ newStatus: this.data.inProgressStatus });
  }

  complete(): void {
    this.dialogRef.close({ newStatus: this.data.completedStatus });
  }
}
