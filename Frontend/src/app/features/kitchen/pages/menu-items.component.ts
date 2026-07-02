import {
  Component,
  inject,
  signal,
  computed,
  OnInit,
  DestroyRef,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { debounceTime, distinctUntilChanged, finalize } from 'rxjs';

import { MenuItemApiService } from '../../admin/services/menu-item-api.service';
import { MenuItem } from '../../admin/models/menu-item.model';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { AlertComponent } from '../../auth/components/alert.component';

@Component({
  selector: 'app-kitchen-menu-items',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatSlideToggleModule,
    MatFormFieldModule,
    MatSelectModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    AlertComponent,
  ],
  templateUrl: './menu-items.component.html',
  styleUrls: ['./menu-items.component.scss'],
})
export class KitchenMenuItemsComponent implements OnInit {
  private menuItemApi = inject(MenuItemApiService);
  private snackBar = inject(MatSnackBar);
  private dialog = inject(MatDialog);
  private destroyRef = inject(DestroyRef);

  menuItems = signal<MenuItem[]>([]);
  filteredItems = signal<MenuItem[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);
  categoryFilter = new FormControl('All', { nonNullable: true });
  searchControl = new FormControl('', { nonNullable: true });

  categories = computed(() => {
    const cats = new Set(this.menuItems().map((i) => i.category || 'Other'));
    return Array.from(cats).sort();
  });

  ngOnInit(): void {
    this.fetchMenuItems();
    this.searchControl.valueChanges
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(() => this.applyFilters());
  }

  fetchMenuItems(): void {
    this.loading.set(true);
    this.error.set(null);
    this.menuItemApi
      .getAll({ pageNumber: 1, pageSize: 200, sortBy: 'id', sortDescending: false, isAvailable: undefined })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.loading.set(false))
      )
      .subscribe({
        next: (res) => {
          this.menuItems.set(res.data);
          this.applyFilters();
        },
        error: (err: any) => this.error.set(this.extractErrorMessage(err)),
      });
  }

  applyFilters(): void {
    const category = this.categoryFilter.value;
    const search = this.searchControl.value.toLowerCase();
    let items = this.menuItems();
    if (category !== 'All') {
      items = items.filter((i) => (i.category || 'Other') === category);
    }
    if (search) {
      items = items.filter((i) => i.name.toLowerCase().includes(search));
    }
    this.filteredItems.set(items);
  }

  getItemsByCategory(category: string): MenuItem[] {
    return this.filteredItems().filter((i) => (i.category || 'Other') === category);
  }

  getItemImage(item: MenuItem): string {
    return item.imageUrl ?? '';
  }

  getItemDescription(item: MenuItem): string {
    if (item.description) {
      return item.description;
    }
    const name = item.name.toLowerCase();
    if (name.includes('wagyu') || name.includes('striploin')) {
      return 'Grade A5 Miyazaki beef, truffle-infused reduction, bone marrow emulsion, and aged balsamic glass.';
    }
    if (name.includes('caviar')) {
      return 'Sustainably sourced royal pearls, traditional accompaniments, and crème fraîche on buckwheat blinis.';
    }
    if (name.includes('bisque')) {
      return 'Slow-simmered brandy reduction, heavy cream, poached lobster medallion, and chive oil drops.';
    }
    if (name.includes('risotto') || name.includes('truffle')) {
      return 'Acquerello rice, Périgord black truffles, 36-month aged parmesan, and clarified heirloom butter.';
    }
    if (name.includes('sphere') || name.includes('chocolate')) {
      return 'Dark chocolate mirror glaze, salted caramel core, and 24k edible gold leaf gilding.';
    }
    if (name.includes('scallop')) {
      return 'Diver-caught scallops, heritage nero di seppia, and a delicate emulsification of champagne-infused butter.';
    }
    if (name.includes('tartare')) {
      return 'Hand-cut premium tenderloin, truffle shavings, organic yolk, served in bone marrow.';
    }
    // Generic fallback
    return `A masterfully prepared ${item.category.toLowerCase() || 'offering'} crafted with seasonal ingredients and culinary precision.`;
  }

  applyFiltersDebounced(): void {
    // Value changes debounced handled in ngOnInit.
  }

  onToggleAvailability(item: MenuItem, newValue: boolean): void {
    if (!newValue) {
      const dialogRef = this.dialog.open(ConfirmDialogComponent, {
        data: {
          title: 'Disable Menu Item',
          message: `Are you sure you want to make "${item.name}" unavailable?`,
        },
      });
      dialogRef
        .afterClosed()
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe((confirmed) => {
          if (confirmed) {
            this.updateAvailability(item, false);
          } else {
            this.menuItems.update((items) =>
              items.map((i) =>
                i.id === item.id ? { ...i, isAvailable: true } : i
              )
            );
            this.applyFilters();
          }
        });
    } else {
      this.updateAvailability(item, true);
    }
  }

  private updateAvailability(item: MenuItem, isAvailable: boolean): void {
    this.menuItemApi
      .updateStatus(item.id, isAvailable)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.snackBar.open(
            `"${item.name}" is now ${isAvailable ? 'available' : 'unavailable'}.`,
            'Close',
            { duration: 3000 }
          );
          this.menuItems.update((items) =>
            items.map((i) => (i.id === item.id ? { ...i, isAvailable } : i))
          );
          this.applyFilters();
        },
        error: (err: any) => {
          this.snackBar.open(this.extractErrorMessage(err), 'Close', {
            duration: 5000,
          });
          this.menuItems.update((items) =>
            items.map((i) =>
              i.id === item.id ? { ...i, isAvailable: !isAvailable } : i
            )
          );
          this.applyFilters();
        },
      });
  }

  private extractErrorMessage(err: any): string {
    if (typeof err === 'string') return err;
    if (err?.error?.message) return err.error.message;
    if (err?.message) return err.message;
    return 'An unexpected error occurred.';
  }
}
