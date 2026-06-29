import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DestroyRef } from '@angular/core';
import { finalize } from 'rxjs';
import { AmenityApiService } from '../../admin/services/amenity-api.service';
import { Amenity } from '../../admin/models/amenity.model';

@Component({
  selector: 'app-public-amenities',
  standalone: true,
  imports: [
    CommonModule, MatCardModule, MatIconModule, MatProgressSpinnerModule
  ],
  templateUrl: './amenities.component.html',
  styleUrls: ['./amenities.component.scss']
})
export class AmenitiesComponent implements OnInit {
  private amenityApi = inject(AmenityApiService);
  private destroyRef = inject(DestroyRef);

  amenities = signal<Amenity[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);

  ngOnInit(): void {
    this.fetchAmenities();
  }

  fetchAmenities(): void {
    this.loading.set(true);
    this.amenityApi.getAll({ isAvailable: true, pageNumber: 1, pageSize: 200, sortBy: 'name', sortDescending: false }).pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.loading.set(false))
    ).subscribe({
      next: res => this.amenities.set(res.data),
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
