import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DestroyRef } from '@angular/core';
import { forkJoin, finalize, map } from 'rxjs';
import { BreakpointObserver } from '@angular/cdk/layout';
import { toSignal } from '@angular/core/rxjs-interop';
import { MenuItemApiService } from '../../admin/services/menu-item-api.service';
import { AmenityApiService } from '../../admin/services/amenity-api.service';
import { MenuItem } from '../../admin/models/menu-item.model';
import { Amenity } from '../../admin/models/amenity.model';

@Component({
  selector: 'app-experiences',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatProgressSpinnerModule],
  templateUrl: './experiences.component.html',
  styleUrls: ['./experiences.component.scss']
})
export class ExperiencesComponent implements OnInit {
  private menuItemApi = inject(MenuItemApiService);
  private amenityApi = inject(AmenityApiService);
  private destroyRef = inject(DestroyRef);
  private breakpointObserver = inject(BreakpointObserver);

  // Menu
  menuLoading = signal(false);
  menuError = signal<string | null>(null);
  menuGroups = signal<{ category: string; items: MenuItem[] }[]>([]);
  expandedCategory = signal<string | null>(null);

  // Amenities
  amenitiesLoading = signal(false);
  amenitiesError = signal<string | null>(null);
  allAmenities = signal<Amenity[]>([]);
  amenityPageIndex = signal(0);

  // Breakpoint observation & pagination
  isMobile = toSignal(this.breakpointObserver.observe('(max-width: 768px)').pipe(map(r => r.matches)), { initialValue: false });
  itemsPerPageComputed = computed(() => this.isMobile() ? 1 : 3);
  totalAmenityPages = computed(() => Math.ceil(this.allAmenities().length / this.itemsPerPageComputed()));
  displayAmenities = computed(() => {
    const total = this.totalAmenityPages();
    const pageIndex = Math.min(this.amenityPageIndex(), Math.max(0, total - 1));
    const start = pageIndex * this.itemsPerPageComputed();
    return this.allAmenities().slice(start, start + this.itemsPerPageComputed());
  });

  // Transition state
  amenityIsTransitioning = signal(false);
  private readonly ANIMATION_DURATION = 600;

  ngOnInit(): void {
    this.fetchData();
  }

  private fetchData(): void {
    this.menuLoading.set(true);
    this.amenitiesLoading.set(true);

    const menu$ = this.menuItemApi.getAll({
      isAvailable: true,
      pageNumber: 1,
      pageSize: 200,
      sortBy: 'name',
      sortDescending: false
    }).pipe(
      map(res => {
        const groups: Record<string, MenuItem[]> = {};
        for (const item of res.data) {
          const cat = item.category || 'Other';
          if (!groups[cat]) groups[cat] = [];
          groups[cat].push(item);
        }
        return Object.entries(groups).map(([category, items]) => ({ category, items }));
      })
    );
    const amenities$ = this.amenityApi.getAll({
      isAvailable: true,
      pageNumber: 1,
      pageSize: 100,
      sortBy: 'name',
      sortDescending: false
    }).pipe(
      map(res => res.data)
    );

    forkJoin([menu$, amenities$]).pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => {
        this.menuLoading.set(false);
        this.amenitiesLoading.set(false);
      })
    ).subscribe({
      next: ([groups, amenities]) => {
        this.menuGroups.set(groups);
        this.allAmenities.set(amenities);
      },
      error: (err: any) => {
        this.menuError.set(this.extractErrorMessage(err));
        this.amenitiesError.set(this.extractErrorMessage(err));
      }
    });
  }

  // Menu accordion
  toggleCategory(category: string): void {
    this.expandedCategory.set(this.expandedCategory() === category ? null : category);
  }

  // Amenity pagination
  nextAmenityPage(): void {
    if (this.amenityPageIndex() < this.totalAmenityPages() - 1 && !this.amenityIsTransitioning()) {
      this.triggerAmenityTransition(() => this.amenityPageIndex.update(i => i + 1));
    }
  }
  prevAmenityPage(): void {
    if (this.amenityPageIndex() > 0 && !this.amenityIsTransitioning()) {
      this.triggerAmenityTransition(() => this.amenityPageIndex.update(i => i - 1));
    }
  }
  private triggerAmenityTransition(updateFn: () => void): void {
    this.amenityIsTransitioning.set(true);
    setTimeout(() => {
      updateFn();
      setTimeout(() => this.amenityIsTransitioning.set(false), this.ANIMATION_DURATION);
    }, 100);
  }

  // Touch swipe detection
  private touchStartX = 0;
  onTouchStart(event: TouchEvent): void {
    this.touchStartX = event.changedTouches[0].screenX;
  }
  onTouchEnd(event: TouchEvent): void {
    const deltaX = event.changedTouches[0].screenX - this.touchStartX;
    if (deltaX < -50) this.nextAmenityPage();
    else if (deltaX > 50) this.prevAmenityPage();
  }

  getAmenityImage(amenity: Amenity): string {
    return amenity.imageUrl ?? '';
  }

  // Amenity number label (global index)
  getAmenityNumber(index: number): string {
    const globalIndex = this.amenityPageIndex() * this.itemsPerPageComputed() + index + 1;
    return `[ ${globalIndex.toString().padStart(2, '0')} ]`;
  }

  private extractErrorMessage(err: any): string {
    if (typeof err === 'string') return err;
    if (err?.error?.message) return err.error.message;
    if (err?.message) return err.message;
    return 'An unexpected error occurred.';
  }
}
