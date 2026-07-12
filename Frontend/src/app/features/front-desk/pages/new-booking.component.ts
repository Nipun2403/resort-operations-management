import { Component, inject, signal, computed, ViewChild, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormGroup, FormControl, FormArray, Validators, AbstractControl } from '@angular/forms';
import { MatStepperModule, MatStepper } from '@angular/material/stepper';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { BreakpointObserver } from '@angular/cdk/layout';
import { toSignal } from '@angular/core/rxjs-interop';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { map, finalize } from 'rxjs/operators';

import { BookingApiService } from '../../user/services/booking-api.service';
import { RoomTypeApiService } from '../../user/services/room-type-api.service';
import { AmenityApiService } from '../../user/services/amenity-api.service';
import { AvailableRoomType } from '../../user/models/available-room-type.model';
import { Amenity } from '../../admin/models/amenity.model';
import { AlertComponent } from '../../auth/components/alert.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { SuccessDialogComponent } from '../components/success-dialog/success-dialog.component';

@Component({
  selector: 'app-front-desk-new-booking',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatStepperModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatSelectModule,
    MatCheckboxModule,
    MatDividerModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatDialogModule,
    AlertComponent,
  ],
  templateUrl: './new-booking.component.html',
  styleUrls: ['./new-booking.component.scss'],
})
export class FrontDeskBookingWizardComponent {
  @ViewChild('stepper') stepper!: MatStepper;

