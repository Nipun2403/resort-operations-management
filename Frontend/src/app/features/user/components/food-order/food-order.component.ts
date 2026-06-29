import { Component, OnInit, inject, signal, computed, input, output, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl, Validators } from '@angular/forms';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { ConfirmDialogComponent } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MenuItemApiService } from '../../services/menu-item-api.service';
import { OrderApiService } from '../../services/order-api.service';
import { MenuGridComponent } from './menu-grid.component';
import { CartDrawerComponent } from './cart-drawer.component';
import { MenuItem } from '../../../../features/admin/models/menu-item.model';
import { BookingRoom } from '../../../../features/admin/models/booking.model';
import { OrderItem } from '../../models/order-item.model';
import { finalize } from 'rxjs/operators';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AlertComponent } from '../../../../features/auth/components/alert.component';

@Component({
  selector: 'app-food-order',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    MatDialogModule,
    MatFormFieldModule,
    MatSelectModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MenuGridComponent,
    CartDrawerComponent,
    AlertComponent
  ],
  templateUrl: './food-order.component.html',
  styleUrls: ['./food-order.component.scss']
})
export class FoodOrderComponent implements OnInit {
  activeBookingId = input.required<number>();
  rooms = input.required<BookingRoom[]>();
  orderPlaced = output<void>();

  private readonly menuApi = inject(MenuItemApiService);
  private readonly orderApi = inject(OrderApiService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly dialog = inject(MatDialog);
  private readonly destroyRef = inject(DestroyRef);

  selectedRoomId = new FormControl<number>(0, { nonNullable: true, validators: Validators.required });

  menuItems = signal<MenuItem[]>([]);
  cartItems = signal<OrderItem[]>([]);
  cartOpen = signal(false);

  loading = signal(false);
  error = signal<string | null>(null);
  submitting = signal(false);

  validRooms = computed(() => this.rooms().filter((r): r is typeof r & { roomId: number } => r.roomId !== null));
  canCheckout = computed(() => this.cartItems().length > 0);
  subtotal = computed(() => this.cartItems().reduce((s, i) => s + i.price * i.quantity, 0));

  ngOnInit(): void {
    this.fetchMenuItems();
    const roomsList = this.validRooms();
    if (roomsList.length > 0) {
      this.selectedRoomId.setValue(roomsList[0].roomId);
    }
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

  onUpdateCartQty(event: { menuItemId: number; delta: number }): void {
    this.cartItems.update(items => {
      const index = items.findIndex(i => i.menuItemId === event.menuItemId);
      if (index === -1) return items;
      const newQty = items[index].quantity + event.delta;
      if (newQty <= 0) {
        return items.filter(i => i.menuItemId !== event.menuItemId);
      }
      return items.map(i => i.menuItemId === event.menuItemId ? { ...i, quantity: newQty } : i);
    });
  }

  placeOrder(): void {
    if (!this.canCheckout() || this.submitting()) {
      return;
    }
    if (this.selectedRoomId.invalid) {
      this.selectedRoomId.markAsTouched();
      return;
    }

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Confirm Order',
        message: `Place this order? Total: $${this.subtotal().toFixed(2)}`
      }
    });

    dialogRef.afterClosed().pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe((confirmed) => {
      if (confirmed) {
        this.submitOrder();
      }
    });
  }

  private submitOrder(): void {
    if (this.selectedRoomId.invalid) {
      this.selectedRoomId.markAsTouched();
      return;
    }
    this.submitting.set(true);
    const dto = {
      bookingId: this.activeBookingId(),
      roomId: this.selectedRoomId.value,
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
          const msg = typeof err.error === 'string' ? err.error : (err.error?.message || 'Failed to place order.');
          this.snackBar.open(msg, 'Close', { duration: 5000 });
        }
      });
  }
}
