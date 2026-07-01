import { Component, inject, signal, OnInit, AfterViewInit, ElementRef, ViewChild, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DestroyRef } from '@angular/core';
import { finalize } from 'rxjs';
import { RoomTypeApiService } from '../../admin/services/room-type-api.service';
import { RoomType } from '../../admin/models/room-type.model';

@Component({
  selector: 'app-room-detail',
  standalone: true,
  imports: [
    CommonModule, RouterModule, MatProgressSpinnerModule
  ],
  templateUrl: './room-detail.component.html',
  styleUrls: ['./room-detail.component.scss']
})
export class RoomDetailComponent implements OnInit, AfterViewInit, OnDestroy {
  private route = inject(ActivatedRoute);
  private roomTypeApi = inject(RoomTypeApiService);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);

  room = signal<RoomType | null>(null);
  loading = signal(false);
  error = signal<string | null>(null);
  isFromBooking = signal(false);

  // Parallax
  @ViewChild('galleryContainer') galleryRef!: ElementRef<HTMLElement>;

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (id) {
      this.fetchRoom(id);
    } else {
      this.error.set('Room not found.');
    }

    const source = this.route.snapshot.queryParamMap.get('source');
    if (source === 'booking') {
      this.isFromBooking.set(true);
    }
  }

  goBackToBooking(): void {
    this.router.navigate(['/user/bookings'], { queryParams: { new: 'true' } });
  }

  ngAfterViewInit(): void {
    // Bind scroll listener for parallax after view init
    const gallery = this.galleryRef?.nativeElement;
    if (gallery) {
      gallery.addEventListener('scroll', this.onGalleryScroll);
    }
  }

  ngOnDestroy(): void {
    if (this.galleryRef?.nativeElement) {
      this.galleryRef.nativeElement.removeEventListener('scroll', this.onGalleryScroll);
    }
  }

  private fetchRoom(id: number): void {
    this.loading.set(true);
    this.roomTypeApi.getById(id).pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.loading.set(false))
    ).subscribe({
      next: (data: any) => this.room.set(data),
      error: (err: any) => this.error.set(this.extractErrorMessage(err))
    });
  }

  getBedEntries(): [string, number][] {
    const config = this.room()?.bedConfiguration;
    if (!config) return [];
    return Object.entries(config).filter(([, v]) => v > 0);
  }

  getBedIcon(bedType: string): string {
    const icons: Record<string, string> = {
      'King': 'king_bed',
      'Queen': 'bed',
      'Twin': 'single_bed',
      'Double': 'bed',
    };
    return icons[bedType] || 'bed';
  }

  checkAvailability(): void {
    const roomId = this.room()?.id;
    if (roomId) {
      // Store room type ID for later booking flow
      sessionStorage.setItem('selectedRoomTypeId', String(roomId));
      this.router.navigate(['/availability'], { queryParams: { roomTypeId: roomId } });
    }
  }

  private onGalleryScroll = (): void => {
    const gallery = this.galleryRef?.nativeElement;
    if (!gallery) return;
    const scrollLeft = gallery.scrollLeft;
    const parallaxImages = gallery.querySelectorAll('.parallax-img') as NodeListOf<HTMLElement>;
    parallaxImages.forEach((img) => {
      const speed = parseFloat(img.getAttribute('data-speed') || '0');
      const imgTag = img.querySelector('img') as HTMLImageElement;
      if (imgTag) {
        imgTag.style.transform = `translateX(${scrollLeft * speed}px) scale(1.1)`;
      }
    });
  };

  private extractErrorMessage(err: any): string {
    if (typeof err === 'string') return err;
    if (err?.error?.message) return err.error.message;
    if (err?.message) return err.message;
    return 'An unexpected error occurred.';
  }
}
