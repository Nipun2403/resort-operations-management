import { Component, input, inject, signal, computed, OnInit, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl, Validators } from '@angular/forms';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs/operators';

import { MenuGridComponent } from '../../../../user/components/food-order/menu-grid.component';
import { CartDrawerComponent } from '../../../../user/components/food-order/cart-drawer.component';
import { OrderApiService } from '../../../../user/services/order-api.service';
import { MenuItemApiService } from '../../../../user/services/menu-item-api.service';
import { MenuItem } from '../../../../admin/models/menu-item.model';
import { BookingRoom } from '../../../../admin/models/booking.model';
import { OrderItem } from '../../../../user/models/order-item.model';
import { ConfirmDialogComponent } from '../../../../../shared/components/confirm-dialog/confirm-dialog.component';
import { AlertComponent } from '../../../../auth/components/alert.component';

@Component({
  selector: 'app-food-order-panel',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MenuGridComponent,
    CartDrawerComponent,
    MatSnackBarModule,
    MatDialogModule,
    MatProgressSpinnerModule,
    MatButtonModule,
    MatFormFieldModule,
    MatSelectModule,
    MatInputModule,
    AlertComponent,
  ],
  templateUrl: './food-order-panel.component.html',
})
export class FoodOrderPanelComponent implements OnInit {
  bookingId = input.required<number>();
  rooms = input.required<BookingRoom[]>();

  menuItems = signal<MenuItem[]>([]);
  cartItems = signal<OrderItem[]>([]);
  cartOpen = signal(false);
  loading = signal(false);
  error = signal<string | null>(null);

  selectedRoomId = new FormControl<number>(0, { nonNullable: true, validators: Validators.required });

  private menuItemApi = inject(MenuItemApiService);
  private orderApi = inject(OrderApiService);
  private snackBar = inject(MatSnackBar);
  private dialog = inject(MatDialog);
  private destroyRef = inject(DestroyRef);

  validRooms = computed(() => this.rooms().filter((r): r is typeof r & { roomId: number } => r.roomId !== null));

  ngOnInit(): void {
    this.loadMenu();
    const roomsList = this.validRooms();
    if (roomsList.length > 0) {
      this.selectedRoomId.setValue(roomsList[0].roomId);
    }
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
    if (this.selectedRoomId.invalid) {
      this.selectedRoomId.markAsTouched();
      return;
    }

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
            roomId: this.selectedRoomId.value,
            items: this.cartItems().map(i => ({ menuItemId: i.menuItemId, quantity: i.quantity })),
          })
          .pipe(takeUntilDestroyed(this.destroyRef))
          .subscribe({
            next: () => {
              this.snackBar.open('Order placed successfully', 'Close', { duration: 3000 });
              this.cartItems.set([]); // clear cart
            },
            error: (err: any) => {
              const msg = typeof err.error === 'string' ? err.error : (err.error?.message || 'Failed to place order.');
              this.snackBar.open(msg, 'Close', { duration: 5000 });
            },
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
