import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { ColumnDef } from '../../../models/crud-config.model';

@Component({
  selector: 'app-cards-view',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatButtonModule, MatIconModule],
  templateUrl: './cards-view.component.html',
  styleUrls: ['./cards-view.component.scss'],
})
export class CardsViewComponent {
  data = input.required<any[]>();
  columns = input.required<ColumnDef[]>();
  edit = output<any>();
}
