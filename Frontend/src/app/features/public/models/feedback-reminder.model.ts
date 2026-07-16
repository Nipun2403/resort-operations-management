export interface FeedbackReminderValidation {
  isValid: boolean;
  bookingId: number;
  guestName: string;
  alreadySubmitted: boolean;
  errorReason: string | null;
}
