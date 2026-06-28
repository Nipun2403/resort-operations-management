import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormGroup, FormControl, Validators } from '@angular/forms';
import { MatDialogModule, MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { FeedbackApiService } from '../../services/feedback-api.service';
import { Feedback, CreateFeedbackDTO } from '../../models/feedback.model';
import { AlertComponent } from '../../../../features/auth/components/alert.component';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'app-feedback-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatSelectModule,
    MatInputModule,
    MatFormFieldModule,
    MatProgressSpinnerModule,
    AlertComponent
  ],
  templateUrl: './feedback-dialog.component.html',
  styles: [`
    .feedback-content {
      min-width: 320px;
      max-width: 480px;
    }
    .read-only-feedback {
      font-size: 1rem;
      line-height: 1.5;
    }
    .rating-value {
      font-size: 1.2rem;
      font-weight: 600;
      color: #3f51b5;
    }
    .full-width {
      width: 100%;
    }
  `]
})
export class FeedbackDialogComponent implements OnInit {
  readonly bookingId: number = inject(MAT_DIALOG_DATA);
  private readonly feedbackApi = inject(FeedbackApiService);
  private readonly dialogRef = inject(MatDialogRef<FeedbackDialogComponent>);

  existingFeedback = signal<Feedback | null>(null);
  loading = signal(false);
  submitting = signal(false);
  error = signal<string | null>(null);

  feedbackForm = new FormGroup({
    rating: new FormControl<number>(5, { validators: [Validators.required, Validators.min(1), Validators.max(5)], nonNullable: true }),
    comments: new FormControl<string>('', { nonNullable: true })
  });

  ngOnInit(): void {
    this.checkExistingFeedback();
  }

  checkExistingFeedback(): void {
    this.loading.set(true);
    this.error.set(null);

    this.feedbackApi.getByBookingId(this.bookingId)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (data) => {
          // If the backend returns 204 or null or empty object, existingFeedback will be null.
          if (data && data.id) {
            this.existingFeedback.set(data);
          } else {
            this.existingFeedback.set(null);
          }
        },
        error: (err) => {
          // A 404 might mean no feedback exists yet. Let's inspect status
          if (err.status === 404) {
            this.existingFeedback.set(null);
          } else {
            const message = err.error?.message || err.message || 'Error checking existing feedback.';
            this.error.set(message);
          }
        }
      });
  }

  submitFeedback(): void {
    if (this.feedbackForm.invalid) {
      return;
    }

    this.submitting.set(true);
    this.error.set(null);

    const dto: CreateFeedbackDTO = {
      bookingId: this.bookingId,
      rating: this.feedbackForm.value.rating!,
      comments: this.feedbackForm.value.comments ?? ''
    };

    this.feedbackApi.submit(dto)
      .pipe(finalize(() => this.submitting.set(false)))
      .subscribe({
        next: (response) => {
          this.dialogRef.close(response);
        },
        error: (err) => {
          const message = err.error?.message || err.message || 'Failed to submit feedback.';
          this.error.set(message);
        }
      });
  }
}
