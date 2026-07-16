import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { ReactiveFormsModule, FormGroup, FormControl, Validators } from '@angular/forms';
import { FeedbackReminderApiService } from '../services/feedback-reminder-api.service';
import { FeedbackReminderValidation } from '../models/feedback-reminder.model';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'app-feedback-submit',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './feedback-submit.component.html',
  styleUrls: ['./feedback-submit.component.scss']
})
export class FeedbackSubmitComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly feedbackReminderApi = inject(FeedbackReminderApiService);

  private token = '';

  loading = signal(true);
  submitting = signal(false);
  submitted = signal(false);
  error = signal<string | null>(null);
  validation = signal<FeedbackReminderValidation | null>(null);

  feedbackForm = new FormGroup({
    rating: new FormControl<number>(5, { validators: [Validators.required, Validators.min(1), Validators.max(5)], nonNullable: true }),
    comments: new FormControl<string>('', { nonNullable: true })
  });

  ngOnInit(): void {
    window.scrollTo(0, 0);
    this.token = this.route.snapshot.queryParamMap.get('token') ?? '';

    if (!this.token) {
      this.loading.set(false);
      this.error.set('This feedback link is missing a token.');
      return;
    }

    this.feedbackReminderApi.validateToken(this.token)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (result) => this.validation.set(result),
        error: (err) => {
          const message = err.error?.errorReason === 'expired'
            ? 'This feedback link has expired.'
            : (err.error?.message || 'This feedback link is invalid.');
          this.error.set(message);
        }
      });
  }

  submitFeedback(): void {
    if (this.feedbackForm.invalid) return;

    this.submitting.set(true);
    this.error.set(null);

    const rating = this.feedbackForm.value.rating!;
    const comments = this.feedbackForm.value.comments ?? '';

    this.feedbackReminderApi.submit(this.token, rating, comments)
      .pipe(finalize(() => this.submitting.set(false)))
      .subscribe({
        next: () => this.submitted.set(true),
        error: (err) => {
          const message = err.error?.message || err.message || 'Failed to submit feedback.';
          this.error.set(message);
        }
      });
  }
}
