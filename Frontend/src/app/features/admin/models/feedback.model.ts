export interface Feedback {
  id: number;
  bookingId: number;
  rating: number;
  comments: string | null;
  createdAt: string; // ISO 8601
  isHidden: boolean;
}

export interface ModerateFeedbackRequest {
  isHidden: boolean;
}
