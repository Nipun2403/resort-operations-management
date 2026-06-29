import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DestroyRef } from '@angular/core';
import { finalize } from 'rxjs';
import { MenuItemApiService } from '../../admin/services/menu-item-api.service';
import { MenuItem } from '../../admin/models/menu-item.model';

@Component({
  selector: 'app-public-menu',
  standalone: true,
  imports: [
    CommonModule, MatCardModule, MatIconModule, MatProgressSpinnerModule
  ],
  templateUrl: './menu.component.html',
  styleUrls: ['./menu.component.scss']
})
export class MenuComponent implements OnInit {
  private menuItemApi = inject(MenuItemApiService);
  private destroyRef = inject(DestroyRef);

  groupedMenu = signal<{ category: string; items: MenuItem[] }[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);

  ngOnInit(): void {
    this.fetchMenu();
  }

  fetchMenu(): void {
    this.loading.set(true);
    this.menuItemApi.getAll({ isAvailable: true, pageNumber: 1, pageSize: 200, sortBy: 'name', sortDescending: false }).pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.loading.set(false))
    ).subscribe({
      next: res => {
        const groups: Record<string, MenuItem[]> = {};
        for (const item of res.data) {
          const cat = item.category || 'Other';
          if (!groups[cat]) groups[cat] = [];
          groups[cat].push(item);
        }
        this.groupedMenu.set(
          Object.entries(groups).map(([category, items]) => ({ category, items }))
        );
      },
      error: (err: any) => this.error.set(this.extractErrorMessage(err))
    });
  }

  private extractErrorMessage(err: any): string {
    if (typeof err === 'string') return err;
    if (err?.error?.message) return err.error.message;
    if (err?.message) return err.message;
    return 'An unexpected error occurred.';
  }
}
