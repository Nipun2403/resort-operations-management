import { Component, input, output, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { OrderItem } from '../../models/order-item.model';

@Component({
  selector: 'app-cart-drawer',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule],
  templateUrl: './cart-drawer.component.html',
  styleUrls: ['./cart-drawer.component.scss']
})
export class CartDrawerComponent {
  cartItems = input.required<OrderItem[]>();
  isOpen = input.required<boolean>();

  cartToggle = output<void>();
  checkout = output<void>();
  updateQuantity = output<{ menuItemId: number; delta: number }>();

  itemCount = computed(() => this.cartItems().reduce((s, i) => s + i.quantity, 0));
  subtotal = computed(() => this.cartItems().reduce((s, i) => s + i.price * i.quantity, 0));

  incrementQty(menuItemId: number): void {
    this.updateQuantity.emit({ menuItemId, delta: 1 });
  }

  decrementQty(menuItemId: number): void {
    this.updateQuantity.emit({ menuItemId, delta: -1 });
  }
}
