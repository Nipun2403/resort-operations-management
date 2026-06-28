import { Component, input, output, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MenuItem } from '../../../../features/admin/models/menu-item.model';
import { OrderItem } from '../../models/order-item.model';

@Component({
  selector: 'app-menu-grid',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatSelectModule,
    MatFormFieldModule
  ],
  templateUrl: './menu-grid.component.html',
  styleUrls: ['./menu-grid.component.scss']
})
export class MenuGridComponent {
  menuItems = input.required<MenuItem[]>();
  cartItems = input<OrderItem[]>([]);
  addToCart = output<MenuItem>();
  updateQuantity = output<{ menuItemId: number; delta: number }>();

  categoryFilter = new FormControl('All', { nonNullable: true });

  cartMap = computed(() => {
    const map: Record<number, number> = {};
    const items = this.cartItems() || [];
    for (const item of items) {
      map[item.menuItemId] = item.quantity;
    }
    return map;
  });

  getQuantity(menuItemId: number): number {
    return this.cartMap()[menuItemId] || 0;
  }

  increment(item: MenuItem): void {
    const current = this.getQuantity(item.id);
    if (current === 0) {
      this.addToCart.emit(item);
    } else {
      this.updateQuantity.emit({ menuItemId: item.id, delta: 1 });
    }
  }

  decrement(item: MenuItem): void {
    const current = this.getQuantity(item.id);
    if (current > 0) {
      this.updateQuantity.emit({ menuItemId: item.id, delta: -1 });
    }
  }

  categories = computed(() => {
    const cats = new Set(this.menuItems().map(i => i.category || 'Other'));
    return Array.from(cats).sort();
  });

  filteredGroups = computed(() => {
    const selected = this.categoryFilter.value;
    const items = selected === 'All' 
      ? this.menuItems() 
      : this.menuItems().filter(i => (i.category || 'Other') === selected);
    
    const groups: Record<string, MenuItem[]> = {};
    for (const item of items) {
      const cat = item.category || 'Other';
      if (!groups[cat]) groups[cat] = [];
      groups[cat].push(item);
    }
    return Object.entries(groups).map(([category, items]) => ({ category, items }));
  });
}
