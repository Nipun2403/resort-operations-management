import { Component, OnInit, inject, signal, computed, input, output, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MenuItemApiService } from '../../services/menu-item-api.service';
import { OrderApiService } from '../../services/order-api.service';
import { MenuGridComponent } from './menu-grid.component';
import { CartDrawerComponent } from './cart-drawer.component';
import { MenuItem } from '../../../../features/admin/models/menu-item.model';
import { OrderItem } from '../../models/order-item.model';
import { finalize } from 'rxjs/operators';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AlertComponent } from '../../../../features/auth/components/alert.component';

@Component({
  selector: 'app-food-order',
  standalone: true,
  imports: [
    CommonModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    MenuGridComponent,
    CartDrawerComponent,
    AlertComponent
  ],
  templateUrl: './food-order.component.html',
  styleUrls: ['./food-order.component.scss']
})
export class FoodOrderComponent implements OnInit {
  activeBookingId = input.required<number>();
  orderPlaced = output<void>();

  private readonly menuApi = inject(MenuItemApiService);
  private readonly orderApi = inject(OrderApiService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly destroyRef = inject(DestroyRef);

  menuItems = signal<MenuItem[]>([]);
  cartItems = signal<OrderItem[]>([]);
  cartOpen = signal(false);

  loading = signal(false);
  error = signal<string | null>(null);
  submitting = signal(false);

  canCheckout = computed(() => this.cartItems().length > 0);

  ngOnInit(): void {
    this.fetchMenuItems();
  }

  fetchMenuItems(): void {
    this.loading.set(true);
    this.error.set(null);

    this.menuApi.getAll({ isAvailable: true, pageSize: 200 })
      .pipe(
        finalize(() => this.loading.set(false)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: (res) => this.menuItems.set(res.data),
        error: (err) => {
          const msg = err.error?.message || err.message || 'Failed to load menu items.';
          this.error.set(msg);
        }
      });
  }

  onAddToCart(item: MenuItem): void {
    this.cartItems.update((items) => {
      const idx = items.findIndex((i) => i.menuItemId === item.id);
      if (idx > -1) {
        const updated = [...items];
        updated[idx] = {
          ...updated[idx],
          quantity: updated[idx].quantity + 1
        };
        return updated;
      } else {
        return [...items, { menuItemId: item.id, name: item.name, price: item.price, quantity: 1 }];
      }
    });

    const snackRef = this.snackBar.open(`Added ${item.name} to cart.`, 'View Cart', {
      duration: 4000
    });

    snackRef.onAction().subscribe(() => {
      this.cartOpen.set(true);
    });
  }

  placeOrder(): void {
    if (!this.canCheckout() || this.submitting()) {
      return;
    }

    this.submitting.set(true);
    const dto = {
      bookingId: this.activeBookingId(),
      items: this.cartItems().map((i) => ({
        menuItemId: i.menuItemId,
        quantity: i.quantity
      }))
    };

    this.orderApi.create(dto)
      .pipe(
        finalize(() => this.submitting.set(false)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: () => {
          this.snackBar.open('Order placed successfully!', 'Close', { duration: 4000 });
          this.cartItems.set([]);
          this.cartOpen.set(false);
          this.orderPlaced.emit();
        },
        error: (err) => {
          const msg = err.error?.message || err.message || 'Failed to place order.';
          this.snackBar.open(msg, 'Close', { duration: 5000 });
        }
      });
  }
}