  private readonly bookingApi = inject(BookingApiService);
  private readonly roomTypeApi = inject(RoomTypeApiService);
  private readonly amenityApi = inject(AmenityApiService);
  private readonly breakpointObserver = inject(BreakpointObserver);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);
  private readonly destroyRef = inject(DestroyRef);

  isMobile = toSignal(
    this.breakpointObserver.observe('(max-width: 599px)').pipe(map(r => r.matches)),
    { initialValue: false }
  );

  // Step 1: Guest Details
  guestForm = new FormGroup({
    firstName: new FormControl('', {
      validators: [Validators.required, Validators.minLength(2), Validators.pattern(/^[a-zA-ZÀ-ž\s\-']+$/)],
      nonNullable: true
    }),
    lastName: new FormControl('', {
      validators: [Validators.required, Validators.minLength(2), Validators.pattern(/^[a-zA-ZÀ-ž\s\-']+$/)],
      nonNullable: true
    }),
    email: new FormControl('', {
      validators: [Validators.required, Validators.email],
      nonNullable: true
    }),
  });

  // Step 2: Dates & Guests
  datesForm = new FormGroup({
    checkInDate: new FormControl<Date | null>(null, [Validators.required, this.futureDateValidator]),
    checkOutDate: new FormControl<Date | null>(null, [Validators.required]),
    guestCount: new FormControl(1, [Validators.required, Validators.min(1), Validators.max(20)]),
  }, { validators: this.checkOutAfterCheckIn });

  // Step 3: Rooms
  roomsForm = new FormGroup({
    dummy: new FormControl<boolean>(false, { validators: [Validators.requiredTrue], nonNullable: true })
  });

  availableRooms = signal<AvailableRoomType[]>([]);
  selectedRoomQuantities = signal<Record<number, number>>({});
  roomsLoading = signal(false);
  roomsError = signal<string | null>(null);

  // Step 4: Amenities
  availableAmenities = signal<Amenity[]>([]);
  selectedAmenities = new FormArray<FormControl<boolean>>([]);
  amenitiesForm = new FormGroup({ amenities: this.selectedAmenities });
  amenitiesLoading = signal(false);

  // Submission
  submitting = signal(false);

  // Convert form values to signals so computed reacts
  private datesValues = toSignal(this.datesForm.valueChanges, { initialValue: this.datesForm.value });
  private amenitiesValues = toSignal(this.amenitiesForm.valueChanges, { initialValue: this.amenitiesForm.value });

  // Computed signals
  totalSelectedQuantity = computed(() => Object.values(this.selectedRoomQuantities()).reduce((a, b) => a + b, 0));

  capacityWarning = computed(() => {
    const totalCap = this.availableRooms().reduce(
      (sum, r) => sum + (this.selectedRoomQuantities()[r.roomTypeId] || 0) * r.maxOccupancy,
      0
    );
    const dates = this.datesValues();
    const guests = dates?.guestCount ?? 0;
    if (this.totalSelectedQuantity() > 0 && totalCap < guests) {
      return `The selected rooms can only accommodate ${totalCap} guests. You need ${guests}.`;
    }
    return null;
  });

  nights = computed(() => {
    const dates = this.datesValues();
    if (!dates || !dates.checkInDate || !dates.checkOutDate) return 0;
    const cin = new Date(dates.checkInDate);
    const cout = new Date(dates.checkOutDate);
    return Math.max(0, Math.ceil((cout.getTime() - cin.getTime()) / (1000 * 3600 * 24)));
  });

  estimatedTotal = computed(() => {
    const amenitiesVal = this.amenitiesValues();
    const nights = this.nights();
    const roomCost = this.availableRooms().reduce(
      (sum, r) => sum + (this.selectedRoomQuantities()[r.roomTypeId] || 0) * r.basePrice * nights,
      0
    );
    const selectedList = amenitiesVal?.amenities || [];
    const amenityCost = this.availableAmenities().reduce(
      (sum, a, i) => sum + (selectedList[i] ? a.price : 0),
      0
    );
    return roomCost + amenityCost;
  });

  selectedRoomEntries = computed(() => {
    const quantities = this.selectedRoomQuantities();
    return this.availableRooms()
      .filter(r => (quantities[r.roomTypeId] || 0) > 0)
      .map(r => ({
        roomTypeId: r.roomTypeId,
        name: r.name,
        basePrice: r.basePrice,
        maxOccupancy: r.maxOccupancy,
        quantity: quantities[r.roomTypeId]
      }));
  });

  selectedAmenityEntries = computed(() => {
    const list = this.availableAmenities();
    const amenitiesVal = this.amenitiesValues();
    const selectedList = amenitiesVal?.amenities || [];
    return list.filter((_, i) => selectedList[i] === true);
  });

  onStepChange(event: any): void {
    if (event.selectedIndex === 2) { // step 3 (0-based)
      this.fetchAvailableRooms();
    }
    if (event.selectedIndex === 3) {
      this.fetchAmenities();
    }
  }

  fetchAvailableRooms(): void {
    const cin = this.datesForm.value.checkInDate;
    const cout = this.datesForm.value.checkOutDate;
    if (!cin || !cout) return;

    this.roomsLoading.set(true);
    this.roomsError.set(null);

    this.roomTypeApi.getAvailable(this.formatDate(cin), this.formatDate(cout))
      .pipe(
        finalize(() => this.roomsLoading.set(false)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: (res) => {
          this.availableRooms.set(res.data);
          const quantities: Record<number, number> = {};
          res.data.forEach(r => {
            quantities[r.roomTypeId] = 0;
          });
          this.selectedRoomQuantities.set(quantities);
          this.updateRoomsFormValidity();
        },
        error: (err) => {
          this.roomsError.set(err.error?.message || err.message || 'Failed to load available rooms.');
        }
      });
  }

  fetchAmenities(): void {
    this.amenitiesLoading.set(true);
    this.amenityApi.getAll({ pageNumber: 1, pageSize: 100, isAvailable: true })
      .pipe(
        finalize(() => this.amenitiesLoading.set(false)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: (res) => {
          this.availableAmenities.set(res.data);
          this.selectedAmenities.clear();
          res.data.forEach(() => {
            this.selectedAmenities.push(new FormControl<boolean>(false, { nonNullable: true }));
          });
        },
        error: (err) => {
          this.snackBar.open(err.error?.message || err.message || 'Failed to load amenities.', 'Close', { duration: 5000 });
        }
      });
  }

  incrementRoom(roomTypeId: number): void {
    const current = this.selectedRoomQuantities();
    const limit = this.availableRooms().find(r => r.roomTypeId === roomTypeId)?.availableCount ?? 0;
    const val = current[roomTypeId] || 0;
    if (val < limit) {
      this.selectedRoomQuantities.set({
        ...current,
        [roomTypeId]: val + 1
      });
      this.updateRoomsFormValidity();
    }
  }

  decrementRoom(roomTypeId: number): void {
    const current = this.selectedRoomQuantities();
    const val = current[roomTypeId] || 0;
    if (val > 0) {
      this.selectedRoomQuantities.set({
        ...current,
        [roomTypeId]: val - 1
      });
      this.updateRoomsFormValidity();
    }
  }

  incrementGuests(): void {
    const current = this.datesForm.controls.guestCount.value || 1;
    if (current < 20) {
      this.datesForm.controls.guestCount.setValue(current + 1);
    }
  }

  decrementGuests(): void {
    const current = this.datesForm.controls.guestCount.value || 1;
    if (current > 1) {
      this.datesForm.controls.guestCount.setValue(current - 1);
    }
  }

  getRoomQuantity(roomTypeId: number): number {
    return this.selectedRoomQuantities()[roomTypeId] || 0;
  }

  getAmenityControl(index: number): FormControl<boolean> {
    return this.selectedAmenities.at(index) as FormControl<boolean>;
  }

  updateRoomsFormValidity(): void {
    const isValid = this.totalSelectedQuantity() > 0 && !this.capacityWarning();
    this.roomsForm.controls.dummy.setValue(isValid);
    this.roomsForm.updateValueAndValidity();
  }

  confirmBooking(): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Confirm Booking',
        message: `Create this booking? Total estimated: $${this.estimatedTotal().toFixed(2)}`
      }
    });

    dialogRef.afterClosed().pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe((confirmed) => {
      if (confirmed) {
        this.performBooking();
      }
    });
  }

  private performBooking(): void {
    this.submitting.set(true);
    const roomTypeIds: number[] = [];
    const quantities = this.selectedRoomQuantities();
    Object.keys(quantities).forEach(key => {
      const typeId = Number(key);
      const qty = quantities[typeId] || 0;
      for (let i = 0; i < qty; i++) {
        roomTypeIds.push(typeId);
      }
    });

    const amenityIds = this.selectedAmenityEntries().map(a => a.id);

    const dto = {
      roomTypeIds,
      guestCount: this.datesForm.value.guestCount!,
      checkInDate: this.datesForm.value.checkInDate!.toISOString(),
      checkOutDate: this.datesForm.value.checkOutDate!.toISOString(),
      guestName: `${this.guestForm.value.firstName} ${this.guestForm.value.lastName}`,
      guestEmail: this.guestForm.value.email!,
      amenityIds
    };

    this.bookingApi.create(dto).pipe(
      finalize(() => this.submitting.set(false)),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: (booking) => {
        const successRef = this.dialog.open(SuccessDialogComponent, {
          data: { bookingId: booking.id, guestName: booking.guestName, guestEmail: booking.guestEmail, origin: booking.origin },
          width: '400px',
        });
        successRef.afterClosed().subscribe(() => {
          this.resetWizard();
        });
      },
      error: (err) => this.snackBar.open(this.extractErrorMessage(err), 'Close', { duration: 5000 })
    });
  }

  resetWizard(): void {
    this.guestForm.reset();
    this.datesForm.reset({ guestCount: 1 });
    this.selectedRoomQuantities.set({});
    this.availableRooms.set([]);
    this.availableAmenities.set([]);
    this.selectedAmenities.clear();
    this.updateRoomsFormValidity();
    if (this.stepper) {
      this.stepper.reset();
    }
  }

  formatDate(date: Date): string {
    const day = String(date.getDate()).padStart(2, '0');
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const year = date.getFullYear();
    return `${day}-${month}-${year}`;
  }

  futureDateValidator(control: AbstractControl): { [key: string]: boolean } | null {
    const value = control.value;
    if (!value) return null;
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const dateVal = new Date(value);
    dateVal.setHours(0, 0, 0, 0);
    if (dateVal < today) {
      return { checkInInPast: true };
    }
    return null;
  }

  checkOutAfterCheckIn(control: AbstractControl): { [key: string]: boolean } | null {
    const cin = control.get('checkInDate')?.value as Date | null;
    const cout = control.get('checkOutDate')?.value as Date | null;
    if (!cin || !cout) return null;

    const cinDate = new Date(cin);
    cinDate.setHours(0, 0, 0, 0);
    const coutDate = new Date(cout);
    coutDate.setHours(0, 0, 0, 0);

    if (coutDate <= cinDate) {
      return { checkOutBeforeCheckIn: true };
    }
    return null;
  }

  private extractErrorMessage(err: any): string {
    return err?.error?.message || err?.message || 'An unexpected error occurred.';
  }
}
