import { Component, inject, signal, computed, input, output, ChangeDetectorRef, DestroyRef, OnInit, ViewChild, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormGroup, FormControl, FormArray, Validators, AbstractControl } from '@angular/forms';
import { MatStepperModule, MatStepper } from '@angular/material/stepper';
import { Router } from '@angular/router';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { ConfirmDialogComponent } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';
import { BreakpointObserver } from '@angular/cdk/layout';
import { map } from 'rxjs/operators';
import { toSignal } from '@angular/core/rxjs-interop';
import { RoomTypeApiService } from '../../services/room-type-api.service';
import { AmenityApiService } from '../../services/amenity-api.service';
import { BookingApiService } from '../../services/booking-api.service';
import { AvailableRoomType } from '../../models/available-room-type.model';
import { Amenity } from '../../../../features/admin/models/amenity.model';
import { AlertComponent } from '../../../../features/auth/components/alert.component';
import { MatDividerModule } from '@angular/material/divider';
import { finalize } from 'rxjs/operators';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-booking-wizard',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatStepperModule,
    MatInputModule,
    MatButtonModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatCheckboxModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatDividerModule,
    MatDialogModule,
    AlertComponent
  ],
  templateUrl: './booking-wizard.component.html',
  styleUrls: ['./booking-wizard.component.scss']
})
export class BookingWizardComponent implements OnInit, AfterViewInit {
  userProfile = input.required<{ firstName: string; lastName: string; email: string }>();
  bookingCreated = output<number>();

  initialCheckIn = input<Date | null>(null);
  initialCheckOut = input<Date | null>(null);
  initialGuests = input<number | null>(null);
  initialRoomTypeId = input<number | null>(null);

  private readonly roomTypeApi = inject(RoomTypeApiService);
  private readonly amenityApi = inject(AmenityApiService);
  private readonly bookingApi = inject(BookingApiService);
  private readonly breakpointObserver = inject(BreakpointObserver);
  private readonly dialog = inject(MatDialog);
  private readonly destroyRef = inject(DestroyRef);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly router = inject(Router);

  @ViewChild('stepper') stepper!: MatStepper;

  private restoredState: any = null;

  isMobile = toSignal(
    this.breakpointObserver.observe('(max-width: 767px)').pipe(map(r => r.matches)),
    { initialValue: false }
  );

  private initialRoomApplied = false;

  ngOnInit(): void {
    const savedStateStr = sessionStorage.getItem('bookingWizardState');
    if (savedStateStr) {
      try {
        this.restoredState = JSON.parse(savedStateStr);
        sessionStorage.removeItem('bookingWizardState');
        if (this.restoredState.dates) {
          const restoredDates = {
            checkInDate: this.restoredState.dates.checkInDate ? new Date(this.restoredState.dates.checkInDate) : null,
            checkOutDate: this.restoredState.dates.checkOutDate ? new Date(this.restoredState.dates.checkOutDate) : null,
            guestCount: this.restoredState.dates.guestCount
          };
          this.datesForm.patchValue(restoredDates);
          this.loadRooms();
        }
      } catch {
        /* sessionStorage parse failed — use defaults */
      }
    } else if (this.initialCheckIn() && this.initialCheckOut() && this.initialGuests()) {
      this.datesForm.patchValue({
        checkInDate: this.initialCheckIn(),
        checkOutDate: this.initialCheckOut(),
        guestCount: this.initialGuests() ?? 1
      });
      this.loadRooms();
    }
  }

  ngAfterViewInit(): void {
    if (this.restoredState && typeof this.restoredState.step === 'number') {
      setTimeout(() => {
        this.stepper.selectedIndex = this.restoredState.step;
        if (this.restoredState.step === 2) {
          this.loadAmenities();
        }
        this.cdr.detectChanges();
      });
    }
  }

  loading = signal(false);
  error = signal<string | null>(null);

  availableRooms = signal<AvailableRoomType[]>([]);
  availableAmenities = signal<Amenity[]>([]);
  selectedRoomQuantities = signal<Record<number, number>>({});

  // Forms definition
  datesForm = new FormGroup({
    checkInDate: new FormControl<Date | null>(null, { validators: [Validators.required] }),
    checkOutDate: new FormControl<Date | null>(null, { validators: [Validators.required] }),
    guestCount: new FormControl<number>(1, { validators: [Validators.required, Validators.min(1), Validators.max(20)], nonNullable: true })
  }, { validators: this.dateRangeValidator });

  roomsForm = new FormGroup({
    dummy: new FormControl<boolean>(false, { validators: [Validators.requiredTrue], nonNullable: true })
  });

  amenitiesForm = new FormGroup({
    selectedAmenities: new FormArray<FormControl<boolean>>([])
  });

  get amenityControls(): FormControl<boolean>[] {
    return (this.amenitiesForm.get('selectedAmenities') as FormArray).controls as FormControl<boolean>[];
  }

  getAmenityControl(index: number): FormControl<boolean> {
    return this.amenityControls[index];
  }

  // Convert form values to signals so computed reacts
  private datesValues = toSignal(this.datesForm.valueChanges, { initialValue: this.datesForm.value });
  private amenitiesValues = toSignal(this.amenitiesForm.valueChanges, { initialValue: this.amenitiesForm.value });

  // Computed signals
  nights = computed(() => {
    const dates = this.datesValues();
    if (!dates || !dates.checkInDate || !dates.checkOutDate) return 0;
    const cin = new Date(dates.checkInDate);
    const cout = new Date(dates.checkOutDate);
    return Math.max(0, Math.ceil((cout.getTime() - cin.getTime()) / (1000 * 3600 * 24)));
  });

