export type { Feedback } from '../../../features/admin/models/feedback.model';

export interface CreateFeedbackDTO {
  bookingId: number;
  rating: number;
  comments: string;
}
