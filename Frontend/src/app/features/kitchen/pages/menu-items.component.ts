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
    if (item.image) {
      return item.image;
    }
    const name = item.name.toLowerCase();
    if (name.includes('wagyu') || name.includes('striploin') || name.includes('steak')) {
      return 'https://lh3.googleusercontent.com/aida-public/AB6AXuBarygmlUbYo8_hlFV_fJMXvnOBX11DAPb6pFwLKXt-UyvvfcRkXRLI7NLY4vrOGCLbwkoe7ZpGDBeNa90_Jk4Kxz0LYwp325FxskDEOBmeVdCh-Gc-w8z_phG2K_oB0Diva182Aj7alzLE-MINpn-U3GwNYJIL-K9yhQfiH28wig-ZX6piiudQ0FgqhrZ4znrPZNN4MBnt5d8bPf0RTnnoIaw1i8WPoNqRgvHGwbw1MZhrx6RTFFhJgtgWV2mesULJsOXkEM_8veyQ';
    }
    if (name.includes('caviar')) {
      return 'https://lh3.googleusercontent.com/aida-public/AB6AXuDbY-HsrCSZXj1MKOoSnNdnY5O2eUG8bqeeY6wE1v1AFQWyeBf60FU3L241aD_fr1Y4Gwvb8uZVqM-aGb5QUfAcuoE-WMoWu-ERyDrOYrixqeS0diuXKvgQEliBnkElMrDMvejOWsaijV3VgCBsNuXIyZuWAJvnqkVmN_Equ_eo4EwwzoX8-vmEauzdIxZdK5g-C6UFhjn9FpTwGwbz46_1QSPmIWi-MdF0aRI-J3lwIPc4i-HnWWlJ90_wVtbRjupmZud7T_ge4gPb';
    }
    if (name.includes('bisque') || name.includes('soup') || name.includes('lobster') || name.includes('scallop')) {
      return 'https://lh3.googleusercontent.com/aida-public/AB6AXuB_rNdyvU6w7p3bpV_1UHUu0WKdiBoWXFLgJ4jyj5Vv0Ci-KniE-Aele9ikC2gHQF0npuqBzzhh2vxgodvD4VOqcOXl7krYZrqlvN0e9O-vnf9_ToEe4aQMmq8YbwYizuPBiILGON-VmY4WIr7DNZHETG1IlDGA2F0IhezgYHibR0KdRt9zANheHQoZHYRTcJ138DF3lA1u6wMdFbh-zxi-HTv2tmb536kGG33-t8hyD9yITQz3Uux4rDfl49DoG6X5xLpA3hywZRin';
    }
    if (name.includes('risotto') || name.includes('rice') || name.includes('pasta')) {
      return 'https://lh3.googleusercontent.com/aida-public/AB6AXuAI0KfDRcQRyuF1wkwmNaOAyfL6nx9spuyEJ6sJ7tcvDXPUZQEc8xmKak1HgUJKLb9JMFjm1YHiZ_Hf-nMiS7Iwv7a0oShuNWqfmqzYEbQknpdotT93Y9JpSgjcb5dxQgnGCjf87dBUjxtpq6HXARRdQSkg5fc2G2igtgjEqT_kl6UmMRRfBrKdPshz6dt47OvyKtEnigHu63AiUm7R12iAdhuBFqrntVFOoeuJdxsmTH7AXFWMZBtgEU1o1gPazO_E32Yi5JjEEihD';
    }
    if (name.includes('sphere') || name.includes('chocolate') || name.includes('dessert') || name.includes('cake') || name.includes('sweet')) {
      return 'https://lh3.googleusercontent.com/aida-public/AB6AXuCf0ldh_BjvPjh_ENynZu6kzl-R8VqbIj8Gb_9RUTI18d2fUGgn6datoLhDKACDl2BhJcMQl7D69JzHkvAHyvyJYPLeH8GyF4cc0X0A54UDhgNSMBQniwl4qnfgrG3BetDbu0CxVjmvgRJ6CU4LUhikNjKvZbBQhXEdzq-5g2RcqsKgg77Q21Iw9dRw_TJ8qat5J6LUX8GDZYgmrPNZAxwa8k1wp2rs6zqc4NZTLUCx_OE5QKrDEi2n5aQ4Z9clCz8U6JzzCg2HQoR8';
    }
    if (name.includes('tartare') || name.includes('marrow')) {
      return 'https://lh3.googleusercontent.com/aida-public/AB6AXuDZCG6JCdByJIipxOKLrSdwwusboiQiUtXql3LdVMpOWORaCGWnyPbEdH4IFBHhwiCLjhtySz9kk3XbDV1PwrJgD8ytR2MHO2GjpHCtXIWQlM7rVwC9rTLdddzwxoJNieibZ8H5Sih9aYGe9nHdpQePFXxgbNSxD6hkfAnUhZJMK9YUJd-caQUlBaM8-lJRL_fjf4xss6aBTLRAT4RfiKBuly5tCBQATOluwll79upYF29FtJpDY9b32jRJPSXptVE7lgkZCP7uBNWP';
    }
    
    // Default fallback image
    return 'https://lh3.googleusercontent.com/aida-public/AB6AXuBarygmlUbYo8_hlFV_fJMXvnOBX11DAPb6pFwLKXt-UyvvfcRkXRLI7NLY4vrOGCLbwkoe7ZpGDBeNa90_Jk4Kxz0LYwp325FxskDEOBmeVdCh-Gc-w8z_phG2K_oB0Diva182Aj7alzLE-MINpn-U3GwNYJIL-K9yhQfiH28wig-ZX6piiudQ0FgqhrZ4znrPZNN4MBnt5d8bPf0RTnnoIaw1i8WPoNqRgvHGwbw1MZhrx6RTFFhJgtgWV2mesULJsOXkEM_8veyQ';
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