  totalSelectedQuantity = computed(() => {
    return Object.values(this.selectedRoomQuantities()).reduce((a, b) => a + b, 0);
  });

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
    const selectedList = amenitiesVal?.selectedAmenities || [];
    return list.filter((_, i) => selectedList[i] === true);
  });

  estimatedTotal = computed(() => {
    const amenitiesVal = this.amenitiesValues();
    const nights = this.nights();
    const roomCost = this.availableRooms().reduce(
      (sum, r) => sum + (this.selectedRoomQuantities()[r.roomTypeId] || 0) * r.basePrice * nights,
      0
    );
    const selectedList = amenitiesVal?.selectedAmenities || [];
    const amenityCost = this.availableAmenities().reduce(
      (sum, a, i) => sum + (selectedList[i] ? a.price : 0),
      0
    );
    return roomCost + amenityCost;
  });

  onStepChange(event: any): void {
    if (event.selectedIndex === 1) {
      this.loadRooms();
    } else if (event.selectedIndex === 2) {
      this.loadAmenities();
    }
  }

  loadRooms(): void {
    const cin = this.datesForm.value.checkInDate;
    const cout = this.datesForm.value.checkOutDate;
    if (!cin || !cout) return;

    this.loading.set(true);
    this.error.set(null);

    this.roomTypeApi.getAvailable(this.formatDate(cin), this.formatDate(cout))
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (res) => {
          this.availableRooms.set(res.data);
          // Pre-populate empty quantities
          const quantities: Record<number, number> = {};
          res.data.forEach(r => {
            quantities[r.roomTypeId] = 0;
          });

          if (this.restoredState && this.restoredState.quantities) {
            Object.keys(this.restoredState.quantities).forEach(key => {
              const rId = Number(key);
              if (quantities[rId] !== undefined) {
                const room = res.data.find(r => r.roomTypeId === rId);
                const limit = room ? room.availableCount : 0;
                quantities[rId] = Math.min(this.restoredState.quantities[key], limit);
              }
            });
          } else if (!this.initialRoomApplied && this.initialRoomTypeId()) {
            const room = res.data.find(r => r.roomTypeId === this.initialRoomTypeId());
            if (room && room.availableCount > 0) {
              quantities[room.roomTypeId] = 1;
              this.initialRoomApplied = true;
            }
          }

          this.selectedRoomQuantities.set(quantities);
          this.updateRoomsFormValidity();
        },
        error: (err) => {
          const message = err.error?.message || err.message || 'Failed to load available rooms.';
          this.error.set(message);
        }
      });
  }

  goToRoomDetails(roomId: number): void {
    const state = {
      dates: this.datesForm.value,
      quantities: this.selectedRoomQuantities(),
      step: this.stepper?.selectedIndex ?? 1
    };
    sessionStorage.setItem('bookingWizardState', JSON.stringify(state));
    this.router.navigate(['/rooms', roomId], { queryParams: { source: 'booking' } });
  }

  loadAmenities(): void {
    this.loading.set(true);
    this.error.set(null);

    this.amenityApi.getAll({ pageNumber: 1, pageSize: 100, isAvailable: true })
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (res) => {
          this.availableAmenities.set(res.data);
          const formArray = this.amenitiesForm.get('selectedAmenities') as FormArray;
          formArray.clear();
          res.data.forEach(() => {
            formArray.push(new FormControl<boolean>(false, { nonNullable: true }));
          });
          this.cdr.detectChanges();
        },
        error: (err) => {
          const message = err.error?.message || err.message || 'Failed to load amenities.';
          this.error.set(message);
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

  getRoomQuantity(roomTypeId: number): number {
    return this.selectedRoomQuantities()[roomTypeId] || 0;
  }

  updateRoomsFormValidity(): void {
    const isValid = this.totalSelectedQuantity() > 0 && !this.capacityWarning();
    this.roomsForm.controls.dummy.setValue(isValid);
    this.roomsForm.updateValueAndValidity();
  }

  submitBooking(): void {
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
    this.loading.set(true);
    this.error.set(null);

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
    const profile = this.userProfile();

    const bookingDto = {
      roomTypeIds,
      guestCount: this.datesForm.value.guestCount!,
      checkInDate: this.datesForm.value.checkInDate!.toISOString(),
      checkOutDate: this.datesForm.value.checkOutDate!.toISOString(),
      guestName: `${profile.firstName} ${profile.lastName}`,
      guestEmail: profile.email,
      amenityIds
    };

    this.bookingApi.create(bookingDto)
      .pipe(
        finalize(() => this.loading.set(false)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: (response) => {
          this.bookingCreated.emit(response.id);
        },
        error: (err) => {
          const message = err.error?.message || err.message || 'Failed to confirm booking.';
          this.error.set(message);
        }
      });
  }

  formatDate(date: Date): string {
    const day = String(date.getDate()).padStart(2, '0');
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const year = date.getFullYear();
    return `${day}-${month}-${year}`;
  }

  private dateRangeValidator(control: AbstractControl): { [key: string]: boolean } | null {
    const cin = control.get('checkInDate')?.value as Date | null;
    const cout = control.get('checkOutDate')?.value as Date | null;
    if (!cin || !cout) return null;

    const today = new Date();
    today.setHours(0, 0, 0, 0);

    const cinDate = new Date(cin);
    cinDate.setHours(0, 0, 0, 0);

    if (cinDate < today) {
      return { checkInInPast: true };
    }

    const coutDate = new Date(cout);
    coutDate.setHours(0, 0, 0, 0);

    if (coutDate <= cinDate) {
      return { checkOutBeforeCheckIn: true };
    }

    return null;
  }
}
