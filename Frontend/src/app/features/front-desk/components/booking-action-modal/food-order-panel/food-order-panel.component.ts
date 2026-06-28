import { Component, input, inject, signal, OnInit, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonModule } from '@angular/material/button';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs/operators';

import { MenuGridComponent } from '../../../../user/components/food-order/menu-grid.component';
import { CartDrawerComponent } from '../../../../user/components/food-order/cart-drawer.component';
import { OrderApiService } from '../../../../user/services/order-api.service';
import { MenuItemApiService } from '../../../../user/services/menu-item-api.service';
import { MenuItem } from '../../../../admin/models/menu-item.model';
import { OrderItem } from '../../../../user/models/order-item.model';
import { ConfirmDialogComponent } from '../../../../../shared/components/confirm-dialog/confirm-dialog.component';
import { AlertComponent } from '../../../../auth/components/alert.component';

@Component({
  selector: 'app-food-order-panel',
  standalone: true,
  imports: [
    CommonModule,
    MenuGridComponent,
    CartDrawerComponent,
    MatSnackBarModule,
    MatDialogModule,
    MatProgressSpinnerModule,
    MatButtonModule,
    AlertComponent,
  ],
  templateUrl: './food-order-panel.component.html',
})
export class FoodOrderPanelComponent implements OnInit {
  bookingId = input.required<number>();

  menuItems = signal<MenuItem[]>([]);
  cartItems = signal<OrderItem[]>([]);
  cartOpen = signal(false);
  loading = signal(false);
  error = signal<string | null>(null);

  private menuItemApi = inject(MenuItemApiService);
  private orderApi = inject(OrderApiService);
  private snackBar = inject(MatSnackBar);
  private dialog = inject(MatDialog);
  private destroyRef = inject(DestroyRef);

  ngOnInit(): void {
    this.loadMenu();
  }

  loadMenu(): void {
    this.loading.set(true);
    this.error.set(null);
    this.menuItemApi
      .getAll({ isAvailable: true, pageSize: 200 })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.loading.set(false))
      )
      .subscribe({
        next: res => this.menuItems.set(res.data),
        error: (err: any) => this.error.set(this.extractErrorMessage(err)),
      });
  }

  onAddToCart(item: MenuItem): void {
    this.cartItems.update(items => {
      const existing = items.find(i => i.menuItemId === item.id);
      if (existing) {
        return items.map(i => (i.menuItemId === item.id ? { ...i, quantity: i.quantity + 1 } : i));
      }
      return [...items, { menuItemId: item.id, name: item.name, price: item.price, quantity: 1 }];
    });
    this.snackBar
      .open(`${item.name} added to cart`, 'View Cart', { duration: 2000 })
      .onAction()
      .subscribe(() => {
        this.cartOpen.set(true);
      });
  }

  onUpdateCartQty(event: { menuItemId: number; delta: number }): void {
    this.cartItems.update(items => {
      return items
        .map(i => (i.menuItemId === event.menuItemId ? { ...i, quantity: Math.max(0, i.quantity + event.delta) } : i))
        .filter(i => i.quantity > 0);
    });
  }

  placeOrder(): void {
    if (this.cartItems().length === 0) return;
    const confirmRef = this.dialog.open(ConfirmDialogComponent, {
      data: { title: 'Confirm Order', message: 'Place this food order for the guest?' },
    });
    confirmRef
      .afterClosed()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(confirmed => {
        if (!confirmed) return;
        this.orderApi
          .create({
            bookingId: this.bookingId(),
            items: this.cartItems().map(i => ({ menuItemId: i.menuItemId, quantity: i.quantity })),
          })
          .pipe(takeUntilDestroyed(this.destroyRef))
          .subscribe({
            next: () => {
              this.snackBar.open('Order placed successfully', 'Close', { duration: 3000 });
              this.cartItems.set([]); // clear cart
            },
            error: (err: any) => this.snackBar.open(this.extractErrorMessage(err), 'Close', { duration: 5000 }),
          });
      });
  }

  private extractErrorMessage(err: any): string {
    if (typeof err === 'string') return err;
    if (err?.error?.message) return err.error.message;
    if (err?.message) return err.message;
    return 'An unexpected error occurred.';
  }
}
